using System.Collections.Generic;
using System.Linq;

namespace proiect_poo
{
    public class GameState
    {
        public string CurrentBlockId { get; set; }
        public List<Status> ToateStatusurile { get; private set; } = new List<Status>();
        public StoryJsonDefinition PovesteIncarcata { get; private set; }

        // ideaId → nivel curent de research (0 = deblocată dar neresearch-uită)
        public Dictionary<string, int> IdeaResearchLevels { get; private set; } = new Dictionary<string, int>();

        private int _deciziiLuateInBlocCurent = 0;

        // =====================================================================
        // INIT
        // =====================================================================
        public void InitializareJoc(StoryJsonDefinition poveste)
        {
            PovesteIncarcata = poveste;
            ToateStatusurile.Clear();
            IdeaResearchLevels.Clear();
            _deciziiLuateInBlocCurent = 0;

            if (poveste.Properties != null)
                foreach (var propDef in poveste.Properties)
                    ToateStatusurile.Add(new Status(propDef));

            CurrentBlockId = poveste.StartBlock;
        }

        // =====================================================================
        // IDEAS
        // =====================================================================
        public void UnlockIdea(string ideaId)
        {
            if (!string.IsNullOrEmpty(ideaId) && !IdeaResearchLevels.ContainsKey(ideaId))
                IdeaResearchLevels[ideaId] = 0;
        }

        // Research: avansează nivelul ideii, aplică innovationAdded + stressCost din nivelul următor
        public void ResearchIdea(string ideaId, int decisionsRequired, string nextBlock)
        {
            var idea = PovesteIncarcata.Ideas?.FirstOrDefault(i => i.Id == ideaId);
            if (idea == null) return;

            int currentLevel = IdeaResearchLevels.ContainsKey(ideaId) ? IdeaResearchLevels[ideaId] : 0;
            var nextLevelDef = idea.ResearchLevels.FirstOrDefault(l => l.Level == currentLevel + 1);
            if (nextLevelDef == null) return; // deja la nivel maxim

            IdeaResearchLevels[ideaId] = nextLevelDef.Level;

            ModificaStatus("innovation", nextLevelDef.InnovationAdded);
            ModificaStatus("stres", nextLevelDef.StressCost);

            PostActionAdvance(decisionsRequired, nextBlock);
        }

        // Implement: aplică progressAdded de la nivelul curent al ideii
        public void ImplementIdea(string ideaId, int decisionsRequired, string nextBlock)
        {
            var idea = PovesteIncarcata.Ideas?.FirstOrDefault(i => i.Id == ideaId);
            if (idea == null) return;

            int currentLevel = IdeaResearchLevels.ContainsKey(ideaId) ? IdeaResearchLevels[ideaId] : 0;
            if (currentLevel == 0) return; // nicio research făcută

            var levelDef = idea.ResearchLevels.FirstOrDefault(l => l.Level == currentLevel);
            if (levelDef == null) return;

            ModificaStatus("progress", levelDef.ProgressAdded);
            ModificaStatus("stres", levelDef.StressCost);

            PostActionAdvance(decisionsRequired, nextBlock);
        }

        // Helper: poate fi apelat din Form1 ca să știe dacă un buton de research mai are sens
        public ResearchLevelJsonDefinition GetNextResearchLevel(string ideaId)
        {
            var idea = PovesteIncarcata.Ideas?.FirstOrDefault(i => i.Id == ideaId);
            if (idea == null) return null;

            int currentLevel = IdeaResearchLevels.ContainsKey(ideaId) ? IdeaResearchLevels[ideaId] : 0;
            return idea.ResearchLevels.FirstOrDefault(l => l.Level == currentLevel + 1);
        }

        // =====================================================================
        // DECIZII NORMALE
        // =====================================================================
        public void AplicaEfecteDecizie(DecisionJsonDefinition decizie, int decisionsRequired)
        {
            // 1. Aplică efectele din lista de effects
            if (decizie.Effects != null)
            {
                foreach (var efect in decizie.Effects)
                {
                    var status = ToateStatusurile.FirstOrDefault(s => s.Key == efect.Property);
                    if (status == null) continue;

                    switch (efect.Type?.ToUpper())
                    {
                        case "SET": status.SetValoare(efect.Value); break;
                        case "ADD": default: status.Modifica(efect.Value); break;
                    }
                }
            }

            // 2. Deblochează idee dacă e setat
            UnlockIdea(decizie.UnlocksIdeaId);

            PostActionAdvance(decisionsRequired, decizie.TargetBlock);
        }

        // =====================================================================
        // QUERY HELPERS
        // =====================================================================
        public BlockJsonDefinition GasesteBlocDupaId(string idBloc)
        {
            if (PovesteIncarcata?.Days == null) return null;
            foreach (var zi in PovesteIncarcata.Days)
            {
                var bloc = zi.Blocks.FirstOrDefault(b => b.Id == idBloc);
                if (bloc != null) return bloc;
            }
            return null;
        }

        public DayJsonDefinition ZiuaCurenta()
        {
            if (PovesteIncarcata?.Days == null) return null;
            return PovesteIncarcata.Days
                .FirstOrDefault(zi => zi.Blocks.Any(b => b.Id == CurrentBlockId));
        }

        // =====================================================================
        // PRIVATE HELPERS
        // =====================================================================

        // Logică comună după orice acțiune: incrementează contorul, verifică redirect-uri,
        // avansează la blocul următor dacă s-au făcut suficiente decizii
        private void PostActionAdvance(int decisionsRequired, string targetBlock)
        {
            _deciziiLuateInBlocCurent++;

            // Redirect forțat dacă un status a atins extrema
            foreach (var status in ToateStatusurile)
            {
                string redirect = status.VerificaRedirectionare();
                if (redirect != null)
                {
                    _deciziiLuateInBlocCurent = 0;
                    CurrentBlockId = redirect;
                    return;
                }
            }

            // Avansăm dacă am îndeplinit numărul de decizii cerute
            if (decisionsRequired == 0 || _deciziiLuateInBlocCurent >= decisionsRequired)
            {
                _deciziiLuateInBlocCurent = 0;
                CurrentBlockId = targetBlock;
            }
            // Altfel rămânem în bloc — interfața se va reîmprospăta cu statusurile actualizate
        }

        private void ModificaStatus(string key, int delta)
        {
            var status = ToateStatusurile.FirstOrDefault(s => s.Key == key);
            status?.Modifica(delta);
        }
    }
}
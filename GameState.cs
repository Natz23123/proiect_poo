using System;
using System.Collections.Generic;
using System.Linq;

namespace proiect_poo
{
    public class GameState
    {
        public string CurrentBlockId { get; set; }
        public List<Status> ToateStatusurile { get; private set; } = new List<Status>();
        public StoryJsonDefinition PovesteIncarcata { get; private set; }

        // ideaId → nivel curent de research FINALIZAT (0 = deblocată dar neresearch-uită)
        public Dictionary<string, int> IdeaResearchLevels { get; private set; } = new Dictionary<string, int>();

        // ideaId → nivelul MAXIM permis de research (se mărește prin decizii speciale)
        public Dictionary<string, int> IdeaMaxAllowedLevels { get; private set; } = new Dictionary<string, int>();

        // NOU: ideaId → nivelul curent IMPLEMENTAT (previne implementarea repetată a aceluiași nivel)
        public Dictionary<string, int> IdeaImplementationLevels { get; private set; } = new Dictionary<string, int>();

        private int _deciziiLuateInBlocCurent = 0;

        // =====================================================================
        // INIT
        // =====================================================================
        public void InitializareJoc(StoryJsonDefinition poveste)
        {
            PovesteIncarcata = poveste;
            ToateStatusurile.Clear();
            IdeaResearchLevels.Clear();
            IdeaMaxAllowedLevels.Clear();
            IdeaImplementationLevels.Clear(); // MODIFICARE: Resetăm și lista de implementări la început de joc
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
            if (!string.IsNullOrEmpty(ideaId))
            {
                if (!IdeaResearchLevels.ContainsKey(ideaId))
                    IdeaResearchLevels[ideaId] = 0;

                // Când deblochezi o idee nouă, ai voie să îi dai research doar până la nivelul 1
                if (!IdeaMaxAllowedLevels.ContainsKey(ideaId))
                    IdeaMaxAllowedLevels[ideaId] = 1;
            }
        }

        public void ResearchIdea(string ideaId, int decisionsRequired, string nextBlock)
        {
            var nextLevelDef = GetNextResearchLevel(ideaId);
            if (nextLevelDef == null) return;

            IdeaResearchLevels[ideaId] = nextLevelDef.Level;

            ModificaStatus("innovation", nextLevelDef.InnovationAdded);
            ModificaStatus("stres", nextLevelDef.StressCost);

            PostActionAdvance(decisionsRequired, nextBlock);
        }

        public void ImplementIdea(string ideaId, int decisionsRequired, string nextBlock)
        {
            var idea = PovesteIncarcata.Ideas?.FirstOrDefault(i => i.Id == ideaId);
            if (idea == null) return;

            int currentLevel = IdeaResearchLevels.ContainsKey(ideaId) ? IdeaResearchLevels[ideaId] : 0;
            if (currentLevel == 0) return;

            // MODIFICARE/VERIFICARE: Dacă am implementat deja acest nivel (sau unul mai mare), nu facem nimic
            if (IdeaImplementationLevels.TryGetValue(ideaId, out int implLevel) && implLevel >= currentLevel)
                return;

            var levelDef = idea.ResearchLevels.FirstOrDefault(l => l.Level == currentLevel);
            if (levelDef == null) return;

            ModificaStatus("progress", levelDef.ProgressAdded);
            ModificaStatus("stres", levelDef.StressCost);

            // MODIFICARE: Salvăm faptul că am implementat acest nivel, ca să nu îl mai repetăm
            IdeaImplementationLevels[ideaId] = currentLevel;

            PostActionAdvance(decisionsRequired, nextBlock);
        }

        // Returnează următorul nivel DOAR DACĂ nu s-a atins încă limita permisă
        public ResearchLevelJsonDefinition GetNextResearchLevel(string ideaId)
        {
            var idea = PovesteIncarcata.Ideas?.FirstOrDefault(i => i.Id == ideaId);
            if (idea == null) return null;

            int currentLevel = IdeaResearchLevels.ContainsKey(ideaId) ? IdeaResearchLevels[ideaId] : 0;
            int maxAllowed = IdeaMaxAllowedLevels.ContainsKey(ideaId) ? IdeaMaxAllowedLevels[ideaId] : 0;

            // Verificarea de barieră: Nu putem trece de nivelul permis!
            if (currentLevel >= maxAllowed) return null;

            return idea.ResearchLevels.FirstOrDefault(l => l.Level == currentLevel + 1);
        }

        // =====================================================================
        // DECIZII NORMALE
        // =====================================================================
        public void AplicaEfecteDecizie(DecisionJsonDefinition decizie, int decisionsRequired)
        {
            if (decizie.Effects != null)
            {
                foreach (var efect in decizie.Effects)
                {
                    // Tratăm cazul special de deblocare a unui nivel nou de research
                    if (efect.Type?.ToUpper() == "UNLOCK_LEVEL")
                    {
                        // efect.Property = ID-ul ideii, efect.Value = Nivelul pe care îl deblocăm
                        if (IdeaMaxAllowedLevels.ContainsKey(efect.Property))
                        {
                            if (IdeaMaxAllowedLevels[efect.Property] < efect.Value)
                                IdeaMaxAllowedLevels[efect.Property] = efect.Value;
                        }
                        else
                        {
                            // FIX: Dacă ideea nu e în dicționar, o salvăm direct cu noul nivel permis
                            IdeaMaxAllowedLevels[efect.Property] = efect.Value;
                        }
                        continue;
                    }

                    // Logica normală de statusuri
                    var status = ToateStatusurile.FirstOrDefault(s => s.Key == efect.Property);
                    if (status == null) continue;

                    switch (efect.Type?.ToUpper())
                    {
                        case "SET": status.SetValoare(efect.Value); break;
                        case "ADD": default: status.Modifica(efect.Value); break;
                    }
                }
            }

            UnlockIdea(decizie.UnlocksIdeaId);
            PostActionAdvance(decisionsRequired, decizie.TargetBlock);
        }

        // =====================================================================
        // QUERY & HELPERS 
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
            return PovesteIncarcata.Days.FirstOrDefault(zi => zi.Blocks.Any(b => b.Id == CurrentBlockId));
        }

        private void PostActionAdvance(int decisionsRequired, string targetBlock)
        {
            _deciziiLuateInBlocCurent++;

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

            if (decisionsRequired == 0 || _deciziiLuateInBlocCurent >= decisionsRequired)
            {
                _deciziiLuateInBlocCurent = 0;
                CurrentBlockId = targetBlock;
            }
        }

        private void ModificaStatus(string key, int delta)
        {
            var status = ToateStatusurile.FirstOrDefault(s => s.Key == key);
            status?.Modifica(delta);
        }
    }
}
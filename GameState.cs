using System.Collections.Generic;
using System.Linq;

namespace proiect_poo
{
    public class GameState
    {
        public string CurrentBlockId { get; set; }
        public List<Status> ToateStatusurile { get; private set; } = new List<Status>();
        public StoryJsonDefinition PovesteIncarcata { get; private set; }

        private int _deciziiLuateInBlocCurent = 0;

        public void InitializareJoc(StoryJsonDefinition poveste)
        {
            PovesteIncarcata = poveste;
            ToateStatusurile.Clear();
            _deciziiLuateInBlocCurent = 0;

            if (poveste.Properties != null)
            {
                foreach (var propDef in poveste.Properties)
                    ToateStatusurile.Add(new Status(propDef));
            }

            CurrentBlockId = poveste.StartBlock;
        }

        public void AplicaEfecteDecizie(DecisionJsonDefinition decizie, int decisionsRequired)
        {
            // 1. Aplicăm toate efectele deciziei
            if (decizie.Effects != null)
            {
                foreach (var efect in decizie.Effects)
                {
                    Status statusGasit = ToateStatusurile.FirstOrDefault(s => s.Key == efect.Property);
                    if (statusGasit == null) continue;

                    switch (efect.Type?.ToUpper())
                    {
                        case "SET":
                            statusGasit.SetValoare(efect.Value);
                            break;
                        case "ADD":
                        default:
                            statusGasit.Modifica(efect.Value);
                            break;
                    }
                }
            }

            _deciziiLuateInBlocCurent++;

            // 2. Verificăm dacă vreun status a atins limita — redirect imediat, indiferent de deciziiLuate
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

            // 3. Verificăm dacă am făcut suficiente decizii pentru a avansa la blocul următor
            // decisionsRequired == 0 înseamnă că fiecare decizie avansează imediat
            if (decisionsRequired == 0 || _deciziiLuateInBlocCurent >= decisionsRequired)
            {
                _deciziiLuateInBlocCurent = 0;
                CurrentBlockId = decizie.TargetBlock;
            }
            // Altfel rămânem în blocul curent cu statusurile actualizate
        }

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
    }
}
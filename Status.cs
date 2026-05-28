namespace proiect_poo
{
    public class Status
    {
        public string Key { get; private set; }
        public string Nume { get; private set; }
        public int Valoare { get; private set; }

        public int Min { get; private set; }
        public int Max { get; private set; }

        public string OnMinBlock { get; private set; }
        public string OnMaxBlock { get; private set; }
        public bool VisibleInHud { get; private set; }
        public int HudOrder { get; private set; }

        public Status(PropertyJsonDefinition def)
        {
            Key = def.Key;
            Nume = def.HudLabel;
            Valoare = def.Initial;
            Min = def.Min;
            Max = def.Max;
            OnMinBlock = def.OnMinBlock;
            OnMaxBlock = def.OnMaxBlock;
            VisibleInHud = def.VisibleInHud;
            HudOrder = def.HudOrder;
        }

        // Adaugă sau scade din valoare, respectând limitele
        public void Modifica(int delta)
        {
            Valoare = Clamp(Valoare + delta);
        }

        // Setează valoarea direct, respectând limitele
        public void SetValoare(int valoareNoua)
        {
            Valoare = Clamp(valoareNoua);
        }

        private int Clamp(int val)
        {
            if (val > Max) return Max;
            if (val < Min) return Min;
            return val;
        }

        // Întoarce ID-ul blocului de redirect dacă valoarea a atins un extrem, altfel null
        public string VerificaRedirectionare()
        {
            if (Valoare == Max && !string.IsNullOrEmpty(OnMaxBlock))
                return OnMaxBlock;

            if (Valoare == Min && !string.IsNullOrEmpty(OnMinBlock))
                return OnMinBlock;

            return null;
        }
    }
}
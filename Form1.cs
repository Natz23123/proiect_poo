using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace proiect_poo
{
    public partial class Form1 : Form
    {
        private TextAnimation _textAnimation;
        private GameState _gameState;
        private ToolTip _tooltip = new ToolTip();

        public Form1()
        {
            InitializeComponent();
            _textAnimation = new TextAnimation(lblTextHolder, 25);
            _gameState = new GameState();
            IncarcaPovesteDinJson("default_story.json");
        }

        private void IncarcaPovesteDinJson(string caleFisier)
        {
            try
            {
                if (!File.Exists(caleFisier))
                {
                    MessageBox.Show($"Fișierul '{caleFisier}' nu a fost găsit!\n" +
                                    $"Asigură-te că l-ai copiat în folderul 'bin/Debug' al proiectului tău.");
                    return;
                }
                StoryJsonDefinition poveste = JsonManager.IncarcaPoveste(caleFisier);
                _gameState.InitializareJoc(poveste);
                ActualizeazaInterfata();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare critică la încărcarea jocului: " + ex.Message);
            }
        }

        private void ActualizeazaInterfata()
        {
            var blocCurent = _gameState.GasesteBlocDupaId(_gameState.CurrentBlockId);
            if (blocCurent == null)
            {
                MessageBox.Show($"Eroare: Blocul '{_gameState.CurrentBlockId}' nu există în JSON.");
                return;
            }

            var ziCurenta = _gameState.ZiuaCurenta();
            this.Text = ziCurenta != null
                ? $"{_gameState.PovesteIncarcata.Title} | {ziCurenta.Name}"
                : _gameState.PovesteIncarcata.Title;

            _textAnimation.StartAnimation(blocCurent.Text);
            createStatusHud();
            createButtons(blocCurent);
        }

        // =====================================================================
        // HUD
        // =====================================================================
        private void createStatusHud()
        {
            panelHUD.Controls.Clear();

            var statusuriDeAfisat = _gameState.ToateStatusurile
                .Where(s => s.VisibleInHud);

            int index = 0;
            foreach (var status in statusuriDeAfisat)
            {
                FlowLayoutPanel cutie = new FlowLayoutPanel();
                cutie.FlowDirection = FlowDirection.TopDown;
                cutie.WrapContents = false;
                cutie.AutoSize = true;
                cutie.Margin = new Padding(0, 0, 0, 4);

                Label lbl = new Label();
                lbl.Text = $"{status.Nume}: {status.Valoare}";
                lbl.AutoSize = true;

                ProgressBar pb = new ProgressBar();
                pb.Minimum = status.Min;
                pb.Maximum = status.Max;
                pb.Value = status.Valoare;

                bool isPrimary = index == 0;
                lbl.Font = new Font("Segoe UI", isPrimary ? 12 : 10, FontStyle.Bold);
                lbl.Margin = new Padding(5, 0, 5, 0);
                pb.Size = new Size(isPrimary ? 110 : 60, isPrimary ? 14 : 8);
                pb.Margin = new Padding(5, 1, 5, 0);

                cutie.Controls.Add(lbl);
                cutie.Controls.Add(pb);
                panelHUD.Controls.Add(cutie);
                index++;
            }
        }

        // =====================================================================
        // BUTOANE
        // =====================================================================
        private void createButtons(BlockJsonDefinition blocCurent)
        {
            panelButoane.Controls.Clear();
            _tooltip.RemoveAll();
            _tooltip.InitialDelay = 300;
            _tooltip.AutoPopDelay = 5000;

            string blockType = blocCurent.BlockType ?? "normal";

            // --- Decizii normale (prezente indiferent de tipul blocului) ---
            foreach (var decizie in blocCurent.Decisions)
            {
                if (decizie.Condition != null && !decizie.Condition.Evaluate(_gameState.ToateStatusurile))
                    continue;

                var btn = BuildButton(decizie.Text, BuildNormalTooltip(decizie));
                var decizieCapturata = decizie;
                int decisionsRequired = blocCurent.DecisionsRequired;

                btn.Click += (s, e) =>
                {
                    _gameState.AplicaEfecteDecizie(decizieCapturata, decisionsRequired);
                    ActualizeazaInterfata();
                };
                panelButoane.Controls.Add(btn);
            }

            // --- Buton agregat RESEARCH ---
            // Apare în blockType "research", dar NUMAI dacă există cel puțin o idee researchabilă
            if (blockType == "research")
            {
                bool areIdeiDeResearch = _gameState.IdeaResearchLevels.Keys
                    .Any(id => _gameState.GetNextResearchLevel(id) != null);

                if (areIdeiDeResearch)
                {
                    int decisionsReqR = blocCurent.DecisionsRequired;
                    string nextBlockR = blocCurent.NextBlock;

                    var btnResearch = BuildButton(
                        "Dă research la o idee →",
                        "Deschide lista de idei disponibile pentru research.");

                    btnResearch.Click += (s, e) =>
                    {
                        var optiuni = BuildResearchOptions(decisionsReqR, nextBlockR);
                        ShowIdeaPickerDialog("Research — alege o idee", optiuni);
                    };
                    panelButoane.Controls.Add(btnResearch);
                }

                // --- Buton agregat IMPLEMENT (vizibil și din blocul de research) ---
                // Apare dacă există cel puțin o idee cu research level >= 1
                bool areIdeiDeImplementat = _gameState.IdeaResearchLevels.Any(kv => kv.Value >= 1);
                if (areIdeiDeImplementat)
                {
                    int decisionsReqI = blocCurent.DecisionsRequired;
                    string nextBlockI = blocCurent.NextBlock;

                    var btnImpl = BuildButton(
                        "Implementează o idee →",
                        "Deschide lista de idei gata de implementat.");

                    btnImpl.Click += (s, e) =>
                    {
                        var optiuni = BuildImplementOptions(decisionsReqI, nextBlockI);
                        ShowIdeaPickerDialog("Implementare — alege o idee", optiuni);
                    };
                    panelButoane.Controls.Add(btnImpl);
                }
            }
        }

        // =====================================================================
        // POPUP PICKER
        // =====================================================================

        // Deschide o fereastră modală cu butoane pentru fiecare opțiune
        // Fiecare opțiune are: textul butonului, textul tooltip-ului și acțiunea la click
        private void ShowIdeaPickerDialog(string titlu, List<(string label, string tooltip, Action actiune)> optiuni)
        {
            Form popup = new Form();
            popup.Text = titlu;
            popup.Size = new Size(440, 80 + optiuni.Count * 55);
            popup.StartPosition = FormStartPosition.CenterParent;
            popup.FormBorderStyle = FormBorderStyle.FixedDialog;
            popup.MaximizeBox = false;
            popup.MinimizeBox = false;

            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.FlowDirection = FlowDirection.TopDown;
            panel.Padding = new Padding(10);
            panel.AutoScroll = true;

            foreach (var (label, tooltipText, actiune) in optiuni)
            {
                Button btn = new Button();
                btn.Text = label;
                btn.Size = new Size(400, 45);
                btn.Font = new Font("Segoe UI", 9, FontStyle.Regular);
                _tooltip.SetToolTip(btn, tooltipText);

                btn.Click += (s, e) =>
                {
                    popup.Close();
                    actiune();         // actiunea apelează deja ActualizeazaInterfata()
                };
                panel.Controls.Add(btn);
            }

            popup.Controls.Add(panel);
            popup.ShowDialog(this);
        }

        // Construiește lista de opțiuni pentru research (câte una per idee researchabilă)
        private List<(string, string, Action)> BuildResearchOptions(int decisionsRequired, string nextBlock)
        {
            var lista = new List<(string, string, Action)>();

            foreach (var kv in _gameState.IdeaResearchLevels)
            {
                string ideaId = kv.Key;
                var nextLvl = _gameState.GetNextResearchLevel(ideaId);
                if (nextLvl == null) continue;

                var idea = _gameState.PovesteIncarcata.Ideas?.FirstOrDefault(i => i.Id == ideaId);
                if (idea == null) continue;

                // Construim tooltip-ul din lista de efecte
                string tooltipText = "Efecte:\n";
                if (nextLvl.Effects != null && nextLvl.Effects.Count > 0)
                {
                    foreach (var ef in nextLvl.Effects)
                    {
                        if (ef.Type?.ToUpper() == "ADD")
                        {
                            var status = _gameState.ToateStatusurile.FirstOrDefault(s => s.Key == ef.Property);
                            string nume = status != null ? status.Nume : ef.Property;
                            string semn = ef.Value >= 0 ? "+" : "";
                            tooltipText += $"{nume}: {semn}{ef.Value}\n";
                        }
                        // poți adăuga și alte tipuri de efecte dacă există
                    }
                }
                else
                {
                    tooltipText = "Fără efecte.";
                }

                string capturedId = ideaId;
                lista.Add((
                    $"{idea.Name}  —  Nivel {nextLvl.Level}: {nextLvl.Description}",
                    tooltipText,
                    () =>
                    {
                        _gameState.ResearchIdea(capturedId, decisionsRequired, nextBlock);
                        ActualizeazaInterfata();
                    }
                ));
            }
            return lista;
        }

        // Construiește lista de opțiuni pentru implement (câte una per idee cu nivel >= 1)
        private List<(string, string, Action)> BuildImplementOptions(int decisionsRequired, string nextBlock)
        {
            var lista = new List<(string, string, Action)>();

            foreach (var kv in _gameState.IdeaResearchLevels.Where(x => x.Value >= 1))
            {
                string ideaId = kv.Key;
                int level = kv.Value;

                // Verificăm dacă deja a fost implementat acest nivel
                if (_gameState.IdeaImplementationLevels.TryGetValue(ideaId, out int implLevel) && implLevel >= level)
                    continue;

                var idea = _gameState.PovesteIncarcata.Ideas?.FirstOrDefault(i => i.Id == ideaId);
                var levelDef = idea?.ResearchLevels.FirstOrDefault(l => l.Level == level);
                if (idea == null || levelDef == null) continue;

                // Tooltip din efecte
                string tooltipText = "Efecte:\n";
                if (levelDef.Effects != null && levelDef.Effects.Count > 0)
                {
                    foreach (var ef in levelDef.Effects)
                    {
                        if (ef.Type?.ToUpper() == "ADD")
                        {
                            var status = _gameState.ToateStatusurile.FirstOrDefault(s => s.Key == ef.Property);
                            string nume = status != null ? status.Nume : ef.Property;
                            string semn = ef.Value >= 0 ? "+" : "";
                            tooltipText += $"{nume}: {semn}{ef.Value}\n";
                        }
                    }
                }
                else
                {
                    tooltipText = "Fără efecte.";
                }

                string capturedId = ideaId;
                lista.Add((
                    $"{idea.Name}  —  Research Nivel {level}",
                    tooltipText,
                    () =>
                    {
                        _gameState.ImplementIdea(capturedId, decisionsRequired, nextBlock);
                        ActualizeazaInterfata();
                    }
                ));
            }
            return lista;
        }

        // =====================================================================
        // HELPERS UI
        // =====================================================================
        private Button BuildButton(string text, string tooltipText)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(320, 45);
            btn.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            _tooltip.SetToolTip(btn, tooltipText);
            return btn;
        }

        private string BuildNormalTooltip(DecisionJsonDefinition decizie)
        {
            string result = "Efecte:";
            bool areEfecteAfisabile = false;

            if (decizie.Effects != null && decizie.Effects.Count > 0)
            {
                foreach (var efect in decizie.Effects)
                {
                    if (efect.Type?.ToUpper() == "UNLOCK_LEVEL")
                    {
                        // 1. Verificăm ce nivel este deblocat în acest moment în joc pentru această idee
                        int maxAllowedAcum = 0;
                        if (_gameState.IdeaMaxAllowedLevels.ContainsKey(efect.Property))
                        {
                            maxAllowedAcum = _gameState.IdeaMaxAllowedLevels[efect.Property];
                        }

                        // 2. Dacă nivelul oferit de buton (ex: Nivel 2) este deja deblocat (ex: avem deja 2 sau mai mult)
                        // ascundem complet acest rând din tooltip folosind 'continue'
                        if (efect.Value <= maxAllowedAcum)
                            continue;

                        // Găsim numele ideii pentru un display mai frumos
                        var ideeGasita = _gameState.PovesteIncarcata.Ideas?.FirstOrDefault(i => i.Id == efect.Property);
                        string numeIdee = ideeGasita != null ? ideeGasita.Name : efect.Property;

                        result += $"\n💡 Permite Research Nivel {efect.Value} pentru: {numeIdee}";
                        areEfecteAfisabile = true;
                        continue;
                    }

                    var status = _gameState.ToateStatusurile.FirstOrDefault(s => s.Key == efect.Property);
                    if (status != null) // E un status normal
                    {
                        string semn = efect.Value >= 0 ? "+" : "";
                        result += $"\n{status.Nume}: {semn}{efect.Value}";
                        areEfecteAfisabile = true;
                    }
                }
            }

            if (!string.IsNullOrEmpty(decizie.UnlocksIdeaId) && !_gameState.IdeaResearchLevels.ContainsKey(decizie.UnlocksIdeaId))
            {
                result += "\n! - Descoperă o idee nouă de proiect";
                areEfecteAfisabile = true;
            }

            if (!areEfecteAfisabile)
            {
                return "Fără efecte directe.";
            }

            return result;
        }

        private void lblTextHolder_Click(object sender, EventArgs e)
        {
            if (_textAnimation.IsRunning)
                _textAnimation.Skip();
        }
    }
}
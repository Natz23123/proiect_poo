using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.IO.Compression;

namespace proiect_poo
{
    public partial class Form1 : Form
    {
        private TextAnimation _textAnimation;
        private GameState _gameState;
        private ToolTip _tooltip = new ToolTip();
        private string _tempExtractFolder;
        private string _calePoveste;

        private Dictionary<string, Image> _imageCache = new Dictionary<string, Image>();

        public Form1(string calePovesteCustom = null)
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.UpdateStyles();

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            lblTextHolder.ForeColor = Color.White;
            _textAnimation = new TextAnimation(lblTextHolder, 25);
            _gameState = new GameState();

            _tooltip.OwnerDraw = true;
            _tooltip.Draw += CustomToolTip_Draw;
            _tooltip.Popup += CustomToolTip_Popup;

            lblTextHolder.BringToFront();
            panelHUD.BringToFront();
            panelButoane.BringToFront();

            lblTextHolder.BackColor = Color.Transparent;
            panelHUD.BackColor = Color.Transparent;
            panelButoane.BackColor = Color.Transparent;

            panelHUD.BorderStyle = BorderStyle.None;
            panelButoane.BorderStyle = BorderStyle.None;
            lblTextHolder.BorderStyle = BorderStyle.None;

            // ----- LOGICA NOUĂ DE ÎNCĂRCARE -----
            string povesteDeIncarcat = calePovesteCustom;

            // Dacă nu a fost aleasă o poveste din OpenFileDialog, căutăm povestea default
            if (string.IsNullOrEmpty(povesteDeIncarcat))
            {
                if (File.Exists("default_story.zip"))
                {
                    povesteDeIncarcat = "default_story.zip";
                }
                else if (File.Exists("default_story.json"))
                {
                    povesteDeIncarcat = "default_story.json";
                }
            }

            // Încărcăm fișierul găsit sau primit ca parametru
            if (!string.IsNullOrEmpty(povesteDeIncarcat) && File.Exists(povesteDeIncarcat))
            {
                IncarcaPovesteDinFisier(povesteDeIncarcat);
            }
            else
            {
                MessageBox.Show("Nu s-a găsit nicio poveste validă (zip/json). Jocul pornește fără poveste.");
            }
            // ------------------------------------

            // Curăță folderul temporar la închidere
            this.FormClosed += (s, e) =>
            {
                foreach (var img in _imageCache.Values) img?.Dispose();
                _imageCache.Clear();
                if (this.BackgroundImage != null)
                {
                    this.BackgroundImage.Dispose();
                    this.BackgroundImage = null;
                }

                ElibereazaImaginiDinControale(this.Controls);

                GC.Collect();
                GC.WaitForPendingFinalizers();

                if (!string.IsNullOrEmpty(_tempExtractFolder) && Directory.Exists(_tempExtractFolder))
                    Directory.Delete(_tempExtractFolder, true);
            };
        }

        private void ElibereazaImaginiDinControale(Control.ControlCollection controale)
        {
            foreach (Control c in controale)
            {
                if (c is PictureBox pb)
                {
                    pb.Image?.Dispose();
                    pb.Image = null;
                }
                // Parcurge recursiv controalele copil (ex. panourile care conțin iconițe)
                if (c.HasChildren)
                    ElibereazaImaginiDinControale(c.Controls);
            }
        }

        private void IncarcaPovesteDinFisier(string caleFisier)
        {
            try
            {
                string ext = Path.GetExtension(caleFisier)?.ToLower();

                if (ext == ".zip")
                {
                    // Extrage arhiva într-un folder temporar
                    _tempExtractFolder = Path.Combine(Path.GetTempPath(), "poveste_joc_" + Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(_tempExtractFolder);
                    ZipFile.ExtractToDirectory(caleFisier, _tempExtractFolder);

                    string jsonPath = Path.Combine(_tempExtractFolder, "story.json");
                    if (!File.Exists(jsonPath))
                    {
                        MessageBox.Show("Arhiva ZIP nu conține story.json!", "Eroare");
                        return;
                    }
                    _calePoveste = _tempExtractFolder;
                    _gameState.InitializareJoc(JsonManager.IncarcaPoveste(jsonPath));
                    // Validare la încărcare
                    var erori = ValidareRapida(_gameState.PovesteIncarcata);
                    if (erori.Count > 0)
                    {
                        string mesaj = "⚠️ Povestea are probleme:\n\n";
                        foreach (var e in erori)
                            mesaj += "• " + e + "\n";
                        mesaj += "\nJocul va porni, dar pot apărea erori.";
                        MessageBox.Show(mesaj, "Avertisment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else // JSON obișnuit
                {
                    if (!File.Exists(caleFisier))
                    {
                        MessageBox.Show($"Fișierul '{caleFisier}' nu a fost găsit!");
                        return;
                    }
                    _calePoveste = Path.GetDirectoryName(caleFisier);
                    _gameState.InitializareJoc(JsonManager.IncarcaPoveste(caleFisier));
                }

                RezolvaCaiImagini();
                ActualizeazaInterfata();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare critică la încărcarea jocului: " + ex.Message);
            }
        }

        private void RezolvaCaiImagini()
        {
            string baseDir = _calePoveste;
            if (string.IsNullOrEmpty(baseDir)) return;
            if (_gameState?.PovesteIncarcata == null) return;

            var poveste = _gameState.PovesteIncarcata;
            string imagesDir = Path.Combine(baseDir, "imagini");

            if (poveste.Days != null)
            {
                foreach (var zi in poveste.Days)
                {
                    if (zi.Blocks == null) continue;
                    foreach (var bloc in zi.Blocks)
                    {
                        if (bloc.Decisions != null)
                        {
                            foreach (var dec in bloc.Decisions)
                            {
                                if (!string.IsNullOrEmpty(dec.Icon))
                                {
                                    string fullPath = Path.Combine(imagesDir, dec.Icon);
                                    dec.Icon = File.Exists(fullPath) ? fullPath : null;
                                    if (dec.Icon != null)
                                        IncarcaImagine(dec.Icon); // încarcă în cache
                                }
                            }
                        }
                    }
                }
            }
        }

        private List<string> ValidareRapida(StoryJsonDefinition poveste)
        {
            var erori = new List<string>();
            if (poveste == null) { erori.Add("Povestea e null."); return erori; }

            if (string.IsNullOrEmpty(poveste.StartBlock))
                erori.Add("Nu e definit startBlock.");

            var toateIdurile = poveste.Days?
                .SelectMany(z => z.Blocks ?? new List<BlockJsonDefinition>())
                .Select(b => b.Id)
                .ToHashSet() ?? new HashSet<string>();

            if (!string.IsNullOrEmpty(poveste.StartBlock) && !toateIdurile.Contains(poveste.StartBlock))
                erori.Add($"StartBlock '{poveste.StartBlock}' nu există.");

            if (poveste.Days == null || poveste.Days.Count == 0)
                erori.Add("Povestea nu are nicio zi definită.");

            if (poveste.Properties == null || poveste.Properties.Count == 0)
                erori.Add("Povestea nu are proprietăți definite.");

            return erori;
        }

        private Image IncarcaImagine(string cale)
        {
            if (string.IsNullOrEmpty(cale) || !File.Exists(cale)) return null;
            if (!_imageCache.ContainsKey(cale))
                _imageCache[cale] = Image.FromFile(cale);
            return _imageCache[cale];
        }

        private void ActualizeazaInterfata()
        {
            var blocCurent = _gameState.GasesteBlocDupaId(_gameState.CurrentBlockId);
            if (blocCurent == null)
            {
                blocCurent = _gameState.GasesteEndingPotrivit();
                if (blocCurent == null)
                {
                    MessageBox.Show($"Eroare: Blocul '{_gameState.CurrentBlockId}' nu există și nu există niciun ending.");
                    return;
                }
                _gameState.CurrentBlockId = blocCurent.Id;
            }

            // Asigură-te că textul și panourile sunt deasupra
            lblTextHolder.BringToFront();
            panelHUD.BringToFront();
            panelButoane.BringToFront();

            // ----- AUTOMAT PENTRU BLOCUL DE VERIFICARE A SFÂRȘITULUI -----
            if (blocCurent.Id == "block_endings_check")
            {
                var deciziiValide = blocCurent.Decisions
                    .Where(d => d.Condition == null || d.Condition.Evaluate(_gameState.ToateStatusurile))
                    .ToList();

                string idEndingAles;
                if (deciziiValide.Count > 0)
                    idEndingAles = deciziiValide.First().TargetBlock;
                else
                    idEndingAles = blocCurent.NextBlock ?? "ending_default";

                _gameState.CurrentBlockId = idEndingAles;
                ActualizeazaInterfata();
                return;
            }

            string tipBloc = blocCurent.BlockType ?? "normal";

            if (tipBloc == "ending" || tipBloc == "default_ending")
            {
                AfiseazaEcranEnding(blocCurent);
                return;
            }

            string title = _gameState.PovesteIncarcata?.Title ?? "Joc";
            var ziCurenta = _gameState.ZiuaCurenta();
            this.Text = ziCurenta != null ? $"{title} | {ziCurenta.Name}" : title;

            _textAnimation.StartAnimation(blocCurent.Text);
            createStatusHud();
            createButtons(blocCurent);
        }
        //protected override void OnPaintBackground(PaintEventArgs e) {
        //    //MessageBox.Show($"bg poveste: '{_gameState?.PovesteIncarcata?.BackgroundImage}'\nblock bg: '{(_blockBackgroundPictureBox?.Visible == true ? "visible" : "hidden")}'");
        //    if (_blockBackgroundPictureBox != null && _blockBackgroundPictureBox.Visible && _blockBackgroundPictureBox.Image != null)
        //        e.Graphics.DrawImage(_blockBackgroundPictureBox.Image, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
        //    else if (_gameState?.PovesteIncarcata != null &&
        //             !string.IsNullOrEmpty(_gameState.PovesteIncarcata.BackgroundImage) &&
        //             File.Exists(_gameState.PovesteIncarcata.BackgroundImage))
        //    {
        //        var img = IncarcaImagine(_gameState.PovesteIncarcata.BackgroundImage);
        //        if (img != null)
        //            e.Graphics.DrawImage(img, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
        //        else
        //            e.Graphics.Clear(Color.Black);
        //    }
        //    else
        //        e.Graphics.Clear(Color.Black);
        //}

        private void AfiseazaEcranEnding(BlockJsonDefinition blocEnding)
        {
            // Titlu fereastră
            this.Text = _gameState.PovesteIncarcata.Title + " — SFÂRȘIT";

            // Oprește orice animație și afișează textul direct
            _textAnimation.StartAnimation(blocEnding.Text);

            // Golește butoanele și pune doar "Joacă din nou" și "Ieși"
            while (panelButoane.Controls.Count > 0)
            {
                var c = panelButoane.Controls[0];
                panelButoane.Controls.RemoveAt(0);
                c.Dispose();
            }
            _tooltip.RemoveAll();

            var btnReplay = BuildButton("▶  Joacă din nou", "Repornește jocul de la început.");
            btnReplay.Click += (s, e) =>
            {
                _gameState.InitializareJoc(_gameState.PovesteIncarcata);
                ActualizeazaInterfata();
            };

            var btnIesire = BuildButton("✕  Ieși", "Închide jocul.");
            btnIesire.Click += (s, e) => this.Close();

            panelButoane.Controls.Add(btnReplay);
            panelButoane.Controls.Add(btnIesire);

            createStatusHud(); // arată statusurile finale
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
                lbl.ForeColor = Color.White;

                TerminalProgressBar pb = new TerminalProgressBar();
                pb.Minimum = status.Min;
                pb.Maximum = status.Max;
                pb.Value = status.Valoare;

                bool isPrimary = index == 0;
                lbl.Font = new Font("Smallest Pixel-7", isPrimary ? 12 : 10, FontStyle.Bold);
                lbl.Margin = new Padding(5, 0, 5, 0);
                pb.Size = new Size(isPrimary ? 240 : 180, isPrimary ? 30 : 20);
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
            while (panelButoane.Controls.Count > 0)
            {
                var c = panelButoane.Controls[0];
                panelButoane.Controls.RemoveAt(0);
                c.Dispose();
            }
            _tooltip.RemoveAll();
            _tooltip.InitialDelay = 300;
            _tooltip.AutoPopDelay = 5000;

            string blockType = blocCurent.BlockType ?? "normal";

            // Decizii normale
            foreach (var decizie in blocCurent.Decisions)
            {
                if (decizie.Condition != null && !decizie.Condition.Evaluate(_gameState.ToateStatusurile))
                    continue;

                bool areIcon = !string.IsNullOrEmpty(decizie.Icon) && File.Exists(decizie.Icon);
                Image icon = null;
                if (areIcon)
                {
                    try
                    {
                        var original = IncarcaImagine(decizie.Icon);
                        if (original != null)
                            icon = new Bitmap(original, new Size(128, 128));
                        else
                            areIcon = false;
                    }
                    catch { areIcon = false; }
                }

                Button btn = new Button();
                btn.Size = new Size(128, 128);
                btn.BackColor = Color.Black;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Color.White;
                btn.FlatAppearance.MouseOverBackColor = Color.White;
                btn.Cursor = Cursors.Hand;
                btn.Margin = new Padding(0, 0, 5, 5);

                if (areIcon && icon != null)
                {
                    btn.Image = icon;
                    btn.ImageAlign = ContentAlignment.MiddleCenter;
                    btn.Text = "";
                }
                // fără icon = buton negru cu bordură albă, fără text

                string tooltipText = decizie.Text + "\n\n" + BuildNormalTooltip(decizie);
                _tooltip.SetToolTip(btn, tooltipText);

                var decizieCapturata = decizie;
                int decisionsRequired = blocCurent.DecisionsRequired;

                btn.Click += (s, e) =>
                {
                    _gameState.AplicaEfecteDecizie(decizieCapturata, decisionsRequired);
                    ActualizeazaInterfata();
                };

                btn.MouseEnter += (s, e) => { if (areIcon) btn.Image = icon; btn.ForeColor = Color.Black; };
                btn.MouseLeave += (s, e) => btn.ForeColor = Color.White;

                panelButoane.Controls.Add(btn);
            }

            // Buton agregat RESEARCH
            if (blockType == "research")
            {
                bool areIdeiDeResearch = _gameState.IdeaResearchLevels.Keys
                    .Any(id => _gameState.GetNextResearchLevel(id) != null);

                if (areIdeiDeResearch)
                {
                    int decisionsReqR = blocCurent.DecisionsRequired;
                    string nextBlockR = blocCurent.NextBlock;

                    Button btnResearch = new Button();
                    btnResearch.Size = new Size(128, 128);
                    btnResearch.BackColor = Color.Black;
                    btnResearch.FlatStyle = FlatStyle.Flat;
                    btnResearch.FlatAppearance.BorderSize = 1;
                    btnResearch.FlatAppearance.BorderColor = Color.White;
                    btnResearch.FlatAppearance.MouseOverBackColor = Color.White;
                    btnResearch.Cursor = Cursors.Hand;
                    btnResearch.Margin = new Padding(0, 0, 5, 5);
                    btnResearch.Text = "🔬";
                    btnResearch.ForeColor = Color.White;
                    btnResearch.Font = new Font("Smallest Pixel-7", 24, FontStyle.Bold);
                    btnResearch.MouseEnter += (s, e) => btnResearch.ForeColor = Color.Black;
                    btnResearch.MouseLeave += (s, e) => btnResearch.ForeColor = Color.White;
                    _tooltip.SetToolTip(btnResearch, "Dă research la o idee");

                    btnResearch.Click += (s, e) =>
                    {
                        var optiuni = BuildResearchOptions(decisionsReqR, nextBlockR);
                        ShowIdeaPickerDialog("Research — alege o idee", optiuni);
                    };
                    panelButoane.Controls.Add(btnResearch);
                }

                bool areIdeiDeImplementat = _gameState.IdeaResearchLevels.Any(kv => kv.Value >= 1);
                if (areIdeiDeImplementat)
                {
                    int decisionsReqI = blocCurent.DecisionsRequired;
                    string nextBlockI = blocCurent.NextBlock;

                    Button btnImpl = new Button();
                    btnImpl.Size = new Size(128, 128);
                    btnImpl.BackColor = Color.Black;
                    btnImpl.FlatStyle = FlatStyle.Flat;
                    btnImpl.FlatAppearance.BorderSize = 1;
                    btnImpl.FlatAppearance.BorderColor = Color.White;
                    btnImpl.FlatAppearance.MouseOverBackColor = Color.White;
                    btnImpl.Cursor = Cursors.Hand;
                    btnImpl.Margin = new Padding(0, 0, 5, 5);
                    btnImpl.Text = "⚙︎";
                    btnImpl.ForeColor = Color.White;
                    btnImpl.Font = new Font("Smallest Pixel-7", 24, FontStyle.Bold);
                    btnImpl.MouseEnter += (s, e) => btnImpl.ForeColor = Color.Black;
                    btnImpl.MouseLeave += (s, e) => btnImpl.ForeColor = Color.White;
                    _tooltip.SetToolTip(btnImpl, "Implementează o idee");

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
        private void ShowIdeaPickerDialog(string titlu, List<(string label, string tooltip, Action actiune)> optiuni)
        {
            Form popup = new Form();
            popup.Text = titlu;
            popup.Size = new Size(440, 80 + optiuni.Count * 55);
            popup.StartPosition = FormStartPosition.CenterParent;
            popup.FormBorderStyle = FormBorderStyle.FixedDialog;
            popup.MaximizeBox = false;
            popup.MinimizeBox = false;
            popup.BackColor = Color.Black;
            popup.ForeColor = Color.White;

            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.FlowDirection = FlowDirection.TopDown;
            panel.Padding = new Padding(10);
            panel.AutoScroll = true;
            panel.BackColor = Color.Black;

            foreach (var (label, tooltipText, actiune) in optiuni)
            {
                Button btn = new Button();
                btn.Text = label;
                btn.Size = new Size(400, 45);
                btn.Font = new Font("Smallest Pixel-7", 10, FontStyle.Regular);
                btn.ForeColor = Color.White;
                btn.BackColor = Color.Black;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Color.White;
                btn.FlatAppearance.MouseOverBackColor = Color.White;
                btn.Cursor = Cursors.Hand;
                btn.MouseEnter += (s, e) => btn.ForeColor = Color.Black;
                btn.MouseLeave += (s, e) => btn.ForeColor = Color.White;
                _tooltip.SetToolTip(btn, tooltipText);

                btn.Click += (s, e) =>
                {
                    popup.Close();
                    actiune();
                };
                panel.Controls.Add(btn);
            }

            popup.Controls.Add(panel);
            popup.ShowDialog(this);
        }

        // Construiește lista de opțiuni pentru research
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

                // Tooltip din efecte
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

        // Construiește lista de opțiuni pentru implement
        private List<(string, string, Action)> BuildImplementOptions(int decisionsRequired, string nextBlock)
        {
            var lista = new List<(string, string, Action)>();

            foreach (var kv in _gameState.IdeaResearchLevels.Where(x => x.Value >= 1))
            {
                string ideaId = kv.Key;
                int level = kv.Value;

                if (_gameState.IdeaImplementationLevels.TryGetValue(ideaId, out int implLevel) && implLevel >= level)
                    continue;

                var idea = _gameState.PovesteIncarcata.Ideas?.FirstOrDefault(i => i.Id == ideaId);
                var levelDef = idea?.ResearchLevels.FirstOrDefault(l => l.Level == level);
                if (idea == null || levelDef == null) continue;

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

            btn.AutoSize = true;
            btn.MinimumSize = new Size(340, 45);
            btn.Padding = new Padding(10, 5, 10, 5);
            btn.TextAlign = ContentAlignment.MiddleLeft;

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.White;
            btn.FlatAppearance.MouseOverBackColor = Color.White;

            btn.BackColor = Color.Black;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Smallest Pixel-7", 10, FontStyle.Regular);
            btn.Cursor = Cursors.Hand;

            btn.MouseEnter += (s, e) => btn.ForeColor = Color.Black;
            btn.MouseLeave += (s, e) => btn.ForeColor = Color.White;

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
                        int maxAllowedAcum = 0;
                        if (_gameState.IdeaMaxAllowedLevels.ContainsKey(efect.Property))
                            maxAllowedAcum = _gameState.IdeaMaxAllowedLevels[efect.Property];

                        if (efect.Value <= maxAllowedAcum)
                            continue;

                        var ideeGasita = _gameState.PovesteIncarcata.Ideas?.FirstOrDefault(i => i.Id == efect.Property);
                        string numeIdee = ideeGasita != null ? ideeGasita.Name : efect.Property;

                        result += $"\n💡 Permite Research Nivel {efect.Value} pentru: {numeIdee}";
                        areEfecteAfisabile = true;
                        continue;
                    }

                    var status = _gameState.ToateStatusurile.FirstOrDefault(s => s.Key == efect.Property);
                    if (status != null)
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
                return "Fără efecte directe.";

            return result;
        }

        private void lblTextHolder_Click(object sender, EventArgs e)
        {
            if (_textAnimation.IsRunning)
                _textAnimation.Skip();
        }

        // =====================================================================
        // CUSTOM TOOLTIP STYLING
        // =====================================================================
        private void CustomToolTip_Popup(object sender, PopupEventArgs e)
        {
            // Calculează dimensiunea exactă a textului cu fontul pixelat
            using (var font = new Font("Smallest Pixel-7", 12f, FontStyle.Regular))
            {
                var text = _tooltip.GetToolTip(e.AssociatedControl) ?? string.Empty;
                var textSize = TextRenderer.MeasureText(text, font, new Size(int.MaxValue, int.MaxValue),
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                e.ToolTipSize = new Size(textSize.Width + 15, textSize.Height + 15);
            }
        }

        private void CustomToolTip_Draw(object sender, DrawToolTipEventArgs e)
        {
            using (var font = new Font("Smallest Pixel-7", 12f, FontStyle.Regular))
            using (var borderPen = new Pen(Color.White))
            using (var backBrush = new SolidBrush(Color.Black))
            {
                // Fundal negru
                e.Graphics.FillRectangle(backBrush, e.Bounds);
                // Chenar alb
                var borderRect = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
                e.Graphics.DrawRectangle(borderPen, borderRect);
                // Text alb
                var textRect = new Rectangle(e.Bounds.X + 7, e.Bounds.Y + 7, e.Bounds.Width - 14, e.Bounds.Height - 14);
                TextRenderer.DrawText(e.Graphics, e.ToolTipText, font, textRect, Color.White,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }

        private void panelButoane_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
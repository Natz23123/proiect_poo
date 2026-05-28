using System;
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
                MessageBox.Show($"Eroare: Blocul cu ID-ul '{_gameState.CurrentBlockId}' nu a fost găsit în JSON.");
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

        private void createStatusHud()
        {
            panelHUD.Controls.Clear();

            var statusuriDeAfisat = _gameState.ToateStatusurile
                .Where(s => s.VisibleInHud)
                .OrderBy(s => s.HudOrder);

            int index = 0;

            foreach (var status in statusuriDeAfisat)
            {
                FlowLayoutPanel cutieStatus = new FlowLayoutPanel();
                cutieStatus.FlowDirection = FlowDirection.TopDown;
                cutieStatus.WrapContents = false;
                cutieStatus.AutoSize = true;
                cutieStatus.Margin = new Padding(0, 0, 0, 4);

                Label lblStatus = new Label();
                lblStatus.Text = $"{status.Nume}: {status.Valoare}%";
                lblStatus.AutoSize = true;

                ProgressBar pb = new ProgressBar();
                pb.Minimum = status.Min;
                pb.Maximum = status.Max;
                pb.Value = status.Valoare;

                bool isPrimary = index == 0;
                lblStatus.Font = new Font("Segoe UI", isPrimary ? 12 : 10, FontStyle.Bold);
                lblStatus.Margin = new Padding(5, 0, 5, 0);

                pb.Size = new Size(isPrimary ? 110 : 60, isPrimary ? 14 : 8);
                pb.Margin = new Padding(5, 1, 5, 0);

                cutieStatus.Controls.Add(lblStatus);
                cutieStatus.Controls.Add(pb);
                panelHUD.Controls.Add(cutieStatus);

                index++;
            }
        }

        private void createButtons(BlockJsonDefinition blocCurent)
        {
            panelButoane.Controls.Clear();

            _tooltip.InitialDelay = 300;
            _tooltip.AutoPopDelay = 5000;

            foreach (var decizie in blocCurent.Decisions)
            {
                if (decizie.Condition != null && !decizie.Condition.Evaluate(_gameState.ToateStatusurile))
                    continue;

                Button btn = new Button();
                btn.Text = decizie.Text;
                btn.Size = new Size(320, 45);
                btn.Font = new Font("Segoe UI", 9, FontStyle.Regular);

                string infoHover = "Efecte previzibile:";
                if (decizie.Effects != null && decizie.Effects.Count > 0)
                {
                    foreach (var efect in decizie.Effects)
                    {
                        var status = _gameState.ToateStatusurile.FirstOrDefault(s => s.Key == efect.Property);
                        string numeStatus = status != null ? status.Nume : efect.Property;

                        string semn = efect.Value >= 0 ? "+" : "";
                        infoHover += $"\n{numeStatus}: {semn}{efect.Value}";
                    }
                }
                else
                {
                    infoHover = "Această decizie nu are efecte directe.";
                }

                _tooltip.SetToolTip(btn, infoHover);
                var decizieCapturata = decizie;
                int decisionsRequired = blocCurent.DecisionsRequired;

                btn.Click += (sender, e) =>
                {
                    _gameState.AplicaEfecteDecizie(decizieCapturata, decisionsRequired);
                    ActualizeazaInterfata();
                };

                panelButoane.Controls.Add(btn);
            }
        }

        private void lblTextHolder_Click(object sender, EventArgs e)
        {
            if (_textAnimation.IsRunning)
                _textAnimation.Skip();
        }
    }
}
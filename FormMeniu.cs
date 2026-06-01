using System;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace proiect_poo
{
    public partial class FormMeniu : Form
    {
        // Butoanele existente din Designer
        private Button btnJoaca;
        private Button btnEditor;
        private PictureBox pictureBox1;
        private Label lblArrowHover;
        private Timer timerBlinkingArrow;
        private System.ComponentModel.IContainer components;
        private Button btnIesire;
        private string _hoverSoundPath;
        private string _clickSoundPath;

        // ========================================================
        // LINIILE ADAUGATE: Declarăm butoanele noi ca să fie recunoscute în context
        private Button btnDefaultStory;
        private Button btnLoadStory;
        private Button btnInapoi;
        // ========================================================

        public FormMeniu()
        {
            InitializeComponent();
            ConfigureazaSubmeniuPoveste();

            string soundEffectsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound Effects");
            _hoverSoundPath = Path.Combine(soundEffectsPath, "hover.wav");
            _clickSoundPath = Path.Combine(soundEffectsPath, "click.wav");
        }

        private void ConfigureazaSubmeniuPoveste()
        {
            btnDefaultStory = CloneazaButonMeniu(btnJoaca, "Default Story", btnDefaultStory_Click);
            btnLoadStory = CloneazaButonMeniu(btnEditor, "Load Story", btnLoadStory_Click);
            btnInapoi = CloneazaButonMeniu(btnIesire, "Back", btnInapoi_Click);

            this.Controls.Add(btnDefaultStory);
            this.Controls.Add(btnLoadStory);
            this.Controls.Add(btnInapoi);
        }

        private Button CloneazaButonMeniu(Button model, string text, EventHandler clickEvent)
        {
            Button btn = new Button();
            btn.Size = model.Size;
            btn.Location = model.Location;
            btn.Font = model.Font;
            btn.FlatStyle = model.FlatStyle;
            btn.FlatAppearance.BorderColor = model.FlatAppearance.BorderColor;
            btn.ForeColor = model.ForeColor;
            btn.Text = text;
            btn.AutoSize = model.AutoSize;
            btn.AutoSizeMode = model.AutoSizeMode;
            btn.Visible = false; // Ascunse la început
            btn.Click += (s, e) =>
            {
                PlayClickSound();
                clickEvent(s, e);
            };
            btn.MouseEnter += FormMeniu_MouseEnter; // Sunet retro + indicator automatisme
            return btn;
        }

        // Butonul: PORNEȘTE JOCUL
        private void btnJoaca_Click(object sender, EventArgs e)
        {
            PlayClickSound();
            // Ascundem meniul principal
            btnJoaca.Visible = false;
            btnEditor.Visible = false;
            btnIesire.Visible = false;
            lblArrowHover.Visible = false;

            // Arătăm opțiunile de poveste
            btnDefaultStory.Visible = true;
            btnLoadStory.Visible = true;
            btnInapoi.Visible = true;
        }

        private void btnDefaultStory_Click(object sender, EventArgs e)
        {
            PornesteJocul(null); // Trimite null ca să încarce fișierul default standard
        }

        private void btnLoadStory_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Arhive ZIP (*.zip)|*.zip";
                openFileDialog.Title = "Selectează arhiva ZIP a poveștii";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    PornesteJocul(openFileDialog.FileName);
                }
            }
        }

        private void btnInapoi_Click(object sender, EventArgs e)
        {
            // Ascundem opțiunile de poveste
            btnDefaultStory.Visible = false;
            btnLoadStory.Visible = false;
            btnInapoi.Visible = false;
            lblArrowHover.Visible = false;

            // Revenim la meniul principal
            btnJoaca.Visible = true;
            btnEditor.Visible = true;
            btnIesire.Visible = true;
        }

        private void PornesteJocul(string caleFisier)
        {
            this.Hide();

            Form1 fereastraJoc = new Form1(caleFisier);
            fereastraJoc.ShowDialog();

            // Când jucătorul revine în meniu, resetăm starea vizuală la meniul principal
            btnDefaultStory.Visible = false;
            btnLoadStory.Visible = false;
            btnInapoi.Visible = false;

            btnJoaca.Visible = true;
            btnEditor.Visible = true;
            btnIesire.Visible = true;
            lblArrowHover.Visible = false;

            this.Show();
        }

        // Butonul: DESCHIDE EDITOR
        private void btnEditor_Click(object sender, EventArgs e)
        {
            PlayClickSound();
            this.Hide();
            FormEditor fereastraEditor = new FormEditor();
            fereastraEditor.ShowDialog();
            this.Show();
        }

        // Butonul: IEȘIRE
        private void btnIesire_Click(object sender, EventArgs e)
        {
            PlayClickSound();
            Application.Exit();
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMeniu));
            this.btnJoaca = new System.Windows.Forms.Button();
            this.btnEditor = new System.Windows.Forms.Button();
            this.btnIesire = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblArrowHover = new System.Windows.Forms.Label();
            this.timerBlinkingArrow = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnJoaca
            // 
            this.btnJoaca.AutoSize = true;
            this.btnJoaca.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnJoaca.FlatAppearance.BorderColor = System.Drawing.SystemColors.Desktop;
            this.btnJoaca.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnJoaca.Font = new System.Drawing.Font("Smallest Pixel-7", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnJoaca.ForeColor = System.Drawing.SystemColors.Control;
            this.btnJoaca.Location = new System.Drawing.Point(404, 252);
            this.btnJoaca.Name = "btnJoaca";
            this.btnJoaca.Size = new System.Drawing.Size(105, 52);
            this.btnJoaca.TabIndex = 0;
            this.btnJoaca.Text = "Play";
            this.btnJoaca.UseVisualStyleBackColor = true;
            this.btnJoaca.Click += new System.EventHandler(this.btnJoaca_Click);
            this.btnJoaca.MouseEnter += new System.EventHandler(this.FormMeniu_MouseEnter);
            // 
            // btnEditor
            // 
            this.btnEditor.AutoSize = true;
            this.btnEditor.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnEditor.FlatAppearance.BorderColor = System.Drawing.SystemColors.Desktop;
            this.btnEditor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditor.Font = new System.Drawing.Font("Smallest Pixel-7", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEditor.ForeColor = System.Drawing.SystemColors.Control;
            this.btnEditor.Location = new System.Drawing.Point(404, 310);
            this.btnEditor.Name = "btnEditor";
            this.btnEditor.Size = new System.Drawing.Size(141, 52);
            this.btnEditor.TabIndex = 1;
            this.btnEditor.Text = "Editor";
            this.btnEditor.UseVisualStyleBackColor = true;
            this.btnEditor.Click += new System.EventHandler(this.btnEditor_Click);
            this.btnEditor.MouseEnter += new System.EventHandler(this.FormMeniu_MouseEnter);
            // 
            // btnIesire
            // 
            this.btnIesire.AutoSize = true;
            this.btnIesire.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnIesire.FlatAppearance.BorderColor = System.Drawing.SystemColors.Desktop;
            this.btnIesire.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIesire.Font = new System.Drawing.Font("Smallest Pixel-7", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnIesire.ForeColor = System.Drawing.SystemColors.Control;
            this.btnIesire.Location = new System.Drawing.Point(404, 378);
            this.btnIesire.Name = "btnIesire";
            this.btnIesire.Size = new System.Drawing.Size(101, 52);
            this.btnIesire.TabIndex = 2;
            this.btnIesire.Text = "Exit";
            this.btnIesire.UseVisualStyleBackColor = true;
            this.btnIesire.Click += new System.EventHandler(this.btnIesire_Click);
            this.btnIesire.MouseEnter += new System.EventHandler(this.FormMeniu_MouseEnter);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.pictureBox1.Image = global::proiect_poo.Properties.Resources.title;
            this.pictureBox1.Location = new System.Drawing.Point(26, 24);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(904, 150);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // lblArrowHover
            // 
            this.lblArrowHover.AutoSize = true;
            this.lblArrowHover.BackColor = System.Drawing.Color.Transparent;
            this.lblArrowHover.Font = new System.Drawing.Font("Smallest Pixel-7", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblArrowHover.ForeColor = System.Drawing.SystemColors.Control;
            this.lblArrowHover.Location = new System.Drawing.Point(367, 258);
            this.lblArrowHover.Name = "lblArrowHover";
            this.lblArrowHover.Size = new System.Drawing.Size(34, 40);
            this.lblArrowHover.TabIndex = 4;
            this.lblArrowHover.Text = ">";
            this.lblArrowHover.Visible = false;
            this.lblArrowHover.Click += new System.EventHandler(this.lblArrowHover_Click);
            // 
            // timerBlinkingArrow
            // 
            this.timerBlinkingArrow.Interval = 500;
            this.timerBlinkingArrow.Tick += new System.EventHandler(this.timerBlinkingArrow_Tick);
            // 
            // FormMeniu
            // 
            this.BackColor = System.Drawing.SystemColors.Desktop;
            this.ClientSize = new System.Drawing.Size(942, 541);
            this.Controls.Add(this.lblArrowHover);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnIesire);
            this.Controls.Add(this.btnEditor);
            this.Controls.Add(this.btnJoaca);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMeniu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void timerBlinkingArrow_Tick(object sender, EventArgs e)
        {
            lblArrowHover.Visible = !lblArrowHover.Visible;
        }

        private void lblArrowHover_Click(object sender, EventArgs e)
        {

        }

        private void FormMeniu_MouseEnter(object sender, EventArgs e)
        {
            Button hoveredButton = (Button)sender;

            lblArrowHover.Left = hoveredButton.Left - lblArrowHover.Width;
            lblArrowHover.Top = hoveredButton.Top + 5;
            lblArrowHover.Visible = true;
            timerBlinkingArrow.Stop();
            PlayHoverSound();
            timerBlinkingArrow.Start();
        }

        private void PlayHoverSound()
        {
            PlaySoundImmediate(_hoverSoundPath);
        }

        private void PlayClickSound()
        {
            PlaySoundImmediate(_clickSoundPath);
        }

        private static void PlaySoundImmediate(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return;
            }

            PlaySound(path, IntPtr.Zero, PlaySoundFlags.SND_FILENAME | PlaySoundFlags.SND_ASYNC | PlaySoundFlags.SND_NODEFAULT | PlaySoundFlags.SND_PURGE);
        }

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool PlaySound(string pszSound, IntPtr hmod, PlaySoundFlags fdwSound);

        [Flags]
        private enum PlaySoundFlags : int
        {
            SND_ASYNC = 0x0001,
            SND_NODEFAULT = 0x0002,
            SND_PURGE = 0x0040,
            SND_FILENAME = 0x00020000
        }
    }
}
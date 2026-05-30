using System;
using System.Windows.Forms;
using System.Media;
namespace proiect_poo
{
    public partial class FormMeniu : Form
    {
        private Button btnJoaca;
        private Button btnEditor;
        private PictureBox pictureBox1;
        private Label lblArrowHover;
        private Timer timerBlinkingArrow;
        private System.ComponentModel.IContainer components;
        private Button btnIesire;

        public FormMeniu()
        {
            InitializeComponent();
        }

        // Butonul: PORNEȘTE JOCUL
        private void btnJoaca_Click(object sender, EventArgs e)
        {
            this.Hide(); // Ascundem meniul ca să nu stea în fundal degeaba

            Form1 fereastraJoc = new Form1();
            fereastraJoc.ShowDialog(); // ShowDialog blochează codul aici până când se închide jocul

            this.Show(); // Când jucătorul închide jocul și revine, meniul reapare automatic
        }

        // Butonul: DESCHIDE EDITOR
        private void btnEditor_Click(object sender, EventArgs e)
        {
            this.Hide(); // Ascundem meniul

            FormEditor fereastraEditor = new FormEditor();
            fereastraEditor.ShowDialog(); // Deschidem editorul

            this.Show(); // Când se închide editorul, meniul reapare
        }

        // Butonul: IEȘIRE
        private void btnIesire_Click(object sender, EventArgs e)
        {
            Application.Exit(); // Închide complet aplicația
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
            this.btnJoaca.Location = new System.Drawing.Point(421, 252);
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
            this.btnIesire.Location = new System.Drawing.Point(421, 378);
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
            SoundPlayer hoverSound = new SoundPlayer(Properties.Resources.menu_hover);
            hoverSound.Play();
            timerBlinkingArrow.Start();
        }
    }
}
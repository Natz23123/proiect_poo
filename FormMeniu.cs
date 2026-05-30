using System;
using System.Windows.Forms;

namespace proiect_poo
{
    public partial class FormMeniu : Form
    {
        private Button btnJoaca;
        private Button btnEditor;
        private PictureBox pictureBox1;
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
            this.btnJoaca = new System.Windows.Forms.Button();
            this.btnEditor = new System.Windows.Forms.Button();
            this.btnIesire = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnJoaca
            // 
            this.btnJoaca.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnJoaca.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnJoaca.Font = new System.Drawing.Font("Consolas", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnJoaca.ForeColor = System.Drawing.SystemColors.Control;
            this.btnJoaca.Location = new System.Drawing.Point(386, 250);
            this.btnJoaca.Name = "btnJoaca";
            this.btnJoaca.Size = new System.Drawing.Size(147, 52);
            this.btnJoaca.TabIndex = 0;
            this.btnJoaca.Text = "Play";
            this.btnJoaca.UseVisualStyleBackColor = true;
            this.btnJoaca.Click += new System.EventHandler(this.btnJoaca_Click);
            // 
            // btnEditor
            // 
            this.btnEditor.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnEditor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditor.Font = new System.Drawing.Font("Consolas", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEditor.ForeColor = System.Drawing.SystemColors.Control;
            this.btnEditor.Location = new System.Drawing.Point(386, 320);
            this.btnEditor.Name = "btnEditor";
            this.btnEditor.Size = new System.Drawing.Size(147, 52);
            this.btnEditor.TabIndex = 1;
            this.btnEditor.Text = "Editor";
            this.btnEditor.UseVisualStyleBackColor = true;
            this.btnEditor.Click += new System.EventHandler(this.btnEditor_Click);
            // 
            // btnIesire
            // 
            this.btnIesire.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnIesire.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIesire.Font = new System.Drawing.Font("Consolas", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnIesire.ForeColor = System.Drawing.SystemColors.Control;
            this.btnIesire.Location = new System.Drawing.Point(386, 387);
            this.btnIesire.Name = "btnIesire";
            this.btnIesire.Size = new System.Drawing.Size(147, 52);
            this.btnIesire.TabIndex = 2;
            this.btnIesire.Text = "Exit";
            this.btnIesire.UseVisualStyleBackColor = true;
            this.btnIesire.Click += new System.EventHandler(this.btnIesire_Click);
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
            // FormMeniu
            // 
            this.BackColor = System.Drawing.SystemColors.Desktop;
            this.ClientSize = new System.Drawing.Size(942, 541);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnIesire);
            this.Controls.Add(this.btnEditor);
            this.Controls.Add(this.btnJoaca);
            this.Name = "FormMeniu";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
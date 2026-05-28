using System;
using System.Windows.Forms;

namespace proiect_poo
{
    public partial class FormMeniu : Form
    {
        private Button btnJoaca;
        private Button btnEditor;
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
            this.SuspendLayout();
            // 
            // btnJoaca
            // 
            this.btnJoaca.Location = new System.Drawing.Point(308, 144);
            this.btnJoaca.Name = "btnJoaca";
            this.btnJoaca.Size = new System.Drawing.Size(147, 52);
            this.btnJoaca.TabIndex = 0;
            this.btnJoaca.Text = "Play";
            this.btnJoaca.UseVisualStyleBackColor = true;
            this.btnJoaca.Click += new System.EventHandler(this.btnJoaca_Click);
            // 
            // btnEditor
            // 
            this.btnEditor.Location = new System.Drawing.Point(308, 219);
            this.btnEditor.Name = "btnEditor";
            this.btnEditor.Size = new System.Drawing.Size(147, 52);
            this.btnEditor.TabIndex = 1;
            this.btnEditor.Text = "Editor";
            this.btnEditor.UseVisualStyleBackColor = true;
            this.btnEditor.Click += new System.EventHandler(this.btnEditor_Click);
            // 
            // btnIesire
            // 
            this.btnIesire.Location = new System.Drawing.Point(308, 298);
            this.btnIesire.Name = "btnIesire";
            this.btnIesire.Size = new System.Drawing.Size(147, 52);
            this.btnIesire.TabIndex = 2;
            this.btnIesire.Text = "Exit";
            this.btnIesire.UseVisualStyleBackColor = true;
            this.btnIesire.Click += new System.EventHandler(this.btnIesire_Click);
            // 
            // FormMeniu
            // 
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnIesire);
            this.Controls.Add(this.btnEditor);
            this.Controls.Add(this.btnJoaca);
            this.Name = "FormMeniu";
            this.ResumeLayout(false);

        }
    }
}
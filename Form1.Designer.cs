namespace proiect_poo
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTextHolder = new System.Windows.Forms.Label();
            this.panelButoane = new System.Windows.Forms.FlowLayoutPanel();
            this.panelHUD = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // lblTextHolder
            // 
            this.lblTextHolder.Location = new System.Drawing.Point(566, 250);
            this.lblTextHolder.Name = "lblTextHolder";
            this.lblTextHolder.Size = new System.Drawing.Size(222, 172);
            this.lblTextHolder.TabIndex = 0;
            this.lblTextHolder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTextHolder.Click += new System.EventHandler(this.lblTextHolder_Click);
            // 
            // panelButoane
            // 
            this.panelButoane.Location = new System.Drawing.Point(12, 13);
            this.panelButoane.Name = "panelButoane";
            this.panelButoane.Size = new System.Drawing.Size(496, 409);
            this.panelButoane.TabIndex = 9;
            // 
            // panelHUD
            // 
            this.panelHUD.Location = new System.Drawing.Point(633, 12);
            this.panelHUD.Name = "panelHUD";
            this.panelHUD.Size = new System.Drawing.Size(155, 203);
            this.panelHUD.TabIndex = 10;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.panelHUD);
            this.Controls.Add(this.panelButoane);
            this.Controls.Add(this.lblTextHolder);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblTextHolder;
        private System.Windows.Forms.FlowLayoutPanel panelButoane;
        private System.Windows.Forms.FlowLayoutPanel panelHUD;
    }
}


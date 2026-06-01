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
            this.lblTextHolder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblTextHolder.Font = new System.Drawing.Font("Pixel Operator", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTextHolder.Location = new System.Drawing.Point(18, 503);
            this.lblTextHolder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTextHolder.Name = "lblTextHolder";
            this.lblTextHolder.Size = new System.Drawing.Size(1226, 294);
            this.lblTextHolder.TabIndex = 0;
            this.lblTextHolder.Click += new System.EventHandler(this.lblTextHolder_Click);
            // 
            // panelButoane
            // 
            this.panelButoane.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelButoane.Location = new System.Drawing.Point(18, 20);
            this.panelButoane.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelButoane.Name = "panelButoane";
            this.panelButoane.Size = new System.Drawing.Size(767, 453);
            this.panelButoane.TabIndex = 9;
            this.panelButoane.Paint += new System.Windows.Forms.PaintEventHandler(this.panelButoane_Paint);
            // 
            // panelHUD
            // 
            this.panelHUD.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelHUD.Location = new System.Drawing.Point(838, 18);
            this.panelHUD.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelHUD.Name = "panelHUD";
            this.panelHUD.Size = new System.Drawing.Size(401, 454);
            this.panelHUD.TabIndex = 10;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Desktop;
            this.BackgroundImage = global::proiect_poo.Properties.Resources.scanlines_pixels_2;
            this.ClientSize = new System.Drawing.Size(1258, 826);
            this.Controls.Add(this.panelHUD);
            this.Controls.Add(this.panelButoane);
            this.Controls.Add(this.lblTextHolder);
            this.ForeColor = System.Drawing.SystemColors.Control;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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


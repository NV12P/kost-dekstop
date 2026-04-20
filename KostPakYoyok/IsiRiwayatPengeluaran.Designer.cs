namespace KostPakYoyok
{
    partial class IsiRiwayatPengeluaran
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelData = new Guna.UI2.WinForms.Guna2Panel();
            this.labelPengeluaran = new System.Windows.Forms.Label();
            this.labelDate = new System.Windows.Forms.Label();
            this.labelKeterangan = new System.Windows.Forms.Label();
            this.panelData.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelData
            // 
            this.panelData.BorderColor = System.Drawing.Color.LightGray;
            this.panelData.BorderRadius = 18;
            this.panelData.BorderThickness = 2;
            this.panelData.Controls.Add(this.labelPengeluaran);
            this.panelData.Controls.Add(this.labelDate);
            this.panelData.Controls.Add(this.labelKeterangan);
            this.panelData.FillColor = System.Drawing.Color.White;
            this.panelData.Location = new System.Drawing.Point(20, 16);
            this.panelData.Name = "panelData";
            this.panelData.Size = new System.Drawing.Size(786, 112);
            this.panelData.TabIndex = 32;
            // 
            // labelPengeluaran
            // 
            this.labelPengeluaran.AutoSize = true;
            this.labelPengeluaran.BackColor = System.Drawing.Color.White;
            this.labelPengeluaran.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.labelPengeluaran.ForeColor = System.Drawing.Color.Red;
            this.labelPengeluaran.Location = new System.Drawing.Point(637, 37);
            this.labelPengeluaran.Name = "labelPengeluaran";
            this.labelPengeluaran.Size = new System.Drawing.Size(129, 28);
            this.labelPengeluaran.TabIndex = 31;
            this.labelPengeluaran.Text = "- Rp. 100.000";
            // 
            // labelDate
            // 
            this.labelDate.AutoSize = true;
            this.labelDate.BackColor = System.Drawing.Color.White;
            this.labelDate.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDate.ForeColor = System.Drawing.Color.DarkGray;
            this.labelDate.Location = new System.Drawing.Point(19, 54);
            this.labelDate.Name = "labelDate";
            this.labelDate.Size = new System.Drawing.Size(53, 28);
            this.labelDate.TabIndex = 29;
            this.labelDate.Text = "Date";
            // 
            // labelKeterangan
            // 
            this.labelKeterangan.AutoSize = true;
            this.labelKeterangan.BackColor = System.Drawing.Color.White;
            this.labelKeterangan.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.labelKeterangan.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(18)))), ((int)(((byte)(101)))));
            this.labelKeterangan.Location = new System.Drawing.Point(19, 20);
            this.labelKeterangan.Name = "labelKeterangan";
            this.labelKeterangan.Size = new System.Drawing.Size(57, 32);
            this.labelKeterangan.TabIndex = 28;
            this.labelKeterangan.Text = "text";
            // 
            // IsiRiwayatPengeluaran
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.panelData);
            this.Name = "IsiRiwayatPengeluaran";
            this.Size = new System.Drawing.Size(823, 640);
            this.panelData.ResumeLayout(false);
            this.panelData.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private Guna.UI2.WinForms.Guna2Panel panelData;
        private System.Windows.Forms.Label labelPengeluaran;
        private System.Windows.Forms.Label labelDate;
        private System.Windows.Forms.Label labelKeterangan;
    }
}

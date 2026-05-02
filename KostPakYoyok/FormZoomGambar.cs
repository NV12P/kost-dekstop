using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace KostPakYoyok
{
    public class FormZoomGambar : Form
    {
        public FormZoomGambar(Image img)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.Size = new Size(800, 600);
            this.ShowInTaskbar = false;

            // Efek membulat mang!
            Guna2Elipse el = new Guna2Elipse { TargetControl = this, BorderRadius = 20 };

            var pic = new Guna2PictureBox
            {
                Image = img,
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            var btnClose = new Guna2Button
            {
                Text = "X",
                Size = new Size(45, 45),
                Location = new Point(this.Width - 55, 10),
                FillColor = Color.FromArgb(200, 0, 0),
                ForeColor = Color.White,
                BorderRadius = 22,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(btnClose);
            this.Controls.Add(pic);
            btnClose.BringToFront();

            // Klik gambar juga bisa nutup mang
            pic.Click += (s, e) => this.Close();
        }
    }
}

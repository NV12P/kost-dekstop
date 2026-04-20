using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KostPakYoyok
{
    public class OverlayForm : Form
    {
        public OverlayForm(Form parentForm)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Black;
            this.Opacity = 0.5;
            this.StartPosition = FormStartPosition.Manual;

            // Hanya menutupi area parent form, bukan seluruh layar
            this.Location = parentForm.Location;
            this.Size = parentForm.Size;

            this.ShowInTaskbar = false;
            this.Owner = parentForm; // Set owner
            this.TopMost = false; // Jangan TopMost agar form profil bisa di atas
        }
    }
}

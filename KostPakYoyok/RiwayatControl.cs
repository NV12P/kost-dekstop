using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KostPakYoyok
{
    public partial class RiwayatControl : UserControl
    {
        public RiwayatControl()
        {
            InitializeComponent();
        }

        private void RiwayatControl_Load(object sender, EventArgs e)
        {
            ShowPage(new RiwayatPenghuniAktif());
        }

        private void ShowPage(UserControl page)
        {
            panelRiwayat.Controls.Clear();

            page.Dock = DockStyle.Fill;

            panelRiwayat.Controls.Add(page);
        }

        private void btnPenghuniAktif_Click(object sender, EventArgs e)
        {
            ShowPage(new RiwayatPenghuniAktif());
        }

        private void btnDaftarReservasi_Click(object sender, EventArgs e)
        {
            ShowPage(new RiwayatDaftarReservasi());
        }

        private void btnDaftarSurvei_Click(object sender, EventArgs e)
        {
            ShowPage(new RiwayatDaftarSurvei());
        }

        private void btnPenghuniLama_Click(object sender, EventArgs e)
        {
            ShowPage(new RiwayatPenghuniLama());
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

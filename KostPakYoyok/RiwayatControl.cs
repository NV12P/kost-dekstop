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
        // =====================================================
        // CONSTRUCTOR
        // =====================================================
        public RiwayatControl()
        {
            InitializeComponent();
        }

        // =====================================================
        // UI LOGIC
        // =====================================================
        private void ShowPage(UserControl page)
        {
            panelRiwayat.Controls.Clear();
            page.Dock = DockStyle.Fill;
            panelRiwayat.Controls.Add(page);
        }

        // =====================================================
        // EVENT HANDLERS
        // =====================================================
        private void RiwayatControl_Load(object sender, EventArgs e)
        {
            ShowPage(new RiwayatPenghuniAktif());
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
            string query = guna2TextBox1.Text;
            var activeControl = panelRiwayat.Controls.Cast<Control>().FirstOrDefault();
            
            if (activeControl is RiwayatPenghuniAktif aktif) aktif.FilterData(query);
            else if (activeControl is RiwayatDaftarReservasi reservasi) reservasi.FilterData(query);
            else if (activeControl is RiwayatDaftarSurvei survei) survei.FilterData(query);
            else if (activeControl is RiwayatPenghuniLama lama) lama.FilterData(query);
        }
    }
}

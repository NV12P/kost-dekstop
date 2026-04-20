using System;
using System.Windows.Forms;

namespace KostPakYoyok
{
    public partial class FormPenyewa : Form
    {
        public FormPenyewa()
        {
            InitializeComponent();
            btnTambah.Click += BtnTambah_Click;
        }

        private void BtnTambah_Click(object sender, EventArgs e)
        {
            // VALIDASI
            if (string.IsNullOrWhiteSpace(textNama.Text))
            {
                MessageBox.Show("Nama penyewa harus diisi!");
                return;
            }

            if (string.IsNullOrWhiteSpace(textNIK.Text))
            {
                MessageBox.Show("NIK penyewa harus diisi!");
                return;
            }

            if (string.IsNullOrWhiteSpace(textNomorTelepon.Text))
            {
                MessageBox.Show("Nomor telepon harus diisi!");
                return;
            }

            if (!int.TryParse(textBulanSewa.Text, out int bulan) || bulan <= 0)
            {
                MessageBox.Show("Jumlah bulan harus angka dan lebih dari 0!");
                return;
            }

            if (comboMetodePembayaran.SelectedItem == null)
            {
                MessageBox.Show("Pilih metode pembayaran!");
                return;
            }

            // RETURN OK ke PenyewaControl
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // ================== PUBLIC DATA ==================

        public string Nama => textNama.Text;

        public string NIK => textNIK.Text;

        public string NomorTelepon => textNomorTelepon.Text;

        public int Bulan
        {
            get
            {
                int.TryParse(textBulanSewa.Text, out int b);
                return b;
            }
        }

        public string Catatan => textCatatan.Text;

        public string MetodePembayaran
        {
            get
            {
                return comboMetodePembayaran.SelectedItem?.ToString()?.ToLower() ?? "tunai";
            }
        }

        public DateTime TanggalMulai => dateMulaiSewa.Value;

        private void textNIK_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

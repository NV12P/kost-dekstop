using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KostPakYoyok
{
    public partial class KeuanganControl : UserControl
    {
        private const string KeuanganApiUrl = "https://kost.arcv.web.id/api/keuangan";
        private int selectedId = 0;

        public KeuanganControl()
        {
            InitializeComponent();

            this.Load += async (s, e) =>
            {
                labelPemasukan.BringToFront();
                labelPengeluaran.BringToFront();
                labelTotal.BringToFront();
                labelPemasukan.Cursor = Cursors.Hand;
                labelPengeluaran.Cursor = Cursors.Hand;

                await LoadKeuanganAsync();

                btnEdit.Visible = false;
                btnTambah.Width = 360;

                // Ini nih mang kabel yang tadi lepas!
                btnEdit.Click += btnEdit_Click;

                LoadUserControl(new IsiRiwayatPengeluaran());

                guna2TextBox1.TextChanged += (sender, ev) =>
                {
                    if (panelIsiRiwayat.Controls.Count > 0)
                    {
                        var currentUC = panelIsiRiwayat.Controls[0];
                        if (currentUC is IsiRiwayatPengeluaran ucP) ucP.FilterData(guna2TextBox1.Text);
                        else if (currentUC is IsiRiwayatPemasukan ucM) ucM.FilterData(guna2TextBox1.Text);
                    }
                };
            };
        }

        private string FormatCurrency(long value) => "Rp " + value.ToString("N0");

        private async Task LoadKeuanganAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);
                    var resp = await client.GetAsync(KeuanganApiUrl);
                    if (!resp.IsSuccessStatusCode) return;

                    var token = JToken.Parse(await resp.Content.ReadAsStringAsync());
                    long m = 0, p = 0;
                    if (token.Type == JTokenType.Array)
                    {
                        foreach (var item in token)
                        {
                            long nom = item["nominal"]?.ToObject<long>() ?? 0;
                            string tip = item["tipe"]?.ToString().ToLower() ?? "";
                            if (tip == "pemasukan") m += nom; else p += nom;
                        }
                    }
                    labelPemasukan.Text = FormatCurrency(m);
                    labelPengeluaran.Text = FormatCurrency(p);
                    long total = m - p;
                    labelTotal.Text = (total >= 0 ? "" : "- ") + FormatCurrency(Math.Abs(total));
                    labelTotal.ForeColor = total >= 0 ? Color.LimeGreen : Color.Red;
                }
            }
            catch { }
        }

        private void LoadUserControl(UserControl uc)
        {
            panelIsiRiwayat.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            panelIsiRiwayat.Controls.Add(uc);

            if (uc is IsiRiwayatPengeluaran riwayat)
            {
                riwayat.SelectionChanged += (s, e) =>
                {
                    if (riwayat.SelectedKeterangan != null)
                    {
                        selectedId = riwayat.SelectedId;
                        textKeterangan.Text = riwayat.SelectedKeterangan;
                        
                        // Pas pilih data, format ke Rp. mang
                        if (long.TryParse(riwayat.SelectedNominal.ToString(), out long n))
                            textNominal.Text = "Rp. " + string.Format("{0:N0}", n).Replace(",", ".");
                        else
                            textNominal.Text = riwayat.SelectedNominal.ToString();

                        btnTambah.Width = 260;
                        btnEdit.Visible = true;
                    }
                    else ResetForm();
                };
            }
            else ResetForm();
        }

        private void ResetForm()
        {
            selectedId = 0;
            textKeterangan.Clear();
            textNominal.Clear();
            btnEdit.Visible = false;
            btnTambah.Width = 360;
        }

        private async void btnTambah_Click(object sender, EventArgs e)
        {
            string ket = textKeterangan.Text.Trim();
            string cleanNom = new string(textNominal.Text.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(ket) || string.IsNullOrEmpty(cleanNom) || !long.TryParse(cleanNom, out long nom))
            {
                MessageBox.Show("Isi data dengan benar mang!");
                return;
            }

            btnTambah.Enabled = false;
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);
                    var data = new { keterangan = ket, nominal = nom };
                    var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                    var resp = await client.PostAsync(KeuanganApiUrl, content);
                    if (resp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Berhasil!");
                        ResetForm();
                        LoadUserControl(new IsiRiwayatPengeluaran());
                        await LoadKeuanganAsync();
                    }
                }
            }
            finally { btnTambah.Enabled = true; }
        }

        private async void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedId == 0) return;
            string ket = textKeterangan.Text.Trim();
            string cleanNom = new string(textNominal.Text.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(ket) || string.IsNullOrEmpty(cleanNom) || !long.TryParse(cleanNom, out long nom))
            {
                MessageBox.Show("Isi data dengan benar mang!");
                return;
            }

            btnEdit.Enabled = false;
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);
                    var data = new { keterangan = ket, nominal = nom };
                    var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                    var resp = await client.PutAsync($"{KeuanganApiUrl}/{selectedId}", content);
                    if (resp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Berhasil diubah!");
                        ResetForm();
                        LoadUserControl(new IsiRiwayatPengeluaran());
                        await LoadKeuanganAsync();
                    }
                }
            }
            finally { btnEdit.Enabled = true; }
        }

        private void textNominal_TextChanged(object sender, EventArgs e)
        {
            textNominal.TextChanged -= textNominal_TextChanged;
            try
            {
                string val = new string(textNominal.Text.Where(char.IsDigit).ToArray());
                if (long.TryParse(val, out long price))
                {
                    textNominal.Text = "Rp. " + string.Format("{0:N0}", price).Replace(",", ".");
                    textNominal.SelectionStart = textNominal.Text.Length;
                }
                else { textNominal.Text = ""; }
            }
            catch { }
            textNominal.TextChanged += textNominal_TextChanged;
        }

        private void labelPemasukan_Click(object sender, EventArgs e) => LoadUserControl(new IsiRiwayatPemasukan());
        private void labelPengeluaran_Click(object sender, EventArgs e) => LoadUserControl(new IsiRiwayatPengeluaran());
    }
}

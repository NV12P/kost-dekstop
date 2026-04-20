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
        public int SelectedId { get; set; }
        public string SelectedKeterangan { get; set; }
        public long SelectedNominal { get; set; }

        public event EventHandler SelectionChanged;

        private const string KeuanganApiUrl = "http://localhost:8000/api/keuangan";

        // ID data yang dipilih untuk edit
        private int selectedId = 0;

        public KeuanganControl()
        {
            InitializeComponent();

            this.Load += async (s, e) =>
            {
                // Label selalu di depan
                labelPemasukan.BringToFront();
                labelPengeluaran.BringToFront();
                labelTotal.BringToFront();

                labelPemasukan.Cursor = Cursors.Hand;
                labelPengeluaran.Cursor = Cursors.Hand;

                await LoadKeuanganAsync();

                // Tombol Edit default hidden
                btnEdit.Visible = false;
                btnTambah.Width = 360;

                // Event tombol edit
                btnEdit.Click += btnEdit_Click;

                // Default buka riwayat pengeluaran
                LoadUserControl(new IsiRiwayatPengeluaran());

                // SEARCH
                guna2TextBox1.TextChanged += (sender, ev) =>
                {
                    if (panelIsiRiwayat.Controls.Count > 0)
                    {
                        var currentUC = panelIsiRiwayat.Controls[0];

                        if (currentUC is IsiRiwayatPengeluaran ucPengeluaran)
                        {
                            ucPengeluaran.FilterData(guna2TextBox1.Text);
                        }
                        else if (currentUC is IsiRiwayatPemasukan ucPemasukan)
                        {
                            ucPemasukan.FilterData(guna2TextBox1.Text);
                        }
                    }
                };
            };
        }

        // ==================================================
        // FORMAT RUPIAH
        // ==================================================
        private string FormatCurrency(long value)
        {
            return "Rp " + value.ToString("N0");
        }

        // ==================================================
        // LOAD DATA KEUANGAN
        // ==================================================
        private async Task LoadKeuanganAsync()
        {
            try
            {
                labelPemasukan.Text = "Loading...";
                labelPengeluaran.Text = "Loading...";
                labelTotal.Text = "Loading...";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);

                    var resp = await client.GetAsync(KeuanganApiUrl);
                    var json = await resp.Content.ReadAsStringAsync();

                    if (!resp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Gagal mengambil data keuangan");
                        return;
                    }

                    var token = JToken.Parse(json);

                    long pemasukan = 0;
                    long pengeluaran = 0;

                    if (token.Type == JTokenType.Array)
                    {
                        foreach (var item in token)
                        {
                            long nominal = item["nominal"]?.ToObject<long>() ?? 0;
                            string tipe = item["tipe"]?.ToString().ToLower() ?? "";

                            // LOGIKA TIPIKAL (Biar sinkron sama dokumentasi 5.1)
                            if (tipe == "pemasukan")
                            {
                                pemasukan += nominal;
                            }
                            else if (tipe == "pengeluaran")
                            {
                                pengeluaran += nominal;
                            }
                        }
                    }

                    long total = pemasukan - pengeluaran;

                    labelPemasukan.Text = FormatCurrency(pemasukan);
                    labelPengeluaran.Text = FormatCurrency(pengeluaran);

                    if (total >= 0)
                    {
                        labelTotal.Text = FormatCurrency(total);
                        labelTotal.ForeColor = Color.LimeGreen;
                    }
                    else
                    {
                        labelTotal.Text = "- " + FormatCurrency(Math.Abs(total));
                        labelTotal.ForeColor = Color.Red;
                    }

                    labelPemasukan.ForeColor = Color.LimeGreen;
                    labelPengeluaran.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // ==================================================
        // LOAD USERCONTROL
        // ==================================================
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
                        textNominal.Text = riwayat.SelectedNominal.ToString();

                        btnTambah.Width = 260;
                        btnEdit.Visible = true;
                    }
                    else
                    {
                        selectedId = 0;
                        textKeterangan.Clear();
                        textNominal.Clear();

                        btnEdit.Visible = false;
                        btnTambah.Width = 360;
                    }
                };
            }
            else
            {
                // Jika buka selain pengeluaran (misal pemasukan yang belum jadi), reset form
                selectedId = 0;
                textKeterangan.Clear();
                textNominal.Clear();
                btnEdit.Visible = false;
                btnTambah.Width = 360;
            }
        }

        // ==================================================
        // TAMBAH DATA
        // ==================================================
        private async void btnTambah_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textKeterangan.Text))
            {
                MessageBox.Show("Keterangan wajib diisi");
                return;
            }

            if (!long.TryParse(textNominal.Text, out long nominal))
            {
                MessageBox.Show("Nominal tidak valid");
                return;
            }

            btnTambah.Enabled = false;

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);

                    var data = new
                    {
                        keterangan = textKeterangan.Text,
                        nominal = nominal
                    };

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);

                    var content = new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json"
                    );

                    var resp = await client.PostAsync(KeuanganApiUrl, content);

                    if (resp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Berhasil ditambahkan");

                        textKeterangan.Clear();
                        textNominal.Clear();

                        LoadUserControl(new IsiRiwayatPengeluaran());
                        await LoadKeuanganAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                btnTambah.Enabled = true;
            }
        }

        // ==================================================
        // EDIT DATA
        // ==================================================
        private async void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedId == 0)
            {
                MessageBox.Show("Pilih data dulu");
                return;
            }

            if (string.IsNullOrWhiteSpace(textKeterangan.Text))
            {
                MessageBox.Show("Keterangan wajib diisi");
                return;
            }

            if (!long.TryParse(textNominal.Text, out long nominal))
            {
                MessageBox.Show("Nominal tidak valid");
                return;
            }

            btnEdit.Enabled = false;

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);

                    var data = new
                    {
                        keterangan = textKeterangan.Text,
                        nominal = nominal
                    };

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);

                    var content = new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json"
                    );

                    var resp = await client.PutAsync(
                        $"{KeuanganApiUrl}/{selectedId}",
                        content
                    );

                    if (resp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Data berhasil diubah");

                        selectedId = 0;

                        textKeterangan.Clear();
                        textNominal.Clear();

                        btnEdit.Visible = false;
                        btnTambah.Width = 360;

                        LoadUserControl(new IsiRiwayatPengeluaran());
                        await LoadKeuanganAsync();
                    }
                    else
                    {
                        MessageBox.Show(await resp.Content.ReadAsStringAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                btnEdit.Enabled = true;
            }
        }

        private void labelPemasukan_Click(object sender, EventArgs e)
        {
            LoadUserControl(new IsiRiwayatPemasukan());
        }

        private void labelPengeluaran_Click(object sender, EventArgs e)
        {
            LoadUserControl(new IsiRiwayatPengeluaran());
        }
    }
}

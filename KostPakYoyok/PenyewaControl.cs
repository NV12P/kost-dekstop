using Guna.UI2.WinForms;
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
    public partial class PenyewaControl : UserControl
    {
        private const string PenyewaApiUrl = "https://kost.arcv.web.id/api/penyewa";

        private JToken selectedRoomItem = null;
        private long selectedRoomPrice = 0;
        private long cicilanSebelumnya = 0;
        private string selectedBuktiPath = "";

        // =====================================================
        // CONSTRUCTOR
        // =====================================================
        public PenyewaControl()
        {
            InitializeComponent();

            guna2ShadowPanel2.AutoScroll = true;

            guna2ShadowPanel3.Visible = false;
            this.Load += async (s, e) => await LoadPenyewaAsync();

            textBulanSewa.TextChanged += (s, e) => Recalculate();
            textTotalCicilan.TextChanged += (s, e) => Recalculate();

            btnTambahBulan.Click += (s, e) =>
            {
                if (int.TryParse(textBulanSewa.Text, out int v))
                    textBulanSewa.Text = (v + 1).ToString();
                else
                    textBulanSewa.Text = "1";
            };

            btnKurangBulan.Click += (s, e) =>
            {
                if (int.TryParse(textBulanSewa.Text, out int v) && v > 1)
                    textBulanSewa.Text = (v - 1).ToString();
            };

            btnSimpan.Click += async (s, e) => await SavePenyewaChangesAsync();
            btnAkhiriSewa.Click += async (s, e) => await EndLeaseAsync();
        }

        // =====================================================
        // LOAD LIST KAMAR
        // =====================================================
        private async Task LoadPenyewaAsync()
        {
            try
            {
                guna2ShadowPanel2.Controls.Clear();

                Label header = new Label()
                {
                    Text = "Status Kamar",
                    Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                    ForeColor = Color.FromArgb(26, 18, 101),
                    Location = new Point(20, 15),
                    AutoSize = true
                };

                guna2ShadowPanel2.Controls.Add(header);

                using (var c = new HttpClient())
                {
                    c.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);

                    var resp = await c.GetAsync(PenyewaApiUrl);
                    var json = await resp.Content.ReadAsStringAsync();

                    var token = JToken.Parse(json);
                    JArray arr = token as JArray;

                    int y = 60;
                    int rowHeight = 55;

                    foreach (var item in arr)
                    {
                        guna2ShadowPanel2.Controls.Add(CreateRoomRow(item, y));
                        y += rowHeight;
                    }

                    guna2ShadowPanel2.Height = y + 20;

                    labelDataKamar.Text = arr.Count.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        // =====================================================
        // ROW KAMAR
        // =====================================================
        private Panel CreateRoomRow(JToken item, int y)
        {
            string status = item["status"]?.ToString().ToLower() ?? "tersedia";
            string rawNamaKamar = item["nama"]?.ToString() ?? "Kamar";
            
            string namaKamar = System.Text.RegularExpressions.Regex.Replace(rawNamaKamar, @"^\d+\s*\.?[ ]*", "");
            if (namaKamar.Contains("amar") && !namaKamar.Contains("Kamar"))
            {
                namaKamar = namaKamar.Replace("amar", "Kamar");
            }
            if (namaKamar.StartsWith("KamarKamar")) 
            {
                namaKamar = namaKamar.Replace("KamarKamar", "Kamar");
            }

            Panel p = new Panel()
            {
                Location = new Point(8, y),
                Size = new Size(guna2ShadowPanel2.Width - 24, 48)
            };

            p.Controls.Add(new Label()
            {
                Text = namaKamar,
                Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 18, 101),
                Location = new Point(15, 12),
                AutoSize = true
            });

            p.Controls.Add(new Label()
            {
                Text = status == "tersedia" ? "Tersedia" : "Disewa",
                ForeColor = status == "tersedia" ? Color.LimeGreen : Color.Red,
                Location = new Point(180, 13),
                AutoSize = true
            });

            Guna2Button btn = new Guna2Button()
            {
                Size = new Size(110, 34),
                Location = new Point(p.Width - 125, 7),
                BorderRadius = 16,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            if (status == "tersedia")
            {
                btn.Text = "Tambah";
                btn.FillColor = Color.LimeGreen;
                btn.Click += async (s, e) => await OpenFormTambah(item);
            }
            else
            {
                btn.Text = "Informasi";
                btn.FillColor = Color.FromArgb(26, 18, 101);

                string kamarFix = namaKamar;

                btn.Click += (s, e) => ShowPenyewaDetails(item, kamarFix);
            }

            p.Controls.Add(btn);
            return p;
        }

        // =====================================================
        // DETAIL PENYEWA
        // =====================================================
        private void ShowPenyewaDetails(JToken roomItem, string nomorKamar)
        {
            selectedRoomItem = roomItem;

            var penyewa = roomItem["penyewa"];
            if (penyewa == null) return;

            selectedRoomPrice = roomItem["harga"]?.ToObject<long>() ?? roomItem["harga_kamar_perbulan"]?.ToObject<long>() ?? 0;

            string idKamar = roomItem["id"]?.ToString() ?? "-";

            label21.Text = "Informasi Data Kamar " + nomorKamar; 

            textNama.Text = penyewa["nama"]?.ToString() ?? "";
            textNomorTelepon.Text = penyewa["telp"]?.ToString() ?? "";
            textBulanSewa.Text = penyewa["sewabrpbulan"]?.ToString() ?? "1";
            textCatatan.Text = penyewa["catatan"]?.ToString() ?? "";

            string metode = (penyewa["metodepembayaran"] ?? penyewa["metode_pembayaran"])?.ToString().ToLower() ?? "";
            
            if (metode.Contains("tunai"))
            {
                btnBukti.Visible = false;
                textTotalCicilan.Width = 290; 
            }
            else
            {
                btnBukti.Visible = true;
                textTotalCicilan.Width = 240; 
            }

            long cicilanDariTabel = penyewa["cicilan"]?.Sum(x => x["nominal"]?.ToObject<long?>() ?? 0) ?? 0;
            long totalCicilanField = penyewa["total_cicilan"]?.ToObject<long?>() ?? 0;

            cicilanSebelumnya = Math.Max(cicilanDariTabel, totalCicilanField);

            textTotalCicilan.Text = "0";

            selectedBuktiPath = "";
            btnBukti.FillColor = Color.FromArgb(183, 188, 196);
            btnBukti.ForeColor = Color.FromArgb(26, 18, 101);
            btnBukti.Text = "";

            guna2ShadowPanel3.Visible = true;

            Recalculate();
        }

        // =====================================================
        // HITUNG CICILAN
        // =====================================================
        private void Recalculate()
        {
            if (selectedRoomItem == null) return;

            int.TryParse(textBulanSewa.Text, out int bulan);
            long.TryParse(textTotalCicilan.Text, out long inputCicilanBaru);

            long totalHarga = selectedRoomPrice * bulan;

            long totalSudahBayar = cicilanSebelumnya + inputCicilanBaru;

            long sisa = totalHarga - totalSudahBayar;

            if (sisa < 0)
                sisa = 0;

            labelTotalCicilan.Text = "Rp. " + totalSudahBayar.ToString("N0");

            labelHarusbayar.Text = "Rp. " + sisa.ToString("N0");

            if (sisa == 0)
                labelHarusbayar.ForeColor = Color.FromArgb(26, 18, 101);
            else
                labelHarusbayar.ForeColor = Color.Red;

            label7.Text = "Total yang Harus Dibayar";
        }

        // =====================================================
        // SIMPAN PERUBAHAN
        // =====================================================
        private async Task SavePenyewaChangesAsync()
        {
            if (selectedRoomItem == null) return;

            try
            {
                btnSimpan.Enabled = false;

                int.TryParse(textBulanSewa.Text, out int bulan);
                long.TryParse(textTotalCicilan.Text, out long cicilanBaru);

                using (var c = new HttpClient())
                {
                    c.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);

                    var pData = selectedRoomItem["penyewa"];
                    string nama = textNama.Text;
                    string telp = textNomorTelepon.Text;
                    string metode = (pData["metodepembayaran"] ?? pData["metode_pembayaran"])?.ToString() ?? "transfer";
                    
                    string status = (pData["statuspembayaran"] ?? pData["status_pembayaran"])?.ToString() ?? "pending";

                    var form = new MultipartFormDataContent();

                    form.Add(new StringContent(nama), "nama_penyewa");
                    form.Add(new StringContent(telp), "telp_penyewa");
                    form.Add(new StringContent(bulan.ToString()), "sewa_berapa_bulan");
                    form.Add(new StringContent(metode), "metode_pembayaran");
                    form.Add(new StringContent(status), "status_pembayaran");
                    
                    form.Add(new StringContent(textCatatan.Text), "catatan");
                    
                    if (cicilanBaru > 0)
                    {
                        form.Add(new StringContent(cicilanBaru.ToString()), "cicilan");
                    }

                    form.Add(new StringContent("PUT"), "_method");

                    if (!string.IsNullOrWhiteSpace(selectedBuktiPath))
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(selectedBuktiPath);

                        var fileContent = new ByteArrayContent(bytes);
                        fileContent.Headers.ContentType =
                            new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                        form.Add(
                            fileContent,
                            "bukti_cicilan",
                            System.IO.Path.GetFileName(selectedBuktiPath)
                        );
                    }

                    var resp = await c.PostAsync(
                        $"{PenyewaApiUrl}/{selectedRoomItem["id"]}",
                        form
                    );

                    var result = await resp.Content.ReadAsStringAsync();

                    if (resp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Data berhasil disimpan");

                        selectedBuktiPath = "";
                        btnBukti.Text = ""; 
                        btnBukti.FillColor = Color.FromArgb(241, 241, 241);
                        btnBukti.ForeColor = Color.FromArgb(26, 18, 101);

                        await LoadPenyewaAsync();
                    }
                    else
                    {
                        MessageBox.Show(result);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                btnSimpan.Enabled = true;
            }
        }

        // =====================================================
        // AKHIRI SEWA
        // =====================================================
        private async Task EndLeaseAsync()
        {
            if (selectedRoomItem == null) return;

            if (MessageBox.Show(
                "Yakin ingin mengakhiri sewa?",
                "Konfirmasi",
                MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            try
            {
                btnAkhiriSewa.Enabled = false;

                using (var c = new HttpClient())
                {
                    c.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);

                    var content = new MultipartFormDataContent();
                    content.Add(new StringContent("PUT"), "_method");

                    var resp = await c.PostAsync(
                        $"{PenyewaApiUrl}/akhiri/{selectedRoomItem["id"]}",
                        content
                    );

                    if (resp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Sewa berhasil diakhiri");
                        guna2ShadowPanel3.Visible = false;
                        await LoadPenyewaAsync();
                    }
                    else
                    {
                        MessageBox.Show(await resp.Content.ReadAsStringAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                btnAkhiriSewa.Enabled = true;
            }
        }

        // =====================================================
        // FORM TAMBAH PENYEWA
        // =====================================================
        private async Task OpenFormTambah(JToken roomItem)
        {
            FormUtama parentForm = this.FindForm() as FormUtama;
            if (parentForm == null) return;

            Form formbackground = new Form();
            try
            {
                using (FormPenyewa form = new FormPenyewa())
                {
                    formbackground.StartPosition = FormStartPosition.Manual;
                    formbackground.FormBorderStyle = FormBorderStyle.None;
                    formbackground.Opacity = 0.30d;
                    formbackground.BackColor = Color.Black;
                    formbackground.Size = parentForm.Size;
                    formbackground.Location = parentForm.Location;
                    formbackground.ShowInTaskbar = false;
                    formbackground.TopMost = false;
                    formbackground.Show();

                    form.StartPosition = FormStartPosition.Manual;
                    form.Owner = formbackground;
                    form.Location = new Point(
                        parentForm.Location.X + (parentForm.Width - form.Width) / 2,
                        parentForm.Location.Y + (parentForm.Height - form.Height) / 2
                    );

                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        using (var c = new HttpClient())
                        {
                            c.DefaultRequestHeaders.Authorization =
                                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);

                            // =====================================================
                            // VALIDASI NIK UNIK
                            // =====================================================
                            try
                            {
                                var checkResp = await c.GetAsync("https://kost.arcv.web.id/api/riwayat");
                                if (checkResp.IsSuccessStatusCode)
                                {
                                    var riwayatJson = await checkResp.Content.ReadAsStringAsync();
                                    var riwayatArr = JArray.Parse(riwayatJson);
                                    
                                    bool exists = riwayatArr.Any(x => 
                                        x["nik"]?.ToString() == form.NIK && 
                                        x["kategori"]?.ToString() != "lama" // Hanya cek yang masih relevan (aktif/booking/survei)
                                    );

                                    if (exists)
                                    {
                                        MessageBox.Show($"NIK '{form.NIK}' sudah terdaftar di sistem dan masih aktif. Silakan gunakan NIK lain atau cek riwayat.", "NIK Tidak Unik", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        return;
                                    }
                                }
                            }
                            catch { /* Abaikan error network pas ngecek, lanjut ke post aja nanti server yang nolak kalo emang duplikat */ }

                            var payload = new
                            {
                                kamar_id = roomItem["id"],
                                nama_penyewa = form.Nama,
                                nik_penyewa = form.NIK,
                                telp_penyewa = form.NomorTelepon,
                                tglsewa_sewa = form.TanggalMulai.ToString("yyyy-MM-dd"),
                                sewa_berapa_bulan = form.Bulan,
                                metode_pembayaran = form.MetodePembayaran,
                                catatan = form.Catatan
                            };

                            var content = new StringContent(
                                JObject.FromObject(payload).ToString(),
                                Encoding.UTF8,
                                "application/json"
                            );

                            var resp = await c.PostAsync(PenyewaApiUrl, content);

                            if (resp.IsSuccessStatusCode)
                            {
                                MessageBox.Show("Penyewa berhasil ditambahkan", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                await LoadPenyewaAsync();
                            }
                            else
                            {
                                MessageBox.Show(await resp.Content.ReadAsStringAsync(), "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                formbackground.Dispose();
            }
        }

        // =====================================================
        // UI EVENT HANDLERS
        // =====================================================
        private void guna2ShadowPanel3_Paint(object sender, PaintEventArgs e) { }
        private void PenyewaControl_Load(object sender, EventArgs e) { }

        private void btnBukti_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(selectedBuktiPath))
            {
                MessageBox.Show("Foto sudah dipilih! Jika ingin mengganti, silakan simpan data dulu atau tutup lalu buka kembali menu informasi ini.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Pilih Foto Bukti";
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    selectedBuktiPath = ofd.FileName;

                    btnBukti.FillColor = Color.LimeGreen;
                    btnBukti.ForeColor = Color.Black;

                    MessageBox.Show("Foto bukti berhasil dipilih.");
                }
            }
        }

        private void textTotalCicilan_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using System.IO;

namespace KostPakYoyok
{
    public partial class RiwayatPenghuniLama : UserControl
    {
        private const string ApiUrl = "https://kost.arcv.web.id/api/riwayat";
        private static System.Globalization.CultureInfo cultureIndo = new System.Globalization.CultureInfo("id-ID");
        
        private Panel activeEntry = null;
        private Guna2Panel activeSubDetail = null;
        private Label activeSubArrow = null;

        public RiwayatPenghuniLama()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.AutoScroll = true;
            this.BackColor = SystemColors.ControlLight; 

            this.Load += async (s, e) => {
                await LoadRiwayatLamaAsync();
                ReLayoutAll(); 
            };
            this.SizeChanged += (s, e) => ReLayoutAll(); 
        }

        private async Task LoadRiwayatLamaAsync()
        {
            try {
                this.SuspendLayout();
                var controlsToRemove = this.Controls.Cast<Control>().Where(c => c.Name != null && c.Name.StartsWith("Entry_")).ToList();
                foreach (var c in controlsToRemove) this.Controls.Remove(c);

                using (var client = new HttpClient()) {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);
                    var resp = await client.GetAsync(ApiUrl);
                    if (!resp.IsSuccessStatusCode) return;

                    var arr = JArray.Parse(await resp.Content.ReadAsStringAsync());
                    int index = 1;

                    // FILTER KHUSUS PENGHUNI LAMA (KATEGORI: LAMA) MANG!
                    var lamaData = arr.Where(x => x["kategori"]?.ToString() == "lama").ToList();

                    foreach (var item in lamaData) {
                        var row = CreateRow(item, index++, 0);
                        this.Controls.Add(row);
                    }
                }
            } catch (Exception ex) { 
                MessageBox.Show("Kendala Sinkronisasi: " + ex.Message); 
            }
            finally { 
                ReLayoutAll();
                this.ResumeLayout(true);
            }
        }

        private Panel CreateRow(JToken item, int index, int y)
        {
            var detail = item["detail"];
            DateTime tglMulai; DateTime.TryParse(item["tanggal"]?.ToString(), out tglMulai);
            
            int cardW = 1400;
            var container = new Panel { Name = "Entry_" + index, Size = new Size(cardW, 115), BackColor = Color.Transparent };

            var pU = new Guna2Panel { Name = "DynHeader", Size = new Size(cardW - 20, 105), Location = new Point(10, 0), BorderRadius = 14, FillColor = Color.White, BorderThickness = 1, BorderColor = Color.FromArgb(226, 232, 240), Cursor = Cursors.Hand };
            
            var btnNo = new Guna2Button { Size = new Size(65, 65), Location = new Point(25, 20), BorderRadius = 14, FillColor = Color.FromArgb(26, 18, 101), ForeColor = Color.White, Font = new Font("Segoe UI", 18, FontStyle.Bold), Text = index.ToString() };
            var lblNama = new Label { Text = (item["penyewa"]?.ToString() ?? "User").ToLower(), Location = new Point(105, 48), Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(26, 18, 101), AutoSize = true };
            var lblNIK = new Label { Text = "(NIK: " + (item["nik"]?.ToString() ?? "-") + ")", Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold), ForeColor = Color.FromArgb(26, 18, 101), AutoSize = true };
            var lblTgl = new Label { Text = tglMulai.ToString("dddd, dd/MM/yyyy", cultureIndo), Location = new Point(195, 23), Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold), ForeColor = SystemColors.ControlDarkDark, AutoSize = true };
            var btnLama = new Guna2Button { Size = new Size(80, 22), Location = new Point(105, 21), BorderRadius = 8, FillColor = SystemColors.Control, ForeColor = Color.FromArgb(26, 18, 101), Font = new Font("Segoe UI", 8, FontStyle.Bold), Text = "LAMA" };
            var btnKlik = new Guna2Button { Size = new Size(110, 22), Location = new Point(335, 21), BorderRadius = 8, FillColor = SystemColors.Control, ForeColor = Color.DarkGray, Font = new Font("Segoe UI", 8, FontStyle.Bold), Text = "Klik Detail ▲" };
            
            // STATUS Check-Out MANG!
            var btnStatus = new Guna2Button { Size = new Size(115, 38), Location = new Point(pU.Width - 145, 33), BorderRadius = 14, FillColor = Color.FromArgb(254, 242, 242), ForeColor = Color.Red, Font = new Font("Segoe UI", 10, FontStyle.Bold), Text = "Check-Out" };

            pU.Controls.Add(btnNo); pU.Controls.Add(lblNama); pU.Controls.Add(lblNIK); pU.Controls.Add(lblTgl); pU.Controls.Add(btnLama); pU.Controls.Add(btnKlik); pU.Controls.Add(btnStatus);
            lblNIK.Location = new Point(lblNama.Right + 10, 58);

            var pD = new Guna2Panel { Name = "DynDetail", Size = new Size(cardW - 50, 480), Location = new Point(25, 90), BorderRadius = 14, BorderThickness = 1, BorderColor = SystemColors.ControlDark, FillColor = Color.FromArgb(248, 250, 252), Visible = false };

            BuildDetailInner(pD, item, tglMulai, container);

            Action toggle = () => {
                this.SuspendLayout();
                if (activeEntry != null && activeEntry != container) {
                    var oD = activeEntry.Controls.Find("DynDetail", true).FirstOrDefault();
                    if (oD != null) oD.Visible = false;
                    activeEntry.Height = 115;
                    var oB = activeEntry.Controls.Find("DynHeader", true).FirstOrDefault()?.Controls.Cast<Control>().FirstOrDefault(b => b is Guna2Button && b.Text.Contains("Detail")) as Guna2Button;
                    if (oB != null) oB.Text = "Klik Detail ▲";
                }
                pD.Visible = !pD.Visible;
                btnKlik.Text = pD.Visible ? "Klik Detail ▼" : "Klik Detail ▲";
                container.Height = pD.Visible ? pD.Bottom + 10 : 115;
                activeEntry = pD.Visible ? container : null;
                ReLayoutAll();
                this.ResumeLayout(true);
            };

            pU.Click += (s, e) => toggle();
            btnKlik.Click += (s, e) => toggle();

            container.Controls.Add(pU);
            container.Controls.Add(pD);
            pU.BringToFront();
            pD.SendToBack();
            return container;
        }

        private void BuildDetailInner(Guna2Panel pD, JToken item, DateTime start, Panel container)
        {
            var d = item["detail"];
            var cicilanArr = d["cicilan"] as JArray ?? new JArray();
            
            // Hitung total harga & bayar
            long hargaPerBulan = 400000;
            string durasiText = d["durasi"]?.ToString() ?? "1 Bulan";
            int totalBulan = int.Parse(new string(durasiText.Where(char.IsDigit).ToArray()) ?? "1");
            long totalHargaTagihan = hargaPerBulan * totalBulan;

            long totalBayar = 0;
            foreach(var c in cicilanArr) {
                string nomStr = new string(c["nominal"]?.ToString().Where(char.IsDigit).ToArray());
                totalBayar += long.TryParse(nomStr, out long n) ? n : 0;
            }

            // PANEL LOKASI KAMAR
            var p1 = new Guna2Panel { Size = new Size(580, 95), Location = new Point(35, 40), BorderRadius = 14, FillColor = Color.White, BorderColor = Color.FromArgb(226, 232, 240), BorderThickness = 1 };
            var btnHouse = new Guna2Button { Size = new Size(65, 65), Location = new Point(15, 15), BorderRadius = 14, FillColor = Color.FromArgb(26, 18, 101) };
            try {
                string[] possiblePaths = { "house.png", Path.Combine(Application.StartupPath, "house.png"), Path.Combine(Application.StartupPath, "..\\..\\house.png"), Path.Combine(Application.StartupPath, "..\\..\\..\\house.png") };
                foreach(string path in possiblePaths) { if (File.Exists(path)) { btnHouse.Image = Image.FromFile(path); btnHouse.ImageSize = new Size(30, 30); break; } }
            } catch { }
            p1.Controls.Add(btnHouse);
            p1.Controls.Add(new Label { Text = "LOKASI KAMAR TERAKHIR", Location = new Point(95, 20), Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = SystemColors.ControlDarkDark, AutoSize = true });
            p1.Controls.Add(new Label { Text = (item["kamar"]?.ToString() ?? "KAMAR").ToUpper(), Location = new Point(93, 45), Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(26, 18, 101), AutoSize = true });
            pD.Controls.Add(p1);

            // PANEL STATUS
            var p3 = new Guna2Panel { Size = new Size(280, 130), Location = new Point(35, 150), BorderRadius = 14, FillColor = Color.White, BorderColor = Color.FromArgb(226, 232, 240), BorderThickness = 1 };
            p3.Controls.Add(new Label { Text = "RIWAYAT TAGIHAN", Location = new Point(20, 20), Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = SystemColors.ControlDarkDark, AutoSize = true });
            bool isLunas = totalBayar >= totalHargaTagihan;
            p3.Controls.Add(new Label { Text = isLunas ? "✔️ Lunas" : "❌ BELUM LUNAS", Location = new Point(18, 50), Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = isLunas ? Color.LimeGreen : Color.Red, AutoSize = true });
            p3.Controls.Add(new Label { Text = "TOTAL BIAYA: Rp. " + totalHargaTagihan.ToString("N0", cultureIndo), Location = new Point(20, 95), Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(26, 18, 101), AutoSize = true });
            pD.Controls.Add(p3);

            var p4 = new Guna2Panel { Size = new Size(280, 130), Location = new Point(335, 150), BorderRadius = 14, FillColor = Color.White, BorderColor = Color.FromArgb(226, 232, 240), BorderThickness = 1 };
            p4.Controls.Add(new Label { Text = "DURASI SEWA", Location = new Point(20, 20), Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = SystemColors.ControlDarkDark, AutoSize = true });
            p4.Controls.Add(new Label { Text = totalBulan + " BULAN", Location = new Point(18, 50), Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(26, 18, 101), AutoSize = true });
            p4.Controls.Add(new Label { Text = "TELAH BERAKHIR", Location = new Point(20, 95), Font = new Font("Segoe UI", 8, FontStyle.Bold | FontStyle.Italic), ForeColor = Color.Red, AutoSize = true });
            pD.Controls.Add(p4);

            var pBHeader = new Guna2Panel { Size = new Size(580, 60), Location = new Point(35, 300), BorderRadius = 14, FillColor = Color.White, BorderColor = Color.FromArgb(226, 232, 240), BorderThickness = 1, Cursor = Cursors.Hand };
            pBHeader.Controls.Add(new Label { Text = "RINCIAN BULAN SEWA", Location = new Point(25, 20), Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = SystemColors.ControlDark, AutoSize = true });
            var lblArr = new Label { Text = "▲", Location = new Point(530, 18), Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(26, 18, 101), AutoSize = true };
            pBHeader.Controls.Add(lblArr);
            pD.Controls.Add(pBHeader);

            var pBContent = new Guna2Panel { Size = new Size(580, 200), Location = new Point(35, 365), BorderRadius = 14, FillColor = Color.White, BorderColor = Color.FromArgb(226, 232, 240), BorderThickness = 1, Visible = false, AutoScroll = true };
            int cyB = 20; long sSaldo = totalBayar;
            for (int i = 0; i < totalBulan; i++) {
                DateTime b = start.AddMonths(i);
                bool lBln = sSaldo >= hargaPerBulan;
                long kura = lBln ? 0 : (hargaPerBulan - sSaldo);
                sSaldo = lBln ? (sSaldo - hargaPerBulan) : 0;
                pBContent.Controls.Add(new Label { Text = b.ToString("MMMM yyyy", cultureIndo).ToUpper(), Location = new Point(25, cyB), Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.DimGray, AutoSize = true });
                pBContent.Controls.Add(new Guna2Button { Text = lBln ? "✔️ Lunas" : "❌ Belum Lunas", Size = new Size(120, 30), Location = new Point(430, cyB - 2), BorderRadius = 14, FillColor = lBln ? Color.FromArgb(220, 252, 231) : Color.FromArgb(254, 242, 242), ForeColor = lBln ? Color.LimeGreen : Color.Red, Font = new Font("Segoe UI", 7, FontStyle.Bold) });
                cyB += 45;
                if (!lBln) { pBContent.Controls.Add(new Label { Text = "* Masih kurang Rp. " + kura.ToString("N0", cultureIndo) + " untuk bulan ini", Location = new Point(25, cyB), Font = new Font("Segoe UI", 9, FontStyle.Bold | FontStyle.Italic), ForeColor = Color.Red, AutoSize = true }); cyB += 35; }
                cyB += 15;
            }
            pBContent.Height = Math.Min(400, cyB + 10); pD.Controls.Add(pBContent);

            pBHeader.Click += (s, e) => {
                this.SuspendLayout();
                if (!pBContent.Visible) { if (activeSubDetail != null && activeSubDetail != pBContent) { activeSubDetail.Visible = false; if (activeSubArrow != null) activeSubArrow.Text = "▲"; } activeSubDetail = pBContent; activeSubArrow = lblArr; }
                pBContent.Visible = !pBContent.Visible; lblArr.Text = pBContent.Visible ? "▼" : "▲";
                UpdateDHeight(pD); container.Height = pD.Bottom + 10; ReLayoutAll();
                this.ResumeLayout(true);
            };

            pD.Controls.Add(new Label { Text = "RIWAYAT CICILAN", Location = new Point(660, 35), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = SystemColors.ControlDarkDark, AutoSize = true });
            int cyC = 70; int cIndex = 1;
            foreach (var cData in cicilanArr) {
                var pC = CreateCicilanCard(cData, cyC, pD.Width - 700, container, pD, cIndex++);
                pD.Controls.Add(pC);
                cyC += pC.Height + 12;
            }
            UpdateDHeight(pD);
        }

        private Guna2Panel CreateCicilanCard(JToken d, int y, int w, Panel container, Guna2Panel pD, int idx)
        {
            var p = new Guna2Panel { Size = new Size(w, 110), Location = new Point(660, y), BorderRadius = 14, FillColor = Color.White, BorderColor = Color.FromArgb(226, 232, 240), BorderThickness = 1 };
            p.Controls.Add(new Label { Text = "CICILAN KE-" + idx, Location = new Point(20, 15), Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = SystemColors.ControlDarkDark, AutoSize = true });
            var btnB = new Guna2Button { Text = "Bukti", Location = new Point(p.Width - 100, 12), Size = new Size(80, 30), BorderRadius = 12, FillColor = Color.FromArgb(26, 18, 101), ForeColor = Color.White, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            p.Controls.Add(new Label { Text = d["nominal"]?.ToString() ?? "Rp 0", Location = new Point(20, 42), Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(26, 18, 101), AutoSize = true });
            DateTime tc; DateTime.TryParse(d["tanggal"]?.ToString(), out tc);
            p.Controls.Add(new Label { Text = tc.ToString("dddd, dd/MM/yyyy", cultureIndo), Location = new Point(20, 80), Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = SystemColors.ControlDarkDark, AutoSize = true });
            
            string buktiUrl = d["bukti"]?.ToString();
            bool hasImage = !string.IsNullOrEmpty(buktiUrl);
            if (hasImage) {
                if (!buktiUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)) {
                    string cleanPath = buktiUrl.Replace("\\", "/").TrimStart('/');
                    if (cleanPath.StartsWith("storage/", StringComparison.OrdinalIgnoreCase)) buktiUrl = "https://kost.arcv.web.id/" + cleanPath;
                    else buktiUrl = "https://kost.arcv.web.id/storage/" + cleanPath;
                } else buktiUrl = buktiUrl.Replace("http://", "https://");
            }

            var pic = new Guna2PictureBox { Size = new Size(p.Width - 40, 240), Location = new Point(20, 115), BorderRadius = 14, Visible = false, SizeMode = PictureBoxSizeMode.CenterImage, BackColor = Color.FromArgb(241, 245, 249), Cursor = Cursors.Hand };
            var pNoImg = new Guna2Panel { Size = new Size(p.Width - 40, 60), Location = new Point(20, 115), BorderRadius = 10, FillColor = Color.FromArgb(254, 242, 242), Visible = false };
            var lblNoImg = new Label { Text = hasImage ? "Memuat gambar..." : "Gambar tidak diunggah", Font = new Font("Segoe UI", 9, FontStyle.Bold | FontStyle.Italic), ForeColor = Color.Red, AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            pNoImg.Controls.Add(lblNoImg);

            btnB.Click += async (s, e) => {
                this.SuspendLayout();
                if (hasImage) { 
                    if (pic.Image == null) {
                        pNoImg.Visible = true; lblNoImg.Text = "Memuat gambar...";
                        try {
                            using (var client = new HttpClient()) {
                                var bytes = await client.GetByteArrayAsync(buktiUrl);
                                using (var ms = new System.IO.MemoryStream(bytes)) pic.Image = Image.FromStream(ms);
                            }
                            pNoImg.Visible = false; pic.Visible = true;
                        } catch { lblNoImg.Text = "Gagal memuat gambar"; pNoImg.Visible = true; pic.Visible = false; }
                    } else pic.Visible = !pic.Visible;
                    p.Height = (pic.Visible || pNoImg.Visible) ? (pic.Visible ? 370 : 190) : 110; 
                } else { pNoImg.Visible = !pNoImg.Visible; p.Height = pNoImg.Visible ? 190 : 110; }
                bool tutup = pic.Visible || pNoImg.Visible;
                btnB.Text = tutup ? "Tutup" : "Bukti"; btnB.FillColor = tutup ? Color.FromArgb(254, 242, 242) : Color.FromArgb(26, 18, 101); btnB.ForeColor = tutup ? Color.Red : Color.White;
                UpdateDHeight(pD); container.Height = pD.Bottom + 10; ReLayoutAll(); this.ResumeLayout(true);
            };

            pic.Click += (s, e) => {
                if (pic.Image != null) {
                    Form mainForm = this.FindForm();
                    using (OverlayForm overlay = new OverlayForm(mainForm)) {
                        overlay.Show();
                        using (FormZoomGambar zoom = new FormZoomGambar(pic.Image)) { zoom.Owner = overlay; zoom.ShowDialog(); }
                    }
                }
            };

            p.Controls.Add(btnB); p.Controls.Add(pic); p.Controls.Add(pNoImg);
            return p;
        }

        private void UpdateDHeight(Guna2Panel d) {
            int max = 450;
            foreach (Control c in d.Controls) if (c.Visible && c.Bottom > max) max = c.Bottom;
            d.Height = max + 35; 
        }

        private void ReLayoutAll()
        {
            this.SuspendLayout();
            Point scrollPos = this.AutoScrollPosition;
            this.AutoScrollPosition = new Point(0, 0);
            int curY = 25; int cardW = 1400;
            int cX = (this.ClientSize.Width - cardW) / 2;
            if (cX < 20) cX = 20;
            var entries = this.Controls.Cast<Control>().Where(x => x.Name != null && x.Name.StartsWith("Entry_")).OrderBy(x => int.Parse(x.Name.Replace("Entry_", ""))).ToList();
            foreach (Control c in entries) { c.Location = new Point(cX, curY); curY += c.Height + 15; }
            this.AutoScrollPosition = new Point(Math.Abs(scrollPos.X), Math.Abs(scrollPos.Y));
            this.ResumeLayout(false);
        }
    }
}

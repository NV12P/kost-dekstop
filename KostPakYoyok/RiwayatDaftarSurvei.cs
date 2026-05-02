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
    public partial class RiwayatDaftarSurvei : UserControl
    {
        private const string ApiUrl = "https://kost.arcv.web.id/api/riwayat";
        private const string AcceptUrl = "https://kost.arcv.web.id/api/survei/accept/";
        private const string RejectUrl = "https://kost.arcv.web.id/api/survei/reject/";
        private static System.Globalization.CultureInfo cultureIndo = new System.Globalization.CultureInfo("id-ID");

        private Panel activeEntry = null;

        public RiwayatDaftarSurvei()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.AutoScroll = true;
            this.BackColor = SystemColors.ControlLight;

            this.Load += async (s, e) => {
                await LoadRiwayatSurveiAsync();
                ReLayoutAll();
            };
            this.SizeChanged += (s, e) => ReLayoutAll();
        }

        private async Task LoadRiwayatSurveiAsync()
        {
            try
            {
                this.SuspendLayout();
                var controlsToRemove = this.Controls.Cast<Control>().Where(c => c.Name != null && c.Name.StartsWith("Entry_")).ToList();
                foreach (var c in controlsToRemove) this.Controls.Remove(c);

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);
                    var resp = await client.GetAsync(ApiUrl);
                    if (!resp.IsSuccessStatusCode) return;

                    var arr = JArray.Parse(await resp.Content.ReadAsStringAsync());
                    int index = 1;

                    var surveiData = arr.Where(x => x["kategori"]?.ToString() == "survei").ToList();

                    foreach (var item in surveiData)
                    {
                        var row = CreateRow(item, index++, 0);
                        this.Controls.Add(row);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal Memuat: " + ex.Message); }
            finally
            {
                ReLayoutAll();
                this.ResumeLayout(true);
            }
        }

        private Panel CreateRow(JToken item, int index, int y)
        {
            DateTime tgl; DateTime.TryParse(item["tanggal"]?.ToString(), out tgl);

            int cardW = 1400;
            var container = new Panel { Name = "Entry_" + index, Size = new Size(cardW, 115), BackColor = Color.Transparent };

            var pU = new Guna2Panel { Name = "DynHeader", Size = new Size(cardW - 20, 105), Location = new Point(10, 0), BorderRadius = 14, FillColor = Color.White, BorderThickness = 1, BorderColor = Color.FromArgb(226, 232, 240), Cursor = Cursors.Hand };

            var btnNo = new Guna2Button { Size = new Size(65, 65), Location = new Point(25, 20), BorderRadius = 14, FillColor = SystemColors.Control, ForeColor = Color.FromArgb(26, 18, 101), Font = new Font("Segoe UI", 18, FontStyle.Bold), Text = index.ToString() };
            var lblNama = new Label { Text = (item["penyewa"]?.ToString() ?? "User").ToLower(), Location = new Point(105, 48), Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(26, 18, 101), AutoSize = true };
            var lblTgl = new Label { Text = tgl.ToString("dddd, dd/MM/yyyy", cultureIndo), Location = new Point(195, 23), Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold), ForeColor = SystemColors.ControlDarkDark, AutoSize = true };
            
            // Design Simpel: Tag SURVEI lebih kecil mang
            var btnSurveiTag = new Guna2Button { Size = new Size(70, 20), Location = new Point(105, 22), BorderRadius = 6, FillColor = Color.FromArgb(255, 248, 225), ForeColor = Color.FromArgb(255, 160, 0), Font = new Font("Segoe UI", 7, FontStyle.Bold), Text = "SURVEI" };
            var btnKlik = new Guna2Button { Size = new Size(110, 20), Location = new Point(335, 22), BorderRadius = 6, FillColor = SystemColors.Control, ForeColor = Color.DarkGray, Font = new Font("Segoe UI", 7, FontStyle.Bold), Text = "Klik Detail ▲" };
            
            string status = item["status"]?.ToString() ?? "Pending";
            var btnStatus = new Guna2Button { 
                Size = new Size(115, 38), 
                Location = new Point(pU.Width - 145, 33), 
                BorderRadius = 14, 
                FillColor = status == "finish" ? Color.FromArgb(220, 252, 231) : (status == "cancel" ? Color.FromArgb(254, 242, 242) : Color.FromArgb(254, 249, 195)), 
                ForeColor = status == "finish" ? Color.LimeGreen : (status == "cancel" ? Color.Red : Color.FromArgb(161, 98, 7)), 
                Font = new Font("Segoe UI", 10, FontStyle.Bold), 
                Text = (status == "finish" ? "Berhasil" : (status == "cancel" ? "Dibatalkan" : "Pending")) 
            };

            pU.Controls.Add(btnNo); pU.Controls.Add(lblNama); pU.Controls.Add(lblTgl); pU.Controls.Add(btnSurveiTag); pU.Controls.Add(btnKlik); pU.Controls.Add(btnStatus);

            // DETAIL LEBIH SEDERHANA MANG (Tinggi dikurangi)
            var pD = new Guna2Panel { Name = "DynDetail", Size = new Size(cardW - 50, 260), Location = new Point(25, 90), BorderRadius = 14, BorderThickness = 1, BorderColor = SystemColors.ControlDark, FillColor = Color.FromArgb(248, 250, 252), Visible = false };
            BuildDetailSurvei(pD, item);

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

            container.Controls.Add(pU); container.Controls.Add(pD);
            pU.BringToFront(); pD.SendToBack();
            return container;
        }

        private void BuildDetailSurvei(Guna2Panel pD, JToken item)
        {
            var d = item["detail"];
            string idSurvei = item["id"]?.ToString()?.Replace("SRV-", "");

            // Layout 1 Kolom biar simpel mang!
            pD.Controls.Add(new Label { Text = "RENCANA WAKTU SURVEI", Location = new Point(35, 30), Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = SystemColors.ControlDarkDark, AutoSize = true });
            pD.Controls.Add(new Label { Text = d["tgl_survei"]?.ToString() ?? "-", Location = new Point(33, 55), Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(26, 18, 101), AutoSize = true });
            
            pD.Controls.Add(new Label { Text = "CATATAN DARI CALON PENYEWA", Location = new Point(35, 105), Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = SystemColors.ControlDarkDark, AutoSize = true });
            pD.Controls.Add(new Label { Text = d["catatan"]?.ToString() ?? "-", Location = new Point(33, 130), Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(26, 18, 101), AutoSize = true });

            if (item["status"]?.ToString() == "Pending")
            {
                var btnAccept = new Guna2Button { Text = "Terima Survei", Size = new Size(160, 40), Location = new Point(35, 190), BorderRadius = 12, FillColor = Color.FromArgb(220, 252, 231), ForeColor = Color.LimeGreen, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
                var btnReject = new Guna2Button { Text = "Tolak Survei", Size = new Size(160, 40), Location = new Point(210, 190), BorderRadius = 12, FillColor = Color.FromArgb(254, 242, 242), ForeColor = Color.Red, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };

                btnAccept.Click += async (s, e) => {
                    if (MessageBox.Show("Terima jadwal survei ini?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        await ActionSurvei(AcceptUrl + idSurvei);
                };
                btnReject.Click += async (s, e) => {
                    if (MessageBox.Show("Tolak jadwal survei ini?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        await ActionSurvei(RejectUrl + idSurvei);
                };

                pD.Controls.Add(btnAccept); pD.Controls.Add(btnReject);
            }
            else
            {
                pD.Controls.Add(new Label { Text = "Aksi sudah diproses.", Location = new Point(35, 190), Font = new Font("Segoe UI", 9, FontStyle.Bold | FontStyle.Italic), ForeColor = SystemColors.ControlDark, AutoSize = true });
            }
        }

        private async Task ActionSurvei(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);
                    var resp = await client.PutAsync(url, null);
                    if (resp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Berhasil memperbarui status survei");
                        await LoadRiwayatSurveiAsync();
                    }
                    else MessageBox.Show("Gagal: " + await resp.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
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
            foreach (Control c in entries)
            {
                c.Location = new Point(cX, curY);
                curY += c.Height + 15;
            }
            this.AutoScrollPosition = new Point(Math.Abs(scrollPos.X), Math.Abs(scrollPos.Y));
            this.ResumeLayout(false);
        }
    }
}

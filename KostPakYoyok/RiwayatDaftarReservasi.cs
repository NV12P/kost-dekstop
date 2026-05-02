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
    public partial class RiwayatDaftarReservasi : UserControl
    {
        private const string ApiUrl = "https://kost.arcv.web.id/api/riwayat";
        private const string RejectUrl = "https://kost.arcv.web.id/api/penyewa/reject/";
        private static System.Globalization.CultureInfo cultureIndo = new System.Globalization.CultureInfo("id-ID");

        private Panel activeEntry = null;

        public RiwayatDaftarReservasi()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.AutoScroll = true;
            this.BackColor = SystemColors.ControlLight;

            this.Load += async (s, e) => {
                await LoadRiwayatReservasiAsync();
                ReLayoutAll();
            };
            this.SizeChanged += (s, e) => ReLayoutAll();
        }

        private async Task LoadRiwayatReservasiAsync()
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

                    var reservasiData = arr.Where(x => x["kategori"]?.ToString() == "booking").ToList();

                    foreach (var item in reservasiData)
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
            var detail = item["detail"];
            DateTime tgl; DateTime.TryParse(item["tanggal"]?.ToString(), out tgl);

            int cardW = 1400;
            var container = new Panel { Name = "Entry_" + index, Size = new Size(cardW, 115), BackColor = Color.Transparent };

            var pU = new Guna2Panel { Name = "DynHeader", Size = new Size(cardW - 20, 105), Location = new Point(10, 0), BorderRadius = 14, FillColor = Color.White, BorderThickness = 1, BorderColor = Color.FromArgb(226, 232, 240), Cursor = Cursors.Hand };

            var btnNo = new Guna2Button { Size = new Size(65, 65), Location = new Point(25, 20), BorderRadius = 14, FillColor = SystemColors.Control, ForeColor = Color.FromArgb(26, 18, 101), Font = new Font("Segoe UI", 18, FontStyle.Bold), Text = index.ToString() };
            var lblNama = new Label { Text = (item["penyewa"]?.ToString() ?? "User").ToLower(), Location = new Point(105, 48), Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(26, 18, 101), AutoSize = true };
            var lblTgl = new Label { Text = tgl.ToString("dddd, dd/MM/yyyy", cultureIndo), Location = new Point(195, 23), Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold), ForeColor = SystemColors.ControlDarkDark, AutoSize = true };
            var btnBooking = new Guna2Button { Size = new Size(80, 22), Location = new Point(105, 21), BorderRadius = 8, FillColor = Color.FromArgb(224, 231, 255), ForeColor = Color.FromArgb(67, 56, 202), Font = new Font("Segoe UI", 8, FontStyle.Bold), Text = "BOOKING" };
            var btnKlik = new Guna2Button { Size = new Size(110, 22), Location = new Point(335, 21), BorderRadius = 8, FillColor = SystemColors.Control, ForeColor = Color.DarkGray, Font = new Font("Segoe UI", 8, FontStyle.Bold), Text = "Klik Detail ▲" };
            
            string status = item["status"]?.ToString() ?? "Booking";
            var btnStatus = new Guna2Button { 
                Size = new Size(115, 38), 
                Location = new Point(pU.Width - 145, 33), 
                BorderRadius = 14, 
                FillColor = status == "Booking" ? Color.FromArgb(254, 249, 195) : Color.FromArgb(254, 242, 242), 
                ForeColor = status == "Booking" ? Color.FromArgb(161, 98, 7) : Color.Red, 
                Font = new Font("Segoe UI", 10, FontStyle.Bold), 
                Text = status.ToUpper() 
            };

            pU.Controls.Add(btnNo); pU.Controls.Add(lblNama); pU.Controls.Add(lblTgl); pU.Controls.Add(btnBooking); pU.Controls.Add(btnKlik); pU.Controls.Add(btnStatus);

            var pD = new Guna2Panel { Name = "DynDetail", Size = new Size(cardW - 50, 320), Location = new Point(25, 90), BorderRadius = 14, BorderThickness = 1, BorderColor = SystemColors.ControlDark, FillColor = Color.FromArgb(248, 250, 252), Visible = false };
            BuildDetailBooking(pD, item, container);

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

        private void BuildDetailBooking(Guna2Panel pD, JToken item, Panel container)
        {
            var d = item["detail"];
            string idDetail = item["id"]?.ToString()?.Replace("BKG-", "").Replace("CAN-", "");

            pD.Controls.Add(new Label { Text = "KAMAR", Location = new Point(35, 30), Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = SystemColors.ControlDarkDark, AutoSize = true });
            pD.Controls.Add(new Label { Text = item["kamar"]?.ToString() ?? "-", Location = new Point(33, 60), Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(26, 18, 101), AutoSize = true });
            
            pD.Controls.Add(new Label { Text = "DURASI & METODE", Location = new Point(35, 120), Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = SystemColors.ControlDarkDark, AutoSize = true });
            pD.Controls.Add(new Label { Text = (d["durasi"]?.ToString() ?? "-") + " (" + (d["metode"]?.ToString() ?? "-") + ")", Location = new Point(33, 150), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(26, 18, 101), AutoSize = true });

            pD.Controls.Add(new Label { Text = "CATATAN", Location = new Point(35, 200), Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = SystemColors.ControlDarkDark, AutoSize = true });
            pD.Controls.Add(new Label { Text = d["catatan"]?.ToString() ?? "-", Location = new Point(33, 230), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(26, 18, 101), AutoSize = true });

            if (item["status"]?.ToString() == "Booking")
            {
                var btnReject = new Guna2Button { Text = "Tolak Reservasi", Size = new Size(180, 45), Location = new Point(pD.Width - 215, 240), BorderRadius = 14, FillColor = Color.FromArgb(254, 242, 242), ForeColor = Color.Red, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
                btnReject.Click += async (s, e) => {
                    if (MessageBox.Show("Tolak reservasi ini?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        await RejectBooking(RejectUrl + idDetail);
                    }
                };
                pD.Controls.Add(btnReject);
            }
        }

        private async Task RejectBooking(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);
                    var resp = await client.PutAsync(url, null);
                    if (resp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Reservasi berhasil ditolak");
                        await LoadRiwayatReservasiAsync();
                    }
                    else
                    {
                        MessageBox.Show("Gagal: " + await resp.Content.ReadAsStringAsync());
                    }
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

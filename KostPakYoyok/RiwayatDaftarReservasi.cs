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
            DateTime tgl; DateTime.TryParse(item["tanggal"]?.ToString(), out tgl);
            string namaUser = item["penyewa"]?.ToString() ?? "User";
            string status = item["status"]?.ToString() ?? "Booking";

            int cardW = 1400;
            var container = new Panel { Name = "Entry_" + index, Size = new Size(cardW, 115), BackColor = Color.Transparent };

            // HEADER
            var pU = new Guna2Panel { Name = "DynHeader", Size = new Size(cardW - 20, 105), Location = new Point(10, 0), BorderRadius = 14, FillColor = Color.White, BorderThickness = 1, BorderColor = Color.FromArgb(226, 232, 240), Cursor = Cursors.Hand };

            var btnIcon = new Guna2Button { Size = new Size(65, 65), Location = new Point(25, 20), BorderRadius = 14, FillColor = Color.FromArgb(26, 18, 101), ForeColor = Color.White, Font = new Font("Segoe UI", 18, FontStyle.Bold), Text = "R" };
            var lblNama = new Label { Text = namaUser.ToLower(), Location = new Point(105, 48), Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(26, 18, 101), AutoSize = true };
            var lblTgl = new Label { Text = tgl.ToString("dddd, dd/MM/yyyy", cultureIndo), Location = new Point(245, 24), Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold), ForeColor = SystemColors.ControlDarkDark, AutoSize = true };
            var btnBookingTag = new Guna2Button { Size = new Size(110, 22), Location = new Point(105, 22), BorderRadius = 8, FillColor = SystemColors.Control, ForeColor = Color.FromArgb(26, 18, 101), Font = new Font("Segoe UI", 8, FontStyle.Bold), Text = "BOOKING" };
            var btnKlik = new Guna2Button { Size = new Size(110, 22), Location = new Point(410, 22), BorderRadius = 8, FillColor = SystemColors.Control, ForeColor = Color.DarkGray, Font = new Font("Segoe UI", 8, FontStyle.Bold), Text = "Klik Detail ▲" };
            
            var btnStatus = new Guna2Button { Size = new Size(105, 38), Location = new Point(pU.Width - 130, 33), BorderRadius = 14, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            if (status.ToLower() == "booking") {
                btnStatus.FillColor = Color.FromArgb(219, 234, 254); btnStatus.ForeColor = Color.FromArgb(37, 99, 235); btnStatus.Text = "Booking";
            } else {
                btnStatus.FillColor = Color.FromArgb(254, 242, 242); btnStatus.ForeColor = Color.Red; btnStatus.Text = "Cancel";
            }

            pU.Controls.Add(btnIcon); pU.Controls.Add(lblNama); pU.Controls.Add(lblTgl); pU.Controls.Add(btnBookingTag); pU.Controls.Add(btnKlik); pU.Controls.Add(btnStatus);

            // DETAIL
            var pD = new Guna2Panel { Name = "DynDetail", Size = new Size(cardW - 50, 280), Location = new Point(25, 95), BorderRadius = 14, BorderThickness = 1, BorderColor = SystemColors.ControlDark, FillColor = Color.FromArgb(248, 250, 252), Visible = false };
            BuildDetailBooking(pD, item);

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

        private void BuildDetailBooking(Guna2Panel pD, JToken item)
        {
            var d = item["detail"];
            string idDetail = item["id"]?.ToString()?.Replace("BKG-", "").Replace("CAN-", "");
            string durasi = d["durasi"]?.ToString() ?? "1 Bulan";
            string metode = d["metode"]?.ToString() ?? "TUNAI";
            string catatan = d["catatan"]?.ToString() ?? "-";
            string status = item["status"]?.ToString()?.ToLower() ?? "booking";

            // Box DURASI
            var sp1 = new Guna2Panel { Size = new Size(300, 95), Location = new Point(35, 35), BorderRadius = 10, FillColor = Color.White, BorderColor = Color.FromArgb(226, 232, 240), BorderThickness = 1 };
            sp1.Controls.Add(new Label { Text = "DURASI SEWA", Location = new Point(20, 15), Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = SystemColors.ControlDarkDark, AutoSize = true });
            sp1.Controls.Add(new Label { Text = durasi, Location = new Point(18, 42), Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(26, 18, 101), AutoSize = true });
            pD.Controls.Add(sp1);

            // Box METODE
            var sp2 = new Guna2Panel { Size = new Size(300, 95), Location = new Point(350, 35), BorderRadius = 10, FillColor = Color.White, BorderColor = Color.FromArgb(226, 232, 240), BorderThickness = 1 };
            sp2.Controls.Add(new Label { Text = "METODE BAYAR", Location = new Point(20, 15), Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = SystemColors.ControlDarkDark, AutoSize = true });
            sp2.Controls.Add(new Label { Text = metode, Location = new Point(18, 42), Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(26, 18, 101), AutoSize = true });
            pD.Controls.Add(sp2);

            // Box CATATAN
            var sp3 = new Guna2Panel { Size = new Size(615, 95), Location = new Point(35, 145), BorderRadius = 10, FillColor = Color.White, BorderColor = Color.FromArgb(226, 232, 240), BorderThickness = 1 };
            sp3.Controls.Add(new Label { Text = "CATATAN", Location = new Point(20, 15), Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = SystemColors.ControlDarkDark, AutoSize = true });
            sp3.Controls.Add(new Label { Text = "\"" + catatan + "\"", Location = new Point(18, 42), Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold | FontStyle.Italic), ForeColor = Color.FromArgb(26, 18, 101), AutoSize = true });
            pD.Controls.Add(sp3);

            // Box AKSI
            var sp4 = new Guna2Panel { Size = new Size(615, 205), Location = new Point(685, 35), BorderRadius = 14, FillColor = Color.FromArgb(243, 245, 251), BorderColor = Color.FromArgb(226, 232, 240), BorderThickness = 1 };
            
            if (status == "booking")
            {
                var btnAccept = new Guna2Button { Text = "ACCEPT", Size = new Size(550, 60), Location = new Point(32, 35), BorderRadius = 14, FillColor = Color.FromArgb(26, 18, 101), ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
                var btnReject = new Guna2Button { Text = "CANCEL", Size = new Size(550, 60), Location = new Point(32, 110), BorderRadius = 14, FillColor = Color.FromArgb(254, 242, 242), BorderColor = Color.MistyRose, BorderThickness = 1, ForeColor = Color.Red, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
                btnReject.HoverState.FillColor = Color.Red; btnReject.HoverState.ForeColor = Color.FromArgb(254, 242, 242);

                btnAccept.Click += (s, e) => { MessageBox.Show("Silakan daftarkan penyewa secara manual melalui halaman 'Penyewa' dengan data booking ini.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); };
                btnReject.Click += async (s, e) => { if (MessageBox.Show("Tolak reservasi ini?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes) await ActionReject(RejectUrl + idDetail); };
                sp4.Controls.Add(btnAccept); sp4.Controls.Add(btnReject);
            }
            else
            {
                var lblMsg = new Label { Text = "NO ACTION AVAILABLE", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = SystemColors.ControlDark, AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
                sp4.Controls.Add(lblMsg);
            }
            pD.Controls.Add(sp4);
        }

        private async Task ActionReject(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);
                    var resp = await client.PutAsync(url, null);
                    if (resp.IsSuccessStatusCode) { MessageBox.Show("Reservasi berhasil ditolak"); await LoadRiwayatReservasiAsync(); }
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
            foreach (Control c in entries) { c.Location = new Point(cX, curY); curY += c.Height + 15; }
            this.AutoScrollPosition = new Point(Math.Abs(scrollPos.X), Math.Abs(scrollPos.Y));
            this.ResumeLayout(false);
        }
    }
}

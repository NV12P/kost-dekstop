using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace KostPakYoyok
{
    public partial class RiwayatPenghuniAktif : UserControl
    {
        private const string ApiUrl = "https://kost.arcv.web.id/api/penyewa";
        private static System.Globalization.CultureInfo cultureIndo = new System.Globalization.CultureInfo("id-ID");
        
        private Guna2ShadowPanel templateUtama;
        private Guna2Panel templateDetail;

        private Guna2Panel activeRoomDetail = null;
        private Guna2ShadowPanel activeSubDetail = null;
        private Label activeSubArrow = null;

        public RiwayatPenghuniAktif()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            
            templateUtama = panelUtama;
            templateDetail = panelDetailUtama;

            templateUtama.Visible = false;
            templateDetail.Visible = false;

            this.Load += async (s, e) => await LoadRiwayatAktifAsync();
        }

        private async Task LoadRiwayatAktifAsync()
        {
            try
            {
                var controlsToRemove = this.Controls.Cast<Control>()
                    .Where(c => c != templateUtama && c != templateDetail && c.Name != "panel1")
                    .ToList();
                
                foreach (var c in controlsToRemove) this.Controls.Remove(c);

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);

                    var resp = await client.GetAsync(ApiUrl);
                    if (!resp.IsSuccessStatusCode) return;

                    var json = await resp.Content.ReadAsStringAsync();
                    var arr = JArray.Parse(json);

                    int index = 1;
                    foreach (var item in arr)
                    {
                        if (item["status"]?.ToString().ToLower() == "disewa" && item["penyewa"] != null)
                        {
                            var row = CreateRiwayatRow(item, index++);
                            this.Controls.Add(row.Item1); 
                            this.Controls.Add(row.Item2); 
                        }
                    }
                    RefreshLayout();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load riwayat: " + ex.Message);
            }
        }

        private Tuple<Guna2ShadowPanel, Guna2Panel> CreateRiwayatRow(JToken item, int index)
        {
            var penyewa = item["penyewa"];
            string rawTgl = penyewa["tgl_mulai"]?.ToString() ?? penyewa["tglsewa_sewa"]?.ToString() ?? DateTime.Now.ToString("yyyy-MM-dd");
            DateTime tglMulai;
            DateTime.TryParse(rawTgl, out tglMulai);

            var pUtama = new Guna2ShadowPanel
            {
                Size = templateUtama.Size,
                Radius = templateUtama.Radius,
                FillColor = templateUtama.FillColor,
                BackColor = Color.Transparent,
                ShadowDepth = 0,
                Name = "pUtama_" + index,
                Cursor = Cursors.Hand
            };

            Label lblPenyewaRef = null;
            Label lblNIKRef = null;

            foreach (Control c in templateUtama.Controls)
            {
                Control clone = CloneControl(c);
                if (clone != null)
                {
                    if (clone.Name == "nomorData") clone.Text = index.ToString();
                    if (clone.Name == "labelPenyewa") { clone.Text = penyewa["nama"]?.ToString() ?? "N/A"; lblPenyewaRef = clone as Label; }
                    if (clone.Name == "labelNIK") { string nikVal = penyewa["nik"]?.ToString() ?? penyewa["nik_penyewa"]?.ToString() ?? "00000000"; clone.Text = "(" + nikVal + ")"; lblNIKRef = clone as Label; }
                    if (clone.Name == "labelTanggal") clone.Text = tglMulai.ToString("dddd, dd/MM/yyyy", cultureIndo);
                    
                    if (clone.Name == "guna2Button9") {
                        clone.Text = "Klik Detail ▲";
                        ((Guna2Button)clone).Click += (s, e) => ToggleRoomDetail(pUtama);
                    } else {
                        clone.Click += (s, e) => ToggleRoomDetail(pUtama);
                    }
                    pUtama.Controls.Add(clone);
                }
            }

            if (lblPenyewaRef != null && lblNIKRef != null) {
                lblNIKRef.Location = new Point(lblPenyewaRef.Right + 5, lblPenyewaRef.Location.Y + 5);
            }

            pUtama.Click += (s, e) => ToggleRoomDetail(pUtama);

            var pDetail = new Guna2Panel
            {
                Size = templateDetail.Size,
                BorderColor = templateDetail.BorderColor,
                BorderRadius = templateDetail.BorderRadius,
                BorderThickness = templateDetail.BorderThickness,
                FillColor = templateDetail.FillColor,
                Visible = false,
                Name = "pDetail_" + index
            };

            FillDetailPanel(pDetail, item, tglMulai);
            pUtama.Tag = pDetail;

            return new Tuple<Guna2ShadowPanel, Guna2Panel>(pUtama, pDetail);
        }

        private void FillDetailPanel(Guna2Panel pDetail, JToken item, DateTime tglMulai)
        {
            var penyewa = item["penyewa"];
            long hargaKamar = item["harga"]?.ToObject<long>() ?? 0;
            int totalBulanSewa = penyewa["sewabrpbulan"]?.ToObject<int>() ?? 1;
            long totalTagihan = hargaKamar * totalBulanSewa;
            long totalSudahBayar = penyewa["cicilan"]?.Sum(c => c["nominal"]?.ToObject<long>() ?? 0) ?? 0;
            long sisaTotal = totalTagihan - totalSudahBayar;

            foreach (Control c in templateDetail.Controls)
            {
                Control cloned = CloneControl(c);
                if (cloned != null)
                {
                    pDetail.Controls.Add(cloned);
                    UpdateDataRecursive(cloned, item, totalTagihan, sisaTotal, totalSudahBayar);
                }
            }

            var pRincianHeader = pDetail.Controls.Find("panelRincianBulanSewa", true).FirstOrDefault() as Guna2ShadowPanel;
            var pRincianContent = pDetail.Controls.Find("panelDetailRincianBulanSewa", true).FirstOrDefault() as Guna2ShadowPanel;
            var lblArrow = pRincianHeader?.Controls.Find("label14", true).FirstOrDefault() as Label;

            if (pRincianHeader != null && pRincianContent != null)
            {
                pRincianContent.Visible = false;
                pRincianContent.AutoScroll = true;
                if (lblArrow != null) lblArrow.Text = "▲";
                pRincianHeader.Cursor = Cursors.Hand;

                var lblBulanTpl = pRincianContent.Controls.Find("labelBulan", true).FirstOrDefault() as Label;
                pRincianContent.Controls.Clear();

                int rowY = 25; 
                long bayarTersisa = totalSudahBayar;

                for (int i = 0; i < totalBulanSewa; i++)
                {
                    DateTime bulanSkrg = tglMulai.AddMonths(i);
                    bool isLunasBulanIni = bayarTersisa >= hargaKamar;
                    long sisaBulanIni = isLunasBulanIni ? 0 : (hargaKamar - bayarTersisa);
                    if (isLunasBulanIni) bayarTersisa -= hargaKamar; else bayarTersisa = 0;

                    pRincianContent.Controls.Add(new Label {
                        Text = bulanSkrg.ToString("MMMM yyyy", cultureIndo).ToUpper(),
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = Color.DimGray, AutoSize = true, Location = new Point(22, rowY)
                    });

                    pRincianContent.Controls.Add(new Guna2Button {
                        Text = isLunasBulanIni ? "✔️ Lunas" : "❌ Belum Lunas",
                        FillColor = isLunasBulanIni ? Color.FromArgb(220, 252, 231) : Color.FromArgb(254, 242, 242),
                        ForeColor = isLunasBulanIni ? Color.LimeGreen : Color.Red,
                        BorderRadius = 10, Size = new Size(110, 27),
                        Location = new Point(pRincianContent.Width - 135, rowY - 2),
                        Font = new Font("Segoe UI", 7, FontStyle.Bold)
                    });

                    rowY += 35;
                    if (!isLunasBulanIni) {
                        pRincianContent.Controls.Add(new Label {
                            Text = "* Masih kurang Rp. " + sisaBulanIni.ToString("N0", cultureIndo) + " untuk bulan ini",
                            Font = new Font("Segoe UI", 9, FontStyle.Bold | FontStyle.Italic),
                            ForeColor = Color.Red, AutoSize = true, Location = new Point(22, rowY)
                        });
                        rowY += 30;
                    }
                    rowY += 10; 
                }

                pRincianContent.Height = Math.Min(rowY + 20, 400); 

                Action toggleSub = () => {
                    if (!pRincianContent.Visible) {
                        if (activeSubDetail != null && activeSubDetail != pRincianContent) {
                            activeSubDetail.Visible = false;
                            if (activeSubArrow != null) activeSubArrow.Text = "▲";
                        }
                        activeSubDetail = pRincianContent; activeSubArrow = lblArrow;
                    } else if (activeSubDetail == pRincianContent) { activeSubDetail = null; activeSubArrow = null; }

                    pRincianContent.Visible = !pRincianContent.Visible;
                    if (lblArrow != null) lblArrow.Text = pRincianContent.Visible ? "▼" : "▲";
                    RefreshLayout();
                };

                pRincianHeader.Click += (s, e) => toggleSub();
                foreach (Control child in pRincianHeader.Controls) child.Click += (s, e) => toggleSub();
            }
        }

        private void ToggleRoomDetail(Guna2ShadowPanel pUtama)
        {
            var pDetail = pUtama.Tag as Guna2Panel;
            if (pDetail == null) return;

            if (!pDetail.Visible) {
                if (activeRoomDetail != null && activeRoomDetail != pDetail) {
                    activeRoomDetail.Visible = false;
                    var otherUtama = this.Controls.Cast<Control>().FirstOrDefault(c => c is Guna2ShadowPanel && c.Tag == activeRoomDetail);
                    var otherBtn = otherUtama?.Controls.Find("guna2Button9", true).FirstOrDefault() as Guna2Button;
                    if (otherBtn != null) otherBtn.Text = "Klik Detail ▲";
                }
                activeRoomDetail = pDetail;
            } else if (activeRoomDetail == pDetail) { activeRoomDetail = null; }

            pDetail.Visible = !pDetail.Visible;
            var btn = pUtama.Controls.Find("guna2Button9", true).FirstOrDefault() as Guna2Button;
            if (btn != null) btn.Text = pDetail.Visible ? "Klik Detail ▼" : "Klik Detail ▲";
            RefreshLayout();
        }

        private Control CloneControl(Control c)
        {
            Control clone = null;
            if (c is Guna2ShadowPanel sp) { clone = new Guna2ShadowPanel { Size = sp.Size, Location = sp.Location, Radius = sp.Radius, FillColor = sp.FillColor, ShadowDepth = sp.ShadowDepth, BackColor = sp.BackColor, Name = sp.Name }; }
            else if (c is Guna2PictureBox pb) { clone = new Guna2PictureBox { Size = pb.Size, Location = pb.Location, BorderRadius = pb.BorderRadius, Image = pb.Image, SizeMode = pb.SizeMode, Name = pb.Name, Visible = pb.Visible }; }
            else if (c is Guna2Button btn) { clone = new Guna2Button { Size = btn.Size, Location = btn.Location, BorderRadius = btn.BorderRadius, FillColor = btn.FillColor, Font = btn.Font, ForeColor = btn.ForeColor, Text = btn.Text, Name = btn.Name, Image = btn.Image, ImageSize = btn.ImageSize }; }
            else if (c is Guna2TextBox tb) { clone = new Guna2TextBox { Size = tb.Size, Location = tb.Location, BorderRadius = tb.BorderRadius, FillColor = tb.FillColor, Font = tb.Font, ForeColor = tb.ForeColor, Text = tb.Text, Name = tb.Name, PlaceholderText = tb.PlaceholderText, BorderColor = tb.BorderColor, BorderStyle = tb.BorderStyle }; }
            else if (c is Label lbl) { clone = new Label { Size = lbl.Size, Location = lbl.Location, Font = lbl.Font, ForeColor = lbl.ForeColor, AutoSize = lbl.AutoSize, Text = lbl.Text, Name = lbl.Name }; }
            else if (c is Guna2Panel gp) { clone = new Guna2Panel { Size = gp.Size, Location = gp.Location, BorderRadius = gp.BorderRadius, FillColor = gp.FillColor, BorderColor = gp.BorderColor, BorderThickness = gp.BorderThickness, Name = gp.Name }; }

            if (clone != null && c.Controls.Count > 0) {
                foreach (Control child in c.Controls) {
                    var childClone = CloneControl(child);
                    if (childClone != null) clone.Controls.Add(childClone);
                }
            }
            return clone;
        }

        private void UpdateDataRecursive(Control parent, JToken item, long total, long sisa, long totalBayar)
        {
            var penyewa = item["penyewa"];
            if (parent is Label lbl) {
                if (lbl.Name == "labelLokasiKamar") lbl.Text = item["nama"]?.ToString() ?? "N/A";
                if (lbl.Name == "labelStatusTagihan") { lbl.Text = sisa <= 0 ? "✔️ LUNAS" : "❌ BELUM LUNAS"; lbl.ForeColor = sisa <= 0 ? Color.LimeGreen : Color.Red; }
                if (lbl.Name == "labelTotalTagihan") lbl.Text = "TOTAL BIAYA Rp. " + total.ToString("N0", cultureIndo);
                if (lbl.Name == "labelDurasiSewa") lbl.Text = (penyewa["sewabrpbulan"]?.ToString() ?? "1") + " BULAN";
                if (lbl.Name == "labelMetodeBayar") lbl.Text = (penyewa["metodepembayaran"]?.ToString() ?? "tunai").ToUpper();
            }
            if (parent.Controls.Count > 0) {
                foreach (Control child in parent.Controls) UpdateDataRecursive(child, item, total, sisa, totalBayar);
            }
        }

        private void RefreshLayout()
        {
            int currentY = 28;
            // Hitung posisi tengah mang!
            int centerX = (this.Width - templateUtama.Width) / 2;
            int detailX = (this.Width - templateDetail.Width) / 2;

            var items = this.Controls.Cast<Control>()
                .Where(c => c is Guna2ShadowPanel && c.Name.StartsWith("pUtama_"))
                .OrderBy(c => int.Parse(c.Name.Split('_')[1]))
                .ToList();

            foreach (var pUtama in items) {
                pUtama.Location = new Point(centerX, currentY);
                var pDetail = pUtama.Tag as Guna2Panel;
                
                currentY += pUtama.Height + 15;

                if (pDetail != null && pDetail.Visible) {
                    pDetail.Location = new Point(detailX, currentY - 10);
                    
                    var rincianContent = pDetail.Controls.Find("panelDetailRincianBulanSewa", true).FirstOrDefault();
                    if (rincianContent != null && rincianContent.Visible) 
                        pDetail.Height = templateDetail.Height + rincianContent.Height - 120;
                    else 
                        pDetail.Height = templateDetail.Height;

                    currentY += pDetail.Height + 15;
                } else if (pDetail != null) {
                    pDetail.Visible = false;
                }
            }
        }
    }
}
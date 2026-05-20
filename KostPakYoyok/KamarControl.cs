using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using System.Net;

namespace KostPakYoyok
{
    public partial class KamarControl : UserControl
    {
        private const string KamarApiUrl = "https://kost.arcv.web.id/api/kamar";

        // =====================================================
        // CONSTRUCTOR
        // =====================================================
        public KamarControl()
        {
            InitializeComponent();
            guna2ShadowPanel2.Visible = false;
            this.Load += async (s, e) => await LoadKamarAsync();
        }

        // =====================================================
        // UI LOGIC
        // =====================================================
        private async Task OnTambahClickedAsync()
        {
            var parentForm = this.FindForm() as FormUtama;
            if (parentForm == null) return;
            Form bg = new Form { StartPosition = FormStartPosition.Manual, FormBorderStyle = FormBorderStyle.None, Opacity = 0.30d, BackColor = Color.Black, Size = parentForm.Size, Location = parentForm.Location };
            bg.Show();
            using (FormKamar form = new FormKamar())
            {
                form.StartPosition = FormStartPosition.Manual;
                form.Owner = bg;
                form.Location = new Point(parentForm.Location.X + (parentForm.Width - form.Width) / 2, parentForm.Location.Y + (parentForm.Height - form.Height) / 2);
                if (form.ShowDialog() == DialogResult.OK) await LoadKamarAsync();
            }
            bg.Dispose();
        }

        // =====================================================
        // DATA LOADING
        // =====================================================
        private async Task LoadKamarAsync()
        {
            try
            {
                var dynamicPanels = this.Controls.OfType<Guna2ShadowPanel>().Where(p => p.Name != null && p.Name.StartsWith("dynamicKamarPanel_")).ToList();
                foreach (var p in dynamicPanels) this.Controls.Remove(p);

                if (string.IsNullOrWhiteSpace(Session.Token)) return;

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);
                    var response = await client.GetAsync(KamarApiUrl);
                    if (!response.IsSuccessStatusCode) return;

                    var json = await response.Content.ReadAsStringAsync();
                    var token = JToken.Parse(json);
                    
                    JArray arr = (token["data"] != null) ? (JArray)token["data"] : (token.Type == JTokenType.Array ? (JArray)token : new JArray());

                    int columns = 5, gapX = 15, gapY = 30, marginLeft = 40, marginTop = 234;
                    int availableWidth = Math.Max(1200, this.ClientSize.Width - marginLeft * 2);
                    int cardWidth = Math.Max(220, (availableWidth - gapX * (columns - 1)) / columns);
                    int cardHeight = 580;

                    for (int i = 0; i < arr.Count; i++)
                    {
                        var panel = CreateKamarPanel(arr[i], i, cardWidth, cardHeight);
                        int col = i % columns, row = i / columns;
                        panel.Location = new Point(marginLeft + col * (cardWidth + gapX), marginTop + row * (cardHeight + gapY));
                        panel.Name = "dynamicKamarPanel_" + i;
                        this.Controls.Add(panel);
                        panel.BringToFront();
                    }
                }
            }
            catch { }
        }

        // =====================================================
        // UI HELPERS
        // =====================================================
        private Guna2ShadowPanel CreateKamarPanel(JToken item, int index, int cardWidth, int cardHeight)
        {
            int padding = 18;
            var panel = new Guna2ShadowPanel { Size = new Size(cardWidth, 580), Radius = 12, FillColor = Color.White, ShadowColor = Color.LightGray, ShadowDepth = 10 };

            var picture = new Guna2PictureBox { Size = new Size(cardWidth - padding * 2, 140), Location = new Point(padding, padding), SizeMode = PictureBoxSizeMode.StretchImage, BorderRadius = 10 };
            var fotoRaw = item["foto_kamar"]?.ToString();
            var fotoUrl = ResolveImageUrl(fotoRaw);

            if (!string.IsNullOrWhiteSpace(fotoUrl))
            {
                Task.Run(async () => {
                    try {
                        using (var wc = new WebClient()) {
                            wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                            byte[] data = await wc.DownloadDataTaskAsync(new Uri(fotoUrl));
                            using (var ms = new System.IO.MemoryStream(data)) {
                                var img = Image.FromStream(ms);
                                this.BeginInvoke(new Action(() => picture.Image = img));
                            }
                        }
                    } catch { }
                });
            }
            panel.Controls.Add(picture);
            int y = picture.Bottom + 12;

            var lblNama = new Label { Text = (string)item["nomor_kamar"] ?? ("Kamar " + (index + 1)), Font = new Font("Segoe UI Semibold", 13, FontStyle.Bold), ForeColor = Color.FromArgb(26, 18, 101), Location = new Point(padding, y), AutoSize = true };
            panel.Controls.Add(lblNama);
            y = lblNama.Bottom + 5;

            var statusVal = item["status_kamar"]?.ToString().ToLower() ?? "tersedia";
            bool isDisewa = statusVal == "disewa" || statusVal == "terisi";
            var lblStatus = new Label { Location = new Point(padding, y), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = isDisewa ? Color.Red : Color.LimeGreen, Text = isDisewa ? "Status - Disewa" : "Status - Tersedia" };
            panel.Controls.Add(lblStatus);
            y = lblStatus.Bottom + 15;

            var fasilitasList = item["fasilitas"] as JArray;
            if (fasilitasList != null) {
                var kFas = fasilitasList.Where(f => (string)f["tipe"] == "kamar");
                if (kFas.Any()) {
                    var lblKamarTitle = new Label { Text = "Fasilitas Kamar :", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.DimGray, Location = new Point(padding, y), AutoSize = true };
                    panel.Controls.Add(lblKamarTitle);
                    y = lblKamarTitle.Bottom + 5;

                    foreach (var f in kFas) {
                        panel.Controls.Add(new Label { 
                            Text = "✔ " + f["nama_fasilitas"], 
                            Font = new Font("Segoe UI", 8F), 
                            ForeColor = Color.FromArgb(26, 18, 101), 
                            Location = new Point(padding + 5, y), 
                            AutoSize = true 
                        });
                        y += 18;
                    }
                    y += 8;
                }

                var bFas = fasilitasList.Where(f => (string)f["tipe"] == "bersama");
                if (bFas.Any()) {
                    var lblBersamaTitle = new Label { Text = "Fasilitas Bersama :", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.DimGray, Location = new Point(padding, y), AutoSize = true };
                    panel.Controls.Add(lblBersamaTitle);
                    y = lblBersamaTitle.Bottom + 5;

                    foreach (var f in bFas) {
                        panel.Controls.Add(new Label { 
                            Text = "✔ " + f["nama_fasilitas"], 
                            Font = new Font("Segoe UI", 8F), 
                            ForeColor = Color.FromArgb(26, 18, 101), 
                            Location = new Point(padding + 5, y), 
                            AutoSize = true 
                        });
                        y += 18;
                    }
                }
            }

            y = 480; 
            var lblHarga = new Label { Location = new Point(padding, y), AutoSize = true, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.Green, Text = "Rp. " + (long.TryParse(item["harga_kamar_perbulan"]?.ToString(), out long h) ? h.ToString("N0").Replace(",", ".") : "0") };
            panel.Controls.Add(lblHarga);

            var btnEdit = new Guna2Button { Size = new Size(cardWidth - padding * 2, 38), Location = new Point(padding, 525), Text = "Edit", BorderRadius = 12, FillColor = Color.FromArgb(26, 18, 101), ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnEdit.Click += async (s, e) => {
                var parentForm = this.FindForm() as FormUtama;
                if (parentForm == null) return;
                Form bg = new Form { StartPosition = FormStartPosition.Manual, FormBorderStyle = FormBorderStyle.None, Opacity = 0.30, BackColor = Color.Black, Size = parentForm.Size, Location = parentForm.Location };
                bg.Show();
                using (FormEditKamar form = new FormEditKamar(item)) {
                    form.StartPosition = FormStartPosition.CenterScreen;
                    if (form.ShowDialog() == DialogResult.OK) await LoadKamarAsync();
                }
                bg.Dispose();
            };
            panel.Controls.Add(btnEdit);
            return panel;
        }

        private string ResolveImageUrl(string path) {
            if (string.IsNullOrWhiteSpace(path)) return null;
            string p = path.Trim();
            
            if (p.StartsWith("http", StringComparison.OrdinalIgnoreCase)) {
                return p.Replace("http://", "https://");
            }
            
            string cleanPath = p.Replace("\\", "/").TrimStart('/');
            return "https://kost.arcv.web.id/" + cleanPath;
        }

        // =====================================================
        // EVENT HANDLERS
        // =====================================================
        private void btnTambah_Click(object sender, EventArgs e) => _ = OnTambahClickedAsync();
    }
}

using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace KostPakYoyok
{
    public partial class KamarControl : UserControl
    {
        private const string KamarApiUrl = "http://localhost:8000/api/kamar";
        private const string ApiBaseUrl = "http://localhost:8000/";

        public KamarControl()
        {
            InitializeComponent();

            // hide designer sample panel
            guna2ShadowPanel2.Visible = false;

            // load data
            this.Load += async (s, e) => await LoadKamarAsync();
        }

        private async Task OnTambahClickedAsync()
        {
            var parentForm = this.FindForm() as FormUtama;
            if (parentForm == null) return;

            Form formbackground = new Form();
            try
            {
                using (FormKamar formTambahKamar = new FormKamar())
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

                    formTambahKamar.StartPosition = FormStartPosition.Manual;
                    formTambahKamar.Owner = formbackground;
                    formTambahKamar.Location = new Point(
                        parentForm.Location.X + (parentForm.Width - formTambahKamar.Width) / 2,
                        parentForm.Location.Y + (parentForm.Height - formTambahKamar.Height) / 2
                    );

                    var dlgResult = formTambahKamar.ShowDialog();

                    formbackground.Dispose();

                    if (dlgResult == DialogResult.OK)
                        await LoadKamarAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan: " + ex.Message);
            }
            finally
            {
                if (formbackground != null && !formbackground.IsDisposed)
                    formbackground.Dispose();
            }
        }

        private async Task LoadKamarAsync()
        {
            try
            {
                // remove previously generated dynamic panels
                var dynamicPanels = this.Controls
                    .OfType<Guna2ShadowPanel>()
                    .Where(p => p.Name != null && p.Name.StartsWith("dynamicKamarPanel_"))
                    .ToList();

                foreach (var p in dynamicPanels)
                    this.Controls.Remove(p);

                if (string.IsNullOrWhiteSpace(Session.Token))
                {
                    MessageBox.Show("Token tidak tersedia. Silakan login.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);

                    HttpResponseMessage response;
                    try
                    {
                        response = await client.GetAsync(KamarApiUrl);
                    }
                    catch (HttpRequestException ex)
                    {
                        MessageBox.Show("Network error: " + ex.Message, "Network error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    catch (TaskCanceledException ex)
                    {
                        MessageBox.Show("Request timed out: " + ex.Message, "Timeout", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var json = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Server returned {(int)response.StatusCode} {response.ReasonPhrase}\nResponse: {json}", "Server error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    JArray arr;
                    try
                    {
                        var token = JToken.Parse(json);
                        if (token.Type == JTokenType.Array)
                            arr = (JArray)token;
                        else if (token["data"] is JArray dataArr)
                            arr = dataArr;
                        else
                        {
                            MessageBox.Show("Response /api/kamar tidak mengandung array data yang dikenali.", "Parse error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Invalid JSON from server:\n" + ex.Message, "Parse error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Force layout to 5 columns
                    int columns = 5;
                    int gapX = 20;
                    int gapY = 30;
                    int marginLeft = 24;
                    int marginTop = 65; // Mang Aceng naikin dari 94 biar gak turun banget mang

                    int availableWidth = Math.Max(600, this.ClientSize.Width - marginLeft * 2);
                    int cardWidth = Math.Max(180, (availableWidth - gapX * (columns - 1)) / columns);
                    int cardHeight = 550;

                    for (int i = 0; i < arr.Count; i++)
                    {
                        var item = arr[i];
                        var panel = CreateKamarPanel(item, i, cardWidth, cardHeight);

                        int col = i % columns;
                        int row = i / columns;
                        int x = marginLeft + col * (cardWidth + gapX);
                        int y = marginTop + row * (cardHeight + gapY);

                        panel.Location = new Point(x, y);
                        panel.Name = "dynamicKamarPanel_" + i;

                        this.Controls.Add(panel);
                        panel.BringToFront();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Load kamar gagal: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Guna2ShadowPanel CreateKamarPanel(JToken item, int index, int cardWidth, int cardHeight)
        {
            int padding = 18;
            int currentCardHeight = 580; // Mang Aceng tambahin tingginya biar lega

            var panel = new Guna2ShadowPanel
            {
                Size = new Size(cardWidth, currentCardHeight),
                Radius = 12,
                FillColor = Color.White,
                ShadowColor = Color.LightGray,
                ShadowDepth = 10,
                Margin = new Padding(10, 25, 10, 15)
            };

            // ================= FOTO (Paling Atas) =================
            // Mang Aceng set jadi 140 biar proporsional mang, gak terlalu tipis
            var picture = new Guna2PictureBox
            {
                Size = new Size(cardWidth - padding * 2, 140), 
                Location = new Point(padding, padding),
                SizeMode = PictureBoxSizeMode.StretchImage, 
                BorderRadius = 10
            };

            var fotoPath = (string)item["foto_kamar"];
            var fotoUrl = ResolveImageUrl(fotoPath);

            picture.LoadCompleted += (s, e) =>
            {
                if (e.Error != null || picture.Image == null)
                {
                    var img = TryGetImageFromResources(fotoPath);
                    if (img != null) picture.Image = img;
                }
            };

            try
            {
                if (!string.IsNullOrWhiteSpace(fotoUrl)) picture.LoadAsync(fotoUrl);
                else
                {
                    var img = TryGetImageFromResources(fotoPath);
                    if (img != null) picture.Image = img;
                }
            }
            catch { }

            panel.Controls.Add(picture);

            int y = picture.Bottom + 12;

            // ================= LABEL NAMA KAMAR (Di bawah foto) =================
            var namaKamar = (string)item["nama_kamar"] ?? ("Kamar " + (index + 1));
            var lblNamaKamar = new Label
            {
                Text = namaKamar,
                Font = new Font("Segoe UI Semibold", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 18, 101),
                Location = new Point(padding, y),
                AutoSize = true,
                BackColor = Color.White
            };

            panel.Controls.Add(lblNamaKamar);
            y = lblNamaKamar.Bottom + 10;

            // ================= STATUS =================
            var statusVal = (string)item["status_kamar"];

            var lblStatus = new Label
            {
                Location = new Point(padding, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = statusVal == "terisi" ? Color.OrangeRed : Color.LimeGreen,
                Text = statusVal == "terisi" ? "Status - Terisi" : "Status - Tersedia"
            };

            panel.Controls.Add(lblStatus);
            y = lblStatus.Bottom + 12;

            // ================= FASILITAS =================
            var lblFasilitas = new Label
            {
                Location = new Point(padding, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Text = "Fasilitas :"
            };

            panel.Controls.Add(lblFasilitas);
            y = lblFasilitas.Bottom + 6;

            var lblKamar = new Label
            {
                Location = new Point(padding, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Text = "Fasilitas Kamar"
            };

            panel.Controls.Add(lblKamar);
            y = lblKamar.Bottom + 4;

            var fasilitasList = item["fasilitas"] as JArray;

            var kamarFasilitas = fasilitasList?
                .Where(f => (string)f["tipe"] == "kamar")
                .SelectMany(f => SplitFacilityString((string)f["nama_fasilitas"]))
                .Take(4)
                .ToList();

            foreach (var f in kamarFasilitas)
            {
                var lbl = new Label
                {
                    Location = new Point(padding + 6, y),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9F),
                    Text = "✔  " + f
                };

                panel.Controls.Add(lbl);
                y += 20;
            }

            y += 6;

            var lblBersama = new Label
            {
                Location = new Point(padding, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Text = "Fasilitas Bersama"
            };

            panel.Controls.Add(lblBersama);
            y = lblBersama.Bottom + 4;

            var bersamaFasilitas = fasilitasList?
                .Where(f => (string)f["tipe"] == "bersama")
                .SelectMany(f => SplitFacilityString((string)f["nama_fasilitas"]))
                .Take(4)
                .ToList();

            foreach (var f in bersamaFasilitas)
            {
                var lbl = new Label
                {
                    Location = new Point(padding + 6, y),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9F),
                    Text = "✔  " + f
                };

                panel.Controls.Add(lbl);
                y += 20;
            }

            y += 10;

            // ================= HARGA =================
            var lblHargaTitle = new Label
            {
                Location = new Point(padding, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Text = "Harga"
            };

            panel.Controls.Add(lblHargaTitle);
            y = lblHargaTitle.Bottom + 5;

            var hargaVal = item["harga_kamar_perbulan"];
            string hargaText = hargaVal != null ? hargaVal.ToString() : "0";

            var lblHarga = new Label
            {
                Location = new Point(padding, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.Green,
                Text = "Rp. " + hargaText
            };

            panel.Controls.Add(lblHarga);

            // ================= BUTTON EDIT =================
            var btnEdit = new Guna2Button
            {
                Size = new Size(cardWidth - padding * 2, 38),
                Location = new Point(padding, panel.Height - 55),
                Text = "Edit",
                BorderRadius = 12,
                FillColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            panel.Controls.Add(btnEdit);

            // ensure safe spacing between harga and button
            if (lblHarga.Bottom > btnEdit.Top - 10)
                btnEdit.Top = lblHarga.Bottom + 12;

            btnEdit.Click += async (s, e) =>
            {
                var parentForm = this.FindForm() as FormUtama;
                if (parentForm == null) return;

                Form bg = new Form();
                try
                {
                    using (FormEditKamar form = new FormEditKamar(item))
                    {
                        bg.StartPosition = FormStartPosition.Manual;
                        bg.FormBorderStyle = FormBorderStyle.None;
                        bg.Opacity = 0.30;
                        bg.BackColor = Color.Black;
                        bg.Size = parentForm.Size;
                        bg.Location = parentForm.Location;
                        bg.Show();

                        form.StartPosition = FormStartPosition.CenterScreen;

                        if (form.ShowDialog() == DialogResult.OK)
                            await LoadKamarAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    bg.Dispose();
                }
            };

            return panel;
        }

        // Try to get image from Resources using several possible keys
        private Image TryGetImageFromResources(string fotoPath)
        {
            if (string.IsNullOrWhiteSpace(fotoPath)) return null;

            var rm = Properties.Resources.ResourceManager;

            // candidates: raw, filename without ext, sanitized variants
            var candidates = new[]
            {
                fotoPath,
                System.IO.Path.GetFileName(fotoPath),
                System.IO.Path.GetFileNameWithoutExtension(fotoPath),
                Regex.Replace(fotoPath, @"[^\w]", "_"),
                Regex.Replace(System.IO.Path.GetFileNameWithoutExtension(fotoPath), @"[^\w]", "_")
            }
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct();

            foreach (var key in candidates)
            {
                try
                {
                    var obj = rm.GetObject(key);
                    if (obj is Image img) return img;
                }
                catch { }
            }

            // no resource found
            return null;
        }

        // Split facility string into parts when combined with &, /, or comma
        private static string[] SplitFacilityString(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return new string[0];
            var parts = input
                .Split(new[] { '&', '/', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();
            return parts;
        }

        private string ResolveImageUrl(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            path = path.Trim();

            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return path;

            // normalize relative path
            while (path.StartsWith("../") || path.StartsWith("./") || path.StartsWith("/") || path.StartsWith("\\"))
            {
                if (path.StartsWith("../")) path = path.Substring(3);
                else if (path.StartsWith("./")) path = path.Substring(2);
                else if (path.StartsWith("/") || path.StartsWith("\\")) path = path.Substring(1);
            }

            return ApiBaseUrl.TrimEnd('/') + "/" + path.TrimStart('/');
        }

        // Designer event handlers
        private void btnTambah_Click(object sender, EventArgs e)
        {
            _ = OnTambahClickedAsync();
        }

        private void btnEditKamar_Click(object sender, EventArgs e)
        {
            _ = LoadKamarAsync();
        }
    }
}

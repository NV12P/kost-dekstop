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
    public partial class IsiRiwayatPemasukan : UserControl
    {
        private const string ApiUrl = "http://localhost:8000/api/keuangan";
        private const int CardSpacing = 15;
        private const int StartY = 16;

        private List<Guna2Panel> cards = new List<Guna2Panel>();
        private Guna2Panel selectedCard = null;

        // =========================
        // PROPERTY UNTUK EDIT DATA
        // =========================
        public int SelectedId { get; private set; }
        public string SelectedKeterangan { get; private set; }
        public long SelectedNominal { get; private set; }

        public event EventHandler SelectionChanged;

        public IsiRiwayatPemasukan()
        {
            InitializeComponent();

            // Sembunyikan panel template jika ada di designer
            if (this.Controls.ContainsKey("panelData"))
                this.Controls["panelData"].Visible = false;

            this.AutoScroll = true;
            this.SizeChanged += (s, e) => ReflowCards();
            this.Load += IsiRiwayatPemasukan_Load;
            this.Click += (s, e) => DeselectAll();
        }

        protected virtual void OnSelectionChanged()
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private string FormatCurrency(long value)
        {
            return "Rp. " + value.ToString("N0");
        }

        private void InternalDeselect()
        {
            if (selectedCard != null)
            {
                selectedCard.FillColor = Color.White;
                foreach (Control c in selectedCard.Controls)
                    c.BackColor = Color.White;
            }
            selectedCard = null;
            SelectedId = 0;
            SelectedKeterangan = null;
            SelectedNominal = 0;
        }

        public void DeselectAll()
        {
            InternalDeselect();
            OnSelectionChanged();
        }

        private void SelectCard(Guna2Panel card)
        {
            if (selectedCard == card)
            {
                DeselectAll();
                return;
            }

            InternalDeselect();

            selectedCard = card;
            Color highlight = Color.FromArgb(210, 232, 255);
            selectedCard.FillColor = highlight;

            foreach (Control c in selectedCard.Controls)
                c.BackColor = highlight;

            if (card.Tag != null)
                SelectedId = Convert.ToInt32(card.Tag);

            var lblKet = card.Controls.OfType<Label>().FirstOrDefault(x => x.Name == "lblKet");
            var lblNom = card.Controls.OfType<Label>().FirstOrDefault(x => x.Name == "lblNom");

            if (lblKet != null) SelectedKeterangan = lblKet.Text;
            if (lblNom != null)
            {
                string clean = new string(lblNom.Text.Where(char.IsDigit).ToArray());
                long.TryParse(clean, out long val);
                SelectedNominal = val;
            }

            OnSelectionChanged();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                foreach (var c in cards)
                {
                    this.Controls.Remove(c);
                    c.Dispose();
                }

                cards.Clear();
                DeselectAll();

                using (var client = new HttpClient())
                {
                    if (!string.IsNullOrWhiteSpace(Session.Token))
                    {
                        client.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);
                    }

                    var resp = await client.GetAsync(ApiUrl);
                    var json = await resp.Content.ReadAsStringAsync();

                    if (!resp.IsSuccessStatusCode) return;

                    var arr = JArray.Parse(json)
                        .Where(x => x["tipe"]?.ToString().ToLower() == "pemasukan") // FILTER PEMASUKAN MANG!
                        .OrderByDescending(x => DateTime.Parse(x["created_at"]?.ToString() ?? DateTime.MinValue.ToString()))
                        .ToList();

                    foreach (var item in arr)
                    {
                        string rawId = item["id_keuangan"]?.ToString() ?? "0";
                        int id = 0;
                        if (rawId.Contains("-")) int.TryParse(rawId.Split('-').Last(), out id);
                        else int.TryParse(rawId, out id);

                        string keterangan = item["keterangan"]?.ToString() ?? "-";
                        long nominal = item["nominal"]?.ToObject<long>() ?? 0;
                        string tanggal = item["created_at"]?.ToString() ?? "";

                        if (DateTime.TryParse(tanggal, out DateTime tgl))
                            tanggal = tgl.ToString("dd MMM yyyy");

                        AddNewCardInternal(id, keterangan, tanggal, nominal);
                    }

                    ReflowCards();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Pemasukan: " + ex.Message);
            }
        }

        private void AddNewCardInternal(int id, string keterangan, string tanggal, long nominal)
        {
            int panelDataWidth = 750; // Fallback jika panelData gak ketemu
            int panelDataHeight = 90;

            Guna2Panel card = new Guna2Panel
            {
                Tag = id,
                BorderColor = Color.LightGray,
                BorderRadius = 18,
                BorderThickness = 2,
                FillColor = Color.White,
                Size = new Size(panelDataWidth, panelDataHeight),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            Label lblKet = new Label
            {
                Name = "lblKet",
                Text = keterangan,
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 18, 101),
                AutoSize = true,
                Location = new Point(19, 20),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            Label lblTgl = new Label
            {
                Text = tanggal,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.DarkGray,
                AutoSize = true,
                Location = new Point(19, 54),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            Label lblNom = new Label
            {
                Name = "lblNom",
                Text = "+ " + FormatCurrency(nominal), // Pake tanda plus biar semangat mang!
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
                ForeColor = Color.LimeGreen, // WARNA HIJAU SESUAI PERMINTAAN MANG!
                AutoSize = true,
                Location = new Point(600, 37),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };

            EventHandler klik = (s, e) => SelectCard(card);
            card.Click += klik;
            lblKet.Click += klik;
            lblTgl.Click += klik;
            lblNom.Click += klik;

            card.Controls.Add(lblKet);
            card.Controls.Add(lblTgl);
            card.Controls.Add(lblNom);

            this.Controls.Add(card);
            cards.Add(card);
        }

        public void FilterData(string keyword)
        {
            keyword = keyword.ToLower();
            foreach (var card in cards)
            {
                var lblKet = card.Controls.OfType<Label>().FirstOrDefault(x => x.Name == "lblKet");
                if (lblKet != null) card.Visible = lblKet.Text.ToLower().Contains(keyword);
            }
            ReflowCards();
        }

        private void ReflowCards()
        {
            int y = StartY;
            int width = this.ClientSize.Width - 40;
            foreach (var card in cards)
            {
                if (!card.Visible) continue;
                card.Width = Math.Max(200, width);
                card.Location = new Point(20, y);
                y += card.Height + CardSpacing;

                var lblNom = card.Controls.OfType<Label>().FirstOrDefault(x => x.Name == "lblNom");
                if (lblNom != null) lblNom.Location = new Point(card.Width - lblNom.Width - 30, lblNom.Location.Y);
            }
        }

        private async void IsiRiwayatPemasukan_Load(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }
    }
}

using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;

namespace KostPakYoyok
{
    public partial class FormEditKamar : Form
    {
        private const string KamarApiUrl = "https://kost.arcv.web.id/api/kamar";
        private int? editId;
        private string selectedFotoPath = ""; 

        // =====================================================
        // CONSTRUCTOR
        // =====================================================
        public FormEditKamar()
        {
            InitializeComponent();
            btnSimpan.Text = "Simpan";
            btnSimpan.Click += btnSimpan_Click;
            btnHapusFoto.Visible = false;
            btnFotoKamar.Width = 385; 
            btnFotoKamar.Text = "Pilih Foto";

            btnFotoKamar.Click += btnFotoKamar_Click;
            btnHapusFoto.Click += btnHapusFoto_Click;
        }

        public FormEditKamar(JToken item) : this()
        {
            if (item == null) return;
            
            editId = item["id_kamar"]?.ToObject<int?>();
            
            textNomorKamar.Text = item["nomor_kamar"]?.ToString() ?? "";
            label1.Text = $"Edit {textNomorKamar.Text}";

            var rawHarga = item["harga_kamar_perbulan"]?.ToString() ?? "0";
            if (long.TryParse(rawHarga, out long hrg))
            {
                textHarga.Text = "Rp. " + hrg.ToString("N0").Replace(",", ".");
            }

            var fasilitas = item["fasilitas"] as JArray;
            if (fasilitas != null)
            {
                var kamar = fasilitas.Where(f => (string)f["tipe"] == "kamar").Select(f => (string)f["nama_fasilitas"]);
                var bersama = fasilitas.Where(f => (string)f["tipe"] == "bersama").Select(f => (string)f["nama_fasilitas"]);
                textFasilitasKamar.Text = string.Join(", ", kamar);
                textFasilitasBersama.Text = string.Join(", ", bersama);
            }
        }

        // =====================================================
        // API METHODS
        // =====================================================
        private async Task SaveEditAsync()
        {
            if (!editId.HasValue) return;

            btnSimpan.Enabled = false;
            var prevCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                var nomor = textNomorKamar.Text.Trim();
                var hargaText = textHarga.Text.Trim();
                var cleanedHarga = new string(hargaText.Where(char.IsDigit).ToArray());

                if (string.IsNullOrEmpty(nomor) || string.IsNullOrEmpty(cleanedHarga))
                {
                    MessageBox.Show("Nomor kamar dan harga wajib diisi!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(60);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);

                    var content = new MultipartFormDataContent();
                    
                    content.Add(new StringContent("PUT"), "_method");
                    content.Add(new StringContent(nomor), "nomor_kamar");
                    content.Add(new StringContent(cleanedHarga), "harga_kamar_perbulan");

                    var fasKamar = textFasilitasKamar.Text.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
                    foreach (var f in fasKamar) {
                        content.Add(new StringContent(f), "fasilitas_kamar[]");
                    }

                    var fasBersama = textFasilitasBersama.Text.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
                    foreach (var f in fasBersama) {
                        content.Add(new StringContent(f), "fasilitas_bersama[]");
                    }

                    if (!string.IsNullOrWhiteSpace(selectedFotoPath) && System.IO.File.Exists(selectedFotoPath))
                    {
                        var bytes = System.IO.File.ReadAllBytes(selectedFotoPath);
                        var fileContent = new ByteArrayContent(bytes);
                        string ext = System.IO.Path.GetExtension(selectedFotoPath).ToLower();
                        string mimeType = (ext == ".png") ? "image/png" : (ext == ".webp" ? "image/webp" : "image/jpeg");
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
                        content.Add(fileContent, "foto_kamar", System.IO.Path.GetFileName(selectedFotoPath));
                    }

                    var resp = await client.PostAsync($"{KamarApiUrl}/{editId.Value}", content);
                    var respStr = await resp.Content.ReadAsStringAsync();

                    if (resp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Kamar berhasil diupdate!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Gagal simpan: " + respStr, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fatal: " + ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSimpan.Enabled = true;
                Cursor.Current = prevCursor;
            }
        }

        // =====================================================
        // EVENT HANDLERS
        // =====================================================
        private void btnFotoKamar_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Pilih Foto Kamar Baru";
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    selectedFotoPath = ofd.FileName;
                    btnHapusFoto.Visible = true;
                    btnFotoKamar.Width = 340; 
                    btnFotoKamar.Text = System.IO.Path.GetFileName(selectedFotoPath);
                }
            }
        }

        private void btnHapusFoto_Click(object sender, EventArgs e)
        {
            selectedFotoPath = "";
            btnHapusFoto.Visible = false;
            btnFotoKamar.Width = 385; 
            btnFotoKamar.Text = "Pilih Foto";
        }

        private void btnSimpan_Click(object sender, EventArgs e)
        {
            _ = SaveEditAsync();
        }

        private void textHarga_TextChanged(object sender, EventArgs e)
        {
            textHarga.TextChanged -= textHarga_TextChanged;
            try {
                string val = new string(textHarga.Text.Where(char.IsDigit).ToArray());
                if (long.TryParse(val, out long p)) {
                    textHarga.Text = "Rp. " + p.ToString("N0").Replace(",", ".");
                    textHarga.SelectionStart = textHarga.Text.Length;
                } else { textHarga.Text = ""; }
            } catch { }
            textHarga.TextChanged += textHarga_TextChanged;
        }

        private void label5_Click(object sender, EventArgs e) { }
    }
}

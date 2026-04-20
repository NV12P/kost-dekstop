using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KostPakYoyok
{
    public partial class FormEditKamar : Form
    {
        private const string KamarApiUrl = "http://localhost:8000/api/kamar";
        private int? editId;
        private string selectedFotoPath = ""; // Buat simpen foto pilihan mang

        public FormEditKamar()
        {
            InitializeComponent();
            // default button text and wire click
            btnSimpan.Text = "Simpan";
            btnSimpan.Click += btnSimpan_Click;

            // Tombol hapus sembunyi dulu mang
            btnHapusFoto.Visible = false;

            // Pasang event klik mang
            btnFotoKamar.Click += btnFotoKamar_Click;
            btnHapusFoto.Click += btnHapusFoto_Click;
        }

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
                    MessageBox.Show("Foto kamar baru berhasil dipilih.");
                }
            }
        }

        private void btnHapusFoto_Click(object sender, EventArgs e)
        {
            selectedFotoPath = "";
            btnHapusFoto.Visible = false;
            MessageBox.Show("Foto kamar baru dibatalkan.");
        }

        // Konstruktor untuk membuka form edit dengan data API (item dari /api/kamar)
        public FormEditKamar(JToken item) : this()
        {
            if (item == null) return;

            // simpan id untuk PUT
            editId = item["id_kamar"]?.ToObject<int?>();

            // tampilkan nomor/keterangan di judul jika ada
            var nomor = item["nomor_kamar"]?.ToString() ?? item["nomor"]?.ToString() ?? ("ID " + (editId?.ToString() ?? ""));
            label1.Text = $"Edit {nomor}";

            // isi field yang tersedia pada designer
            textHarga.Text = item["harga_kamar_perbulan"]?.ToString() ?? item["harga"]?.ToString() ?? "";
            var fasilitas = item["fasilitas"] as JArray;
            if (fasilitas != null)
            {
                var kamar = fasilitas.Where(f => (string)f["tipe"] == "kamar").SelectMany(f => f["nama_fasilitas"].ToString().Split(new[] { '&', ',', '/' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));
                var bersama = fasilitas.Where(f => (string)f["tipe"] == "bersama").SelectMany(f => f["nama_fasilitas"].ToString().Split(new[] { '&', ',', '/' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));
                textFasilitasKamar.Text = string.Join(", ", kamar);
                textFasilitasBersama.Text = string.Join(", ", bersama);
            }
        }

        private void btnSimpan_Click(object sender, EventArgs e)
        {
            _ = SaveEditAsync();
        }

        private async Task SaveEditAsync()
        {
            btnSimpan.Enabled = false;
            var prevCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                if (!editId.HasValue)
                {
                    MessageBox.Show("ID kamar tidak tersedia.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var hargaText = textHarga.Text?.Trim() ?? "";
                var fasilitasKamarText = textFasilitasKamar.Text?.Trim() ?? "";
                var fasilitasBersamaText = textFasilitasBersama.Text?.Trim() ?? "";

                if (string.IsNullOrEmpty(hargaText))
                {
                    MessageBox.Show("Harga tidak boleh kosong.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var cleanedHarga = hargaText.Replace(",", "").Replace(".", "").Trim();
                if (!long.TryParse(cleanedHarga, out long hargaValue))
                {
                    MessageBox.Show("Format harga tidak valid.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Session.Token))
                {
                    MessageBox.Show("Token tidak tersedia. Silakan login ulang.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string[] fasilitasKamar = new string[0];
                string[] fasilitasBersama = new string[0];

                if (!string.IsNullOrWhiteSpace(fasilitasKamarText))
                    fasilitasKamar = fasilitasKamarText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

                if (!string.IsNullOrWhiteSpace(fasilitasBersamaText))
                    fasilitasBersama = fasilitasBersamaText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);

                    // Pake Multipart biar fotonya bisa dikirim mang!
                    var content = new MultipartFormDataContent();

                    content.Add(new StringContent(hargaValue.ToString()), "harga_kamar_perbulan");

                    if (fasilitasKamar.Length > 0)
                        content.Add(new StringContent(string.Join(",", fasilitasKamar)), "fasilitas_kamar");

                    if (fasilitasBersama.Length > 0)
                        content.Add(new StringContent(string.Join(",", fasilitasBersama)), "fasilitas_bersama");

                    // Kalau ada foto baru, masukin ke paket mang!
                    if (!string.IsNullOrWhiteSpace(selectedFotoPath))
                    {
                        var bytes = System.IO.File.ReadAllBytes(selectedFotoPath);
                        var fileContent = new ByteArrayContent(bytes);
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                        content.Add(fileContent, "foto_kamar", System.IO.Path.GetFileName(selectedFotoPath));
                    }

                    // Laravel kadang rewel sama PUT multipart mang, kita akalin pake _method mang!
                    content.Add(new StringContent("PUT"), "_method");

                    HttpResponseMessage resp;
                    try
                    {
                        var url = $"{KamarApiUrl}/{editId.Value}";
                        resp = await client.PostAsync(url, content); // Kirim via POST tapi isinya PUT mang
                    }
                    catch (HttpRequestException ex)
                    {
                        MessageBox.Show("Network error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    catch (TaskCanceledException ex)
                    {
                        MessageBox.Show("Request timed out: " + ex.Message, "Timeout", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var respJson = await resp.Content.ReadAsStringAsync();

                    if (!resp.IsSuccessStatusCode)
                    {
                        string serverMessage = respJson;
                        try
                        {
                            var parsed = JObject.Parse(respJson);
                            serverMessage = (string)parsed["message"] ?? serverMessage;
                        }
                        catch { }

                        MessageBox.Show($"Server error: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{serverMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    MessageBox.Show("Kamar berhasil diubah.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            finally
            {
                btnSimpan.Enabled = true;
                Cursor.Current = prevCursor;
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textHarga_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

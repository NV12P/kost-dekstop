using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KostPakYoyok
{
    public partial class FormKamar : Form
    {
        // API endpoint for create/update
        private const string KamarApiUrl = "http://localhost:8000/api/kamar";
        private int? editId = null;
        private bool isEditMode = false;
        private string selectedFotoPath = ""; // Variabel buat simpen path foto mang

        public FormKamar()
        {
            InitializeComponent();
            // Ensure button text default (designer has "Tambah Kamar")
            btnTambahKamar.Text = "Tambah Kamar";

            // Pas baru buka, tombol hapus foto sembunyiin dulu mang
            btnHapusFoto.Visible = false;

            // Pasang event klik mang
            btnFotoKamar.Click += btnFotoKamar_Click;
            btnHapusFoto.Click += btnHapusFoto_Click;
        }

        private void btnFotoKamar_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Pilih Foto Kamar";
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    selectedFotoPath = ofd.FileName;
                    btnHapusFoto.Visible = true; // Munculin tombol hapusnya mang!
                    MessageBox.Show("Foto kamar berhasil dipilih.");
                }
            }
        }

        private void btnHapusFoto_Click(object sender, EventArgs e)
        {
            selectedFotoPath = "";
            btnHapusFoto.Visible = false; // Sembunyiin lagi mang
            MessageBox.Show("Foto kamar dibatalkan.");
        }

        // New constructor: supply item to open in edit mode
        public FormKamar(JToken item, bool editMode = true) : this()
        {
            if (item != null)
            {
                isEditMode = editMode;
                editId = item["id_kamar"]?.ToObject<int?>();

                // Prefill fields using API response keys
                textNomorKamar.Text = item["nomor_kamar"]?.ToString() ?? "";
                textHarga.Text = item["harga_kamar_perbulan"]?.ToString() ?? item["harga"]?.ToString() ?? "";

                // Populate fasilitas inputs as comma separated strings
                var fasilitas = item["fasilitas"] as JArray;
                if (fasilitas != null)
                {
                    var kamar = fasilitas.Where(f => (string)f["tipe"] == "kamar").Select(f => (string)f["nama_fasilitas"]);
                    var bersama = fasilitas.Where(f => (string)f["tipe"] == "bersama").Select(f => (string)f["nama_fasilitas"]);

                    textFasilitasKamar.Text = string.Join(", ", kamar);
                    textFasilitasBersama.Text = string.Join(", ", bersama);
                }

                btnTambahKamar.Text = "Simpan Perubahan";
            }
        }

        private void FormKamar_Load(object sender, EventArgs e)
        {
            // noop
        }

        // Designer wires btnTambahKamar -> btnSimpan_Click
        private void btnSimpan_Click(object sender, EventArgs e)
        {
            _ = SaveKamarAsync();
        }

        private async Task SaveKamarAsync()
        {
            btnTambahKamar.Enabled = false;
            var prevCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                var nomor = textNomorKamar.Text?.Trim() ?? "";
                var fasilitasKamarText = textFasilitasKamar.Text?.Trim() ?? "";
                var fasilitasBersamaText = textFasilitasBersama.Text?.Trim() ?? "";
                var hargaText = textHarga.Text?.Trim() ?? "";

                if (string.IsNullOrEmpty(nomor))
                {
                    MessageBox.Show("Nomor kamar tidak boleh kosong.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

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

                    content.Add(new StringContent(nomor), "nomor_kamar");
                    content.Add(new StringContent(hargaValue.ToString()), "harga_kamar_perbulan");

                    if (fasilitasKamar.Length > 0)
                        content.Add(new StringContent(string.Join(",", fasilitasKamar)), "fasilitas_kamar");

                    if (fasilitasBersama.Length > 0)
                        content.Add(new StringContent(string.Join(",", fasilitasBersama)), "fasilitas_bersama");

                    // Kalau ada foto, masukin ke paket mang!
                    if (!string.IsNullOrWhiteSpace(selectedFotoPath))
                    {
                        var bytes = System.IO.File.ReadAllBytes(selectedFotoPath);
                        var fileContent = new ByteArrayContent(bytes);
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                        content.Add(fileContent, "foto_kamar", System.IO.Path.GetFileName(selectedFotoPath));
                    }

                    HttpResponseMessage resp;
                    try
                    {
                        if (isEditMode && editId.HasValue)
                        {
                            // PUT to update
                            // Catatan: Laravel kadang minta _method=PUT kalau pake Multipart mang!
                            content.Add(new StringContent("PUT"), "_method");
                            var url = $"{KamarApiUrl}/{editId.Value}";
                            resp = await client.PostAsync(url, content); // Kirim via POST tapi isinya PUT mang
                        }
                        else
                        {
                            resp = await client.PostAsync(KamarApiUrl, content);
                        }
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

                    MessageBox.Show(isEditMode ? "Kamar berhasil diubah." : "Kamar berhasil ditambahkan.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            finally
            {
                btnTambahKamar.Enabled = true;
                Cursor.Current = prevCursor;
            }
        }

        private void textFasilitasKamar_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

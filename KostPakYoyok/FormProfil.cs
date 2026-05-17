using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KostPakYoyok
{
    public partial class FormProfil : Form
    {
        private const string ProfileApiUrl = "https://kost.arcv.web.id/api/profile";

        // =====================================================
        // CONSTRUCTOR
        // =====================================================
        public FormProfil()
        {
            InitializeComponent();

            btnSimpan.Click += BtnSimpan_Click;
            textNama.Text = Session.Nama ?? string.Empty;
        }

        // =====================================================
        // API METHODS
        // =====================================================
        private async Task SaveProfileAsync()
        {
            btnSimpan.Enabled = false;
            var prevCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                var newName = textNama.Text?.Trim() ?? "";
                var newPassword = textPasswordBaru.Text ?? "";
                var confirmPassword = textKonfirmasiPassword.Text ?? "";

                if (string.IsNullOrEmpty(newName))
                {
                    MessageBox.Show("Nama tidak boleh kosong.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!string.IsNullOrEmpty(newPassword) || !string.IsNullOrEmpty(confirmPassword))
                {
                    if (newPassword != confirmPassword)
                    {
                        MessageBox.Show("Password baru dan konfirmasi tidak cocok.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (newPassword.Length < 3)
                    {
                        MessageBox.Show("Password baru minimal 3 karakter.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                if (string.IsNullOrWhiteSpace(Session.Token))
                {
                    MessageBox.Show("Token tidak tersedia. Silakan login ulang.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);

                    // Sinkronisasi Nama dan Username biar satu data mang!
                    var body = new JObject
                    {
                        ["nama_profile"] = newName,
                        ["username"] = newName 
                    };

                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        body["password"] = newPassword;
                        body["password_confirmation"] = confirmPassword;
                    }

                    var content = new StringContent(body.ToString(Formatting.None), Encoding.UTF8, "application/json");

                    HttpResponseMessage resp;
                    try
                    {
                        resp = await client.PutAsync(ProfileApiUrl, content);
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

                    try
                    {
                        var parsed = JObject.Parse(respJson);

                        string updatedValue =
                            (string)parsed["data"]?["nama_profile"]
                            ?? (string)parsed["data"]?["username"]
                            ?? (string)parsed["user"]?["nama_profile"]
                            ?? (string)parsed["user"]?["username"]
                            ?? (string)parsed["nama"]
                            ?? (string)parsed["name"]
                            ?? newName;

                        Session.Nama = updatedValue;
                        Session.Username = updatedValue;

                        var mainForm = System.Windows.Forms.Application.OpenForms
                            .OfType<FormUtama>()
                            .FirstOrDefault();

                        if (mainForm != null)
                            mainForm.SetUserName(Session.Nama);

                        MessageBox.Show("Profil berhasil diperbarui.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Simpan berhasil, tapi gagal memproses response server:\n" + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        this.Close();
                    }
                }
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
        private async void BtnSimpan_Click(object sender, EventArgs e)
        {
            await SaveProfileAsync();
        }
    }
}

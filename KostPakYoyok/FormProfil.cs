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
        // Adjust endpoint if your API uses a different URL
        private const string ProfileApiUrl = "http://localhost:8000/api/profile";

        public FormProfil()
        {
            InitializeComponent();

            // wire up event
            btnSimpan.Click += BtnSimpan_Click;

            // Load current session name into textbox
            textNama.Text = Session.Nama ?? string.Empty;
        }

        private async void BtnSimpan_Click(object sender, EventArgs e)
        {
            await SaveProfileAsync();
        }

        private async Task SaveProfileAsync()
        {
            btnSimpan.Enabled = false;
            var prevCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                // Basic validation
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

                    // Build request body. Adjust property names to match your API (here using 'nama_profile')
                    var body = new JObject
                    {
                        ["nama_profile"] = newName
                    };

                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        // common field names: password & password_confirmation
                        body["password"] = newPassword;
                        body["password_confirmation"] = confirmPassword;
                    }

                    var content = new StringContent(body.ToString(Formatting.None), Encoding.UTF8, "application/json");

                    HttpResponseMessage resp;
                    try
                    {
                        // Use PUT as typical for profile update; change to PostAsync if your API expects POST
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
                        // Try extract message from JSON if present
                        string serverMessage = respJson;
                        try
                        {
                            var parsed = JObject.Parse(respJson);
                            serverMessage = (string)parsed["message"] ?? serverMessage;
                        }
                        catch { /* ignore parse errors */ }

                        MessageBox.Show($"Server error: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{serverMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Parse success response and update Session + UI
                    try
                    {
                        var parsed = JObject.Parse(respJson);

                        // Depending on API, updated user data might be in parsed["data"] or parsed["user"]
                        string updatedName =
                            (string)parsed["data"]?["nama_profile"]
                            ?? (string)parsed["data"]?["name"]
                            ?? (string)parsed["user"]?["nama_profile"]
                            ?? (string)parsed["user"]?["name"]
                            ?? (string)parsed["nama"]
                            ?? (string)parsed["name"]
                            ?? newName;

                        // Save to session
                        Session.Nama = updatedName;

                        // Update FormUtama label if open
                        var mainForm = System.Windows.Forms.Application.OpenForms
                            .OfType<FormUtama>()
                            .FirstOrDefault();

                        if (mainForm != null)
                            mainForm.SetUserName(Session.Nama);

                        MessageBox.Show("Profil berhasil disimpan.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Close dialog after successful save
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
    }
}

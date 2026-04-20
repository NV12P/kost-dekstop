using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KostPakYoyok
{
    public partial class LoginControl : UserControl
    {
        // Event untuk berpindah ke register
        public event EventHandler GoToRegisterClicked;

        public LoginControl()
        {
            InitializeComponent();
        }

        private void LoginControl_Load(object sender, EventArgs e)
        {
        }

        private void linkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GoToRegisterClicked?.Invoke(this, EventArgs.Empty);
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            btnLogin.Enabled = false;
            var prevCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                using (var client = new HttpClient())
                {
                    var data = new
                    {
                        login_identity = textUsername.Text,
                        password = textPassword.Text
                    };

                    var json = JsonConvert.SerializeObject(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response;
                    try
                    {
                        response = await client.PostAsync("http://127.0.0.1:8000/api/login", content);
                    }
                    catch (HttpRequestException httpEx)
                    {
                        MessageBox.Show("Network error: " + httpEx.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    catch (TaskCanceledException tcEx)
                    {
                        MessageBox.Show("Request timed out: " + tcEx.Message, "Timeout", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var result = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Server returned {(int)response.StatusCode} {response.ReasonPhrase}\nResponse: {result}", "Server error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    JObject resObj;
                    try
                    {
                        resObj = JObject.Parse(result);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Invalid JSON from server:\n" + result, "Parse error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // coba baca status/message bila ada
                    var status = (string)resObj["status"] ?? (string)resObj["meta"]?["status"];
                    var message = (string)resObj["message"] ?? (string)resObj["meta"]?["message"] ?? "";

                    bool ok = string.IsNullOrEmpty(status) ? response.IsSuccessStatusCode : status.Equals("success", StringComparison.OrdinalIgnoreCase);

                    if (ok)
                    {
                        // Ambil token dari beberapa kemungkinan lokasi respons
                        string token = (string)resObj["token"]
                            ?? (string)resObj["access_token"]
                            ?? (string)resObj["data"]?["token"]
                            ?? (string)resObj["data"]?["access_token"];

                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            Session.Token = token; // simpan token agar kontrol lain (DashboardControl) bisa pakai
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Login succeeded but no token found in response: " + result);
                        }

                        // Ambil nama user dari beberapa kemungkinan lokasi respons
                        string namaUser = (string)resObj["user"]?["nama_profile"]
                            ?? (string)resObj["user"]?["name"]
                            ?? (string)resObj["user"]?["nama"]
                            ?? (string)resObj["data"]?["user"]?["nama_profile"]
                            ?? (string)resObj["data"]?["user"]?["name"]
                            ?? (string)resObj["name"]
                            ?? (string)resObj["nama"]
                            ?? textUsername.Text; // fallback pakai username input

                        Session.Nama = namaUser;

                        // Buka form utama dan set nama
                        FormUtama dashboard = new FormUtama();
                        dashboard.StartPosition = FormStartPosition.CenterScreen;

                        // set nama sebelum show agar segera terlihat
                        dashboard.SetUserName(Session.Nama);

                        dashboard.Show();
                        dashboard.BringToFront();
                        dashboard.Activate();

                        // Sembunyikan form account (jangan Close() agar Application.Run tidak berhenti)
                        var parentForm = this.FindForm();
                        parentForm?.Hide();
                    }
                    else
                    {
                        MessageBox.Show("Login gagal: " + (string.IsNullOrEmpty(message) ? result : message), "Login failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            finally
            {
                btnLogin.Enabled = true;
                Cursor.Current = prevCursor;
            }
        }
    }
}

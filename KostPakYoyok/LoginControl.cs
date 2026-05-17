using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KostPakYoyok
{
    public partial class LoginControl : UserControl
    {
        public event EventHandler GoToRegisterClicked;

        // =====================================================
        // CONSTRUCTOR
        // =====================================================
        public LoginControl()
        {
            InitializeComponent();
        }

        // =====================================================
        // EVENT HANDLERS
        // =====================================================
        private void LoginControl_Load(object sender, EventArgs e)
        {
        }

        private void linkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GoToRegisterClicked?.Invoke(this, EventArgs.Empty);
        }

        // =====================================================
        // AUTHENTICATION LOGIC
        // =====================================================
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            // Reset & Force Security Protocol
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | (SecurityProtocolType)3072 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;

            btnLogin.Enabled = false;
            var prevCursor = System.Windows.Forms.Cursor.Current;
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

            try
            {
                // Gunakan handler biar lebih mantap tembus SSL-nya mang!
                var handler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                    Proxy = null,
                    UseProxy = false
                };

                using (var client = new HttpClient(handler))
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
                        response = await client.PostAsync("https://kost.arcv.web.id/api/login", content);
                    }
                    catch (HttpRequestException httpEx)
                    {
                        string detailedError = httpEx.Message;
                        if (httpEx.InnerException != null) detailedError += "\nDetail: " + httpEx.InnerException.Message;
                        
                        MessageBox.Show("Kendala Jaringan:\n" + detailedError, "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var result = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Server Error {(int)response.StatusCode}\nMsg: {result}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    JObject resObj = JObject.Parse(result);
                    var status = (string)resObj["status"] ?? (string)resObj["meta"]?["status"];
                    var message = (string)resObj["message"] ?? (string)resObj["meta"]?["message"] ?? "";

                    bool ok = string.IsNullOrEmpty(status) ? response.IsSuccessStatusCode : status.Equals("success", StringComparison.OrdinalIgnoreCase);

                    if (ok)
                    {
                        string token = (string)resObj["token"] ?? (string)resObj["access_token"] ?? (string)resObj["data"]?["token"] ?? (string)resObj["data"]?["access_token"];
                        if (!string.IsNullOrWhiteSpace(token)) Session.Token = token;

                        string namaUser = (string)resObj["user"]?["nama_profile"] ?? (string)resObj["user"]?["name"] ?? (string)resObj["data"]?["user"]?["nama_profile"] ?? textUsername.Text;
                        string userIdent = (string)resObj["user"]?["username"] ?? (string)resObj["data"]?["user"]?["username"] ?? textUsername.Text;
                        
                        Session.Nama = namaUser;
                        Session.Username = userIdent;

                        FormUtama dashboard = new FormUtama();
                        dashboard.StartPosition = FormStartPosition.CenterScreen;
                        dashboard.SetUserName(Session.Nama);
                        dashboard.Show();

                        this.FindForm()?.Hide();
                    }
                    else
                    {
                        MessageBox.Show("Gagal: " + (string.IsNullOrEmpty(message) ? result : message), "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi Kesalahan:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLogin.Enabled = true;
                System.Windows.Forms.Cursor.Current = prevCursor;
            }
        }
    }
}

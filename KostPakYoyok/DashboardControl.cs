using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KostPakYoyok
{
    public partial class DashboardControl : UserControl
    {
        // Endpoint GET /api/dashboard
        private const string DashboardApiUrl = "https://kost.arcv.web.id/api/dashboard";

        public DashboardControl()
        {
            InitializeComponent();
        }

        // Dipanggil saat control di-load
        private async void DashboardControl_Load(object sender, EventArgs e)
        {
            await LoadDashboardAsync();
        }

        // Tombol "Tahun ini"
        private async void btnResultTahun_Click(object sender, EventArgs e)
        {
            await LoadDashboardAsync();
        }

        // Ambil data dari API menggunakan Bearer token (Session.Token) dan update UI
        private async Task LoadDashboardAsync()
        {
            btnResultTahun.Enabled = false;
            var previousCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                if (string.IsNullOrWhiteSpace(Session.Token))
                {
                    MessageBox.Show("Token belum tersedia. Silakan login terlebih dahulu.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        response = await client.GetAsync(DashboardApiUrl);
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

                    DashboardResponse res;
                    try
                    {
                        res = JsonConvert.DeserializeObject<DashboardResponse>(json);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal mem-parse response dari server:\n" + ex.Message + "\n\nResponse:\n" + json, "Parse error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Ensure chart area exists and series "Sewa" exists
                    if (chart1.ChartAreas.Count == 0) return;
                    var area = chart1.ChartAreas[0];
                    if (chart1.Series.IndexOf("Sewa") < 0) return;

                    // Prepare months Jan..Dec in order and integer Y range 0..5
                    int[] monthValues = Enumerable.Repeat(0, 13).ToArray(); // index 0 unused

                    if (res?.sewa_aktif != null)
                    {
                        foreach (var item in res.sewa_aktif)
                        {
                            try
                            {
                                int bulan = item.bulan;
                                if (bulan < 1 || bulan > 12) continue;

                                int val = Convert.ToInt32(Math.Round(Convert.ToDouble(item.total)));
                                monthValues[bulan] += val;
                            }
                            catch
                            {
                                // ignore malformed items
                            }
                        }
                    }

                    // Clamp aggregated results to 0..5
                    for (int m = 1; m <= 12; m++)
                        monthValues[m] = Math.Max(0, Math.Min(5, monthValues[m]));

                    // Clear and repopulate series with months January..December
                    var series = chart1.Series["Sewa"];
                    series.Points.Clear();

                    // Ensure data labels are disabled (no numbers above X axis)
                    series.IsValueShownAsLabel = false;

                    for (int m = 1; m <= 12; m++)
                    {
                        string label = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m);
                        series.Points.AddXY(label, monthValues[m]);
                    }

                    // Configure X axis to show all months in order and avoid skipping
                    area.AxisX.Interval = 1;
                    area.AxisX.MajorGrid.Enabled = false;

                    // Configure Y axis to range 0..5 with integer ticks and no decimals
                    area.AxisY.Minimum = 0;
                    area.AxisY.Maximum = 5;
                    area.AxisY.Interval = 1;
                    area.AxisY.LabelStyle.Format = "0";
                    area.AxisY.MajorGrid.Enabled = true;

                    // Update kartu ringkasan:
                    if (res != null)
                    {
                        labelPemasukan.Text = "Rp " + res.pemasukan.ToString("N0", CultureInfo.InvariantCulture);
                        labelPengeluaran.Text = "Rp " + res.pengeluaran.ToString("N0", CultureInfo.InvariantCulture);
                        labelKamarTersedia.Text = res.kamar_tersedia.ToString();
                    }
                }
            }
            finally
            {
                btnResultTahun.Enabled = true;
                Cursor.Current = previousCursor;
            }
        }

        // Placeholder jika ada event lain pada designer
        private void guna2Button3_Click(object sender, EventArgs e)
        {
        }
    }
}

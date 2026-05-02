using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace KostPakYoyok
{
    public partial class DashboardControl : UserControl
    {
        private const string DashboardApiUrl = "https://kost.arcv.web.id/api/dashboard";

        public DashboardControl()
        {
            InitializeComponent();
        }

        private async void DashboardControl_Load(object sender, EventArgs e)
        {
            await LoadDashboardAsync();
        }

        private async void btnResultTahun_Click(object sender, EventArgs e)
        {
            await LoadDashboardAsync();
        }

        private async Task LoadDashboardAsync()
        {
            btnResultTahun.Enabled = false;
            var previousCursor = System.Windows.Forms.Cursor.Current;
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

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
                    try { response = await client.GetAsync(DashboardApiUrl); }
                    catch (Exception ex) { MessageBox.Show("Error Koneksi: " + ex.Message); return; }

                    var json = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode) return;

                    DashboardResponse res = JsonConvert.DeserializeObject<DashboardResponse>(json);

                    if (chart1.ChartAreas.Count == 0) return;
                    var area = chart1.ChartAreas[0];
                    if (chart1.Series.Count == 0) return;
                    var series = chart1.Series[0];
                    series.Points.Clear();

                    int[] monthValues = new int[13]; 
                    if (res?.sewa_aktif != null) {
                        foreach (var item in res.sewa_aktif) {
                            if (item.bulan >= 1 && item.bulan <= 12) monthValues[item.bulan] += item.total;
                        }
                    }

                    int maxVal = monthValues.Max();
                    if (maxVal < 5) maxVal = 5;

                    series.ChartType = SeriesChartType.Spline;
                    series.Color = System.Drawing.Color.FromArgb(26, 18, 101); 
                    series.BorderWidth = 1; 
                    series["LineTension"] = "0.3"; 
                    series.MarkerStyle = MarkerStyle.None;
                    
                    if (chart1.Legends.Count > 0) chart1.Legends[0].Enabled = false;

                    var idCulture = new CultureInfo("id-ID");
                    for (int m = 1; m <= 12; m++) {
                        string label = idCulture.DateTimeFormat.GetAbbreviatedMonthName(m);
                        if (label.Length > 1) label = char.ToUpper(label[0]) + label.Substring(1).ToLower();
                        series.Points.AddXY(label, monthValues[m]);
                    }

                    area.AxisX.Interval = 1;
                    area.AxisX.MajorGrid.Enabled = false;
                    area.AxisX.LineColor = System.Drawing.Color.FromArgb(209, 213, 219); 
                    area.AxisX.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 8);
                    area.AxisX.LabelStyle.ForeColor = System.Drawing.Color.DimGray;
                    area.AxisX.IsMarginVisible = true; 

                    area.AxisY.Minimum = 0;
                    area.AxisY.Maximum = maxVal + 1;
                    area.AxisY.Interval = 1; 
                    area.AxisY.LabelStyle.Format = "0"; 
                    area.AxisY.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 8);
                    area.AxisY.LabelStyle.ForeColor = System.Drawing.Color.DimGray;
                    area.AxisY.LineColor = System.Drawing.Color.Transparent;
                    
                    area.AxisY.MajorGrid.Enabled = true;
                    area.AxisY.MajorGrid.LineColor = System.Drawing.Color.FromArgb(226, 232, 240);
                    area.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Solid; 

                    area.InnerPlotPosition = new ElementPosition(7, 5, 90, 85);
                    chart1.BackColor = System.Drawing.Color.White;
                    area.BackColor = System.Drawing.Color.White;
                    
                    if (res != null) {
                        labelPemasukan.Text = "Rp " + res.pemasukan.ToString("N0", CultureInfo.InvariantCulture);
                        labelPengeluaran.Text = "Rp " + res.pengeluaran.ToString("N0", CultureInfo.InvariantCulture);
                        labelKamarTersedia.Text = res.kamar_tersedia.ToString();
                    }
                }
            }
            finally {
                btnResultTahun.Enabled = true;
                System.Windows.Forms.Cursor.Current = previousCursor;
            }
        }

        private void guna2Button3_Click(object sender, EventArgs e) { }
    }
}

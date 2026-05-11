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

        // =====================================================
        // INITIALIZE & EVENTS
        // =====================================================
        private async void DashboardControl_Load(object sender, EventArgs e)
        {
            chart1.BringToFront();
            await LoadDashboardAsync();
        }

        private async void btnResultTahun_Click(object sender, EventArgs e)
        {
            chart1.BringToFront();
            await LoadDashboardAsync();
        }

        // =====================================================
        // API DATA LOADING
        // =====================================================
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
                    try 
                    { 
                        response = await client.GetAsync(DashboardApiUrl); 
                    }
                    catch (Exception) 
                    { 
                        // FALLBACK: Tampilkan data dummy saat server down
                        ShowMockData();
                        return; 
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode) 
                    {
                        ShowMockData();
                        return;
                    }

                    DashboardResponse res = JsonConvert.DeserializeObject<DashboardResponse>(json);
                    ProcessDashboardData(res);
                }
            }
            finally 
            {
                btnResultTahun.Enabled = true;
                System.Windows.Forms.Cursor.Current = previousCursor;
            }
        }

        private void ShowMockData()
        {
            var mockRes = new DashboardResponse
            {
                pemasukan = 15000000,
                pengeluaran = 3500000,
                kamar_tersedia = 8,
                sewa_aktif = new List<SewaAktif>
                {
                    new SewaAktif { bulan = 1, total = 2 },
                    new SewaAktif { bulan = 2, total = 3 },
                    new SewaAktif { bulan = 3, total = 1 },
                    new SewaAktif { bulan = 4, total = 4 },
                    new SewaAktif { bulan = 5, total = 5 },
                    new SewaAktif { bulan = 6, total = 3 }
                }
            };
            ProcessDashboardData(mockRes);
        }

        private void ProcessDashboardData(DashboardResponse res)
        {
            if (chart1.ChartAreas.Count == 0) return;
            var area = chart1.ChartAreas[0];
            if (chart1.Series.Count == 0) return;
            var series = chart1.Series[0];
            series.Points.Clear();

            int[] monthValues = new int[13];
            if (res?.sewa_aktif != null)
            {
                foreach (var item in res.sewa_aktif)
                {
                    if (item.bulan >= 1 && item.bulan <= 12)
                    {
                        monthValues[item.bulan] += item.total;
                        if (monthValues[item.bulan] > 5) monthValues[item.bulan] = 5;
                    }
                }
            }

            // =====================================================
            // CHART STYLING (MATCHING REACT RECHARTS)
            // =====================================================
            series.ChartType = SeriesChartType.Spline;
            series.Color = Color.FromArgb(30, 27, 109); // #1E1B6D
            series.BorderWidth = 3;
            series["LineTension"] = "0.4";

            series.MarkerStyle = MarkerStyle.Circle;
            series.MarkerSize = 10;
            series.MarkerColor = Color.FromArgb(30, 27, 109);
            series.MarkerBorderColor = Color.White;
            series.MarkerBorderWidth = 2;
            series.ToolTip = "Bulan: #VALX\nTotal: #VALY";

            if (chart1.Legends.Count > 0) chart1.Legends[0].Enabled = false;

            string[] monthsArr = { "Jan", "Feb", "Mar", "Apr", "Mei", "Jun", "Jul", "Agu", "Sep", "Okt", "Nov", "Des" };
            for (int m = 1; m <= 12; m++)
            {
                series.Points.AddXY(monthsArr[m - 1], monthValues[m]);
            }

            area.AxisX.Interval = 1;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.LineColor = Color.Transparent;
            area.AxisX.MajorTickMark.Enabled = false;
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 9);
            area.AxisX.LabelStyle.ForeColor = Color.FromArgb(156, 163, 175);
            area.AxisX.IsMarginVisible = true;

            area.AxisY.Minimum = -0.5;
            area.AxisY.Maximum = 5.5;
            area.AxisY.Interval = 1;
            area.AxisY.IntervalOffset = 0.5;
            area.AxisY.LabelStyle.Format = "0";
            area.AxisY.LabelStyle.Font = new Font("Segoe UI", 9);
            area.AxisY.LabelStyle.ForeColor = Color.FromArgb(156, 163, 175);
            area.AxisY.LineColor = Color.Transparent;
            area.AxisY.MajorTickMark.Enabled = false;

            area.AxisY.MajorGrid.Enabled = true;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(230, 228, 216);
            area.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            area.InnerPlotPosition = new ElementPosition(7, 8, 90, 80);
            chart1.BackColor = Color.White;
            area.BackColor = Color.White;
            chart1.BringToFront();

            if (res != null)
            {
                labelPemasukan.Text = "Rp " + res.pemasukan.ToString("N0", CultureInfo.InvariantCulture);
                labelPengeluaran.Text = "Rp " + res.pengeluaran.ToString("N0", CultureInfo.InvariantCulture);
                labelKamarTersedia.Text = res.kamar_tersedia.ToString();
            }
        }

        private void guna2Button3_Click(object sender, EventArgs e) { }
    }
}

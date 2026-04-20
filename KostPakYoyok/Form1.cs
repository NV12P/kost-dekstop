using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KostPakYoyok
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            ShowPage(new DashboardControl());
        }

        private void ShowPage(UserControl page)
        {
            panelContent.Controls.Clear();

            page.Dock = DockStyle.Fill;

            panelContent.Controls.Add(page);
        }

        private void panelContent_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnBeranda_Click(object sender, EventArgs e)
        {
            ShowPage(new DashboardControl());
        }

        private void btnPemasukan_Click(object sender, EventArgs e)
        {
            ShowPage(new PemasukanControl());
        }

        private void btnPengeluaran_Click(object sender, EventArgs e)
        {
            ShowPage(new PengeluaranControl());
        }

        private void btnKumar_Click(object sender, EventArgs e)
        {
            ShowPage(new PenyewaControl());
        }

        private void btnMore_Click(object sender, EventArgs e)
        {
            guna2ContextMenuStrip1.Show(btnMore, 0, btnMore.Height);
        }

        private void guna2ContextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            
        }

        private void keluarToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void guna2ContextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "Keluar")
            {
                Close();
            }
        }
    }
}

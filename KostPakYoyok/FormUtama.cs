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
    public partial class FormUtama : Form
    {
        // =====================================================
        // CONSTRUCTOR
        // =====================================================
        public FormUtama()
        {
            InitializeComponent();
        }

        // =====================================================
        // UI LOGIC
        // =====================================================
        private void ShowPage(UserControl page)
        {
            panelContent.Controls.Clear();
            page.Dock = DockStyle.Fill;
            panelContent.Controls.Add(page);
        }

        // =====================================================
        // PUBLIC HELPERS
        // =====================================================
        public void SetUserName(string nama)
        {
            if (string.IsNullOrEmpty(nama))
                return;

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => labelNama.Text = nama));
            }
            else
            {
                labelNama.Text = nama;
            }
        }

        // =====================================================
        // EVENT HANDLERS
        // =====================================================
        private void FormUtama_Load(object sender, EventArgs e)
        {
            ShowPage(new DashboardControl());
        }

        private void btnBeranda_Click(object sender, EventArgs e)
        {
            ShowPage(new DashboardControl());
        }

        private void btnPenyewa_Click(object sender, EventArgs e)
        {
            ShowPage(new PenyewaControl());
        }

        private void btnKeuangan_Click(object sender, EventArgs e)
        {
            ShowPage(new KeuanganControl());
        }

        private void btnRiwayat_Click(object sender, EventArgs e)
        {
            ShowPage(new RiwayatControl());
        }

        private void btnKumar_Click(object sender, EventArgs e)
        {
            ShowPage(new KamarControl());
        }

        private void btnMore_Click(object sender, EventArgs e)
        {
            guna2ContextMenuStrip1.Show(btnMore, 0, btnMore.Height);
        }

        private void guna2ContextMenuStrip1_ItemClicked_1(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "Keluar")
            {
                Close();
            }

            if (e.ClickedItem.Text == "Profil")
            {
                FormUtama parentForm = this.FindForm() as FormUtama;
                if (parentForm == null) return;

                Form formbackground = new Form();
                try
                {
                    using (FormProfil profileForm = new FormProfil())
                    {
                        formbackground.StartPosition = FormStartPosition.Manual;
                        formbackground.FormBorderStyle = FormBorderStyle.None;
                        formbackground.Opacity = 0.30d;
                        formbackground.BackColor = Color.Black;
                        formbackground.Size = parentForm.Size;
                        formbackground.Location = parentForm.Location;
                        formbackground.ShowInTaskbar = false;
                        formbackground.TopMost = false;

                        formbackground.Show();

                        profileForm.StartPosition = FormStartPosition.Manual;
                        profileForm.Owner = formbackground;

                        profileForm.Location = new Point(
                            parentForm.Location.X + 900, 
                            parentForm.Location.Y + 120
                        );

                        profileForm.ShowDialog();

                        formbackground.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan: " + ex.Message);
                }
                finally
                {
                    if (formbackground != null && !formbackground.IsDisposed)
                        formbackground.Dispose();
                }
            }
        }
    }
}

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
    public partial class FormAccount : Form
    {
        private LoginControl loginControl;
        private RegisterControl registerControl;

        public FormAccount()
        {
            InitializeComponent();
        }

        private void FormAccount_Load(object sender, EventArgs e)
        {
            // Inisialisasi controls SEKALI SAJA
            loginControl = new LoginControl();
            registerControl = new RegisterControl();

            // Setup event handlers
            loginControl.GoToRegisterClicked += LoginControl_GoToRegisterClicked;
            registerControl.GoToLoginClicked += RegisterControl_GoToLoginClicked;

            // Tampilkan login pertama
            ShowPage(loginControl); // JANGAN buat instance baru!
        }

        private void LoginControl_GoToRegisterClicked(object sender, EventArgs e)
        {
            ShowPage(registerControl); // Gunakan instance yang sudah ada
        }

        private void RegisterControl_GoToLoginClicked(object sender, EventArgs e)
        {
            ShowPage(loginControl); // Gunakan instance yang sudah ada
        }

        private void ShowPage(UserControl page)
        {
            panelKonten.Controls.Clear();
            page.Dock = DockStyle.Fill;
            panelKonten.Controls.Add(page);
        }
    }
}
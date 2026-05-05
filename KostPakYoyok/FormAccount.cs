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

        // =====================================================
        // CONSTRUCTOR
        // =====================================================
        public FormAccount()
        {
            InitializeComponent();
        }

        // =====================================================
        // UI LOGIC
        // =====================================================
        private void ShowPage(UserControl page)
        {
            panelKonten.Controls.Clear();
            page.Dock = DockStyle.Fill;
            panelKonten.Controls.Add(page);
        }

        // =====================================================
        // EVENT HANDLERS
        // =====================================================
        private void FormAccount_Load(object sender, EventArgs e)
        {
            loginControl = new LoginControl();
            registerControl = new RegisterControl();

            loginControl.GoToRegisterClicked += LoginControl_GoToRegisterClicked;
            registerControl.GoToLoginClicked += RegisterControl_GoToLoginClicked;

            ShowPage(loginControl);
        }

        private void LoginControl_GoToRegisterClicked(object sender, EventArgs e)
        {
            ShowPage(registerControl);
        }

        private void RegisterControl_GoToLoginClicked(object sender, EventArgs e)
        {
            ShowPage(loginControl);
        }
    }
}

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
    public partial class RegisterControl : UserControl
    {
        // Event untuk berpindah ke login
        public event EventHandler GoToLoginClicked;

        public RegisterControl()
        {
            InitializeComponent();
        }

        private void RegisterControl_Load(object sender, EventArgs e)
        {

        }

        private void linkLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Panggil event saat link login diklik
            GoToLoginClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}

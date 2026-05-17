using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace KostPakYoyok
{
    internal static class Program
    {
        // =====================================================
        // MAIN ENTRY POINT
        // =====================================================
        [STAThread]
        static void Main()
        {
            // =====================================================
            // SECURITY PROTOCOL CONFIGURATION
            // =====================================================
            ServicePointManager.Expect100Continue = true;
            // Aktifkan TLS 1.2 dan TLS 1.3 (3072) biar koneksi lancar mang!
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | (SecurityProtocolType)3072 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            Application.Run(new FormAccount());
        }
    }
}

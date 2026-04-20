using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KostPakYoyok
{

    public static class Session
    {
        public static string Token;
        // Simpan nama user untuk ditampilkan di UI setelah login
        public static string Nama;
    }

    class SewaAktif
    {
        public int bulan { get; set; }
        public int total { get; set; }
    }

    class DashboardResponse
    {
        public List<SewaAktif> sewa_aktif { get; set; }
        public int pemasukan { get; set; }
        public int pengeluaran { get; set; }
        public int kamar_tersedia { get; set; }
    }
}

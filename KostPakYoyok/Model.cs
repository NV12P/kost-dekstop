using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KostPakYoyok
{
    // =====================================================
    // SESSION MODEL
    // =====================================================
    public static class Session
    {
        public static string Token;
        public static string Nama;
    }

    // =====================================================
    // DATA MODELS
    // =====================================================
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

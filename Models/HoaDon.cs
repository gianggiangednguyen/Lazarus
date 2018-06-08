using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lazarus.Models
{
    public partial class HoaDon
    {
        public HoaDon()
        {
            ChiTietHoaDon = new HashSet<ChiTietHoaDon>();
            ThongTinGiaoHang = new HashSet<ThongTinGiaoHang>();
        }

        [StringLength(10)]
        public string HoaDonId { get; set; }
        [StringLength(10)]
        public string MaTaiKhoan { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal? TongTien { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? NgayLap { get; set; }
        [StringLength(200)]
        public string DiaChiGiao { get; set; }
        [StringLength(30)]
        public string TrangThai { get; set; }

        [InverseProperty("MaHoaDonNavigation")]
        public ICollection<ChiTietHoaDon> ChiTietHoaDon { get; set; }
        [InverseProperty("MaHoaDonNavigation")]
        public ICollection<ThongTinGiaoHang> ThongTinGiaoHang { get; set; }
    }
}

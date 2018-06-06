using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lazarus.Models
{
    public partial class ChiTietHoaDon
    {
        [StringLength(10)]
        public string MaHoaDon { get; set; }
        [StringLength(10)]
        public string MaSanPham { get; set; }
        public double SoLuong { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal DonGia { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal TongTien { get; set; }

        [ForeignKey("MaHoaDon")]
        [InverseProperty("ChiTietHoaDon")]
        public HoaDon MaHoaDonNavigation { get; set; }
        [ForeignKey("MaSanPham")]
        [InverseProperty("ChiTietHoaDon")]
        public SanPham MaSanPhamNavigation { get; set; }
    }
}

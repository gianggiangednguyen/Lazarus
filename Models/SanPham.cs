using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lazarus.Models
{
    public partial class SanPham
    {
        public SanPham()
        {
            ChiTietHoaDon = new HashSet<ChiTietHoaDon>();
            SanPhamCuaHang = new HashSet<SanPhamCuaHang>();
        }

        [StringLength(10)]
        public string SanPhamId { get; set; }
        [Required]
        [StringLength(200)]
        public string TenSanPham { get; set; }
        [Required]
        [StringLength(10)]
        public string MaLoaiSanPham { get; set; }
        [StringLength(10)]
        public string MaCuaHang { get; set; }
        [Required]
        public string MoTa { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal GiaBan { get; set; }
        public double? SoLuong { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? NgayThem { get; set; }
        [StringLength(30)]
        public string TrangThai { get; set; }

        [ForeignKey("MaLoaiSanPham")]
        [InverseProperty("SanPham")]
        public LoaiSanPham MaLoaiSanPhamNavigation { get; set; }
        [InverseProperty("MaSanPhamNavigation")]
        public ICollection<ChiTietHoaDon> ChiTietHoaDon { get; set; }
        [InverseProperty("MaSanPhamNavigation")]
        public ICollection<SanPhamCuaHang> SanPhamCuaHang { get; set; }
    }
}

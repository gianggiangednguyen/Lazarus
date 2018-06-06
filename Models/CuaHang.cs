using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lazarus.Models
{
    public partial class CuaHang
    {
        public CuaHang()
        {
            SanPhamCuaHang = new HashSet<SanPhamCuaHang>();
            TaiKhoan = new HashSet<TaiKhoan>();
        }

        [StringLength(10)]
        public string CuaHangId { get; set; }
        [Required]
        [StringLength(200)]
        public string TenCuaHang { get; set; }
        [Required]
        [StringLength(30)]
        public string TrangThai { get; set; }

        [InverseProperty("MaCuaHangNavigation")]
        public ICollection<SanPhamCuaHang> SanPhamCuaHang { get; set; }
        [InverseProperty("MaCuaHangNavigation")]
        public ICollection<TaiKhoan> TaiKhoan { get; set; }
    }
}

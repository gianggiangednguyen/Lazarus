using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lazarus.Models
{
    public partial class SanPhamCuaHang
    {
        [StringLength(10)]
        public string MaSanPham { get; set; }
        [StringLength(10)]
        public string MaCuaHang { get; set; }

        [ForeignKey("MaCuaHang")]
        [InverseProperty("SanPhamCuaHang")]
        public CuaHang MaCuaHangNavigation { get; set; }
        [ForeignKey("MaSanPham")]
        [InverseProperty("SanPhamCuaHang")]
        public SanPham MaSanPhamNavigation { get; set; }
    }
}

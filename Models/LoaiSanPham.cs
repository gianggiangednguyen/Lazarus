using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lazarus.Models
{
    public partial class LoaiSanPham
    {
        public LoaiSanPham()
        {
            SanPham = new HashSet<SanPham>();
        }

        [StringLength(10)]
        public string LoaiSanPhamId { get; set; }
        [Required]
        [StringLength(200)]
        public string TenLoaiSanPham { get; set; }
        [StringLength(30)]
        public string TrangThai { get; set; }

        [InverseProperty("MaLoaiSanPhamNavigation")]
        public ICollection<SanPham> SanPham { get; set; }
    }
}

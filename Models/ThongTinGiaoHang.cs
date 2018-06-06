using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lazarus.Models
{
    public partial class ThongTinGiaoHang
    {
        [StringLength(10)]
        public string ThongTinId { get; set; }
        [StringLength(10)]
        public string MaHoaDon { get; set; }
        [Required]
        [StringLength(200)]
        public string DiaChi { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal TongTien { get; set; }
        [Required]
        [StringLength(30)]
        public string TrangThai { get; set; }

        [ForeignKey("MaHoaDon")]
        [InverseProperty("ThongTinGiaoHang")]
        public HoaDon MaHoaDonNavigation { get; set; }
    }
}

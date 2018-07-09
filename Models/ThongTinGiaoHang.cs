using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lazarus.Models
{
    public partial class ThongTinGiaoHang
    {
        [Key]
        [StringLength(10)]
        public string ThongTinId { get; set; }
        [Required]
        [StringLength(10)]
        public string MaHoaDon { get; set; }
        [StringLength(200)]
        public string DiaChi { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal? PhiVanChuyen { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal? SoTienPhaiThu { get; set; }
        [StringLength(30)]
        public string TrangThai { get; set; }

        [ForeignKey("MaHoaDon")]
        [InverseProperty("ThongTinGiaoHang")]
        public HoaDon MaHoaDonNavigation { get; set; }
    }
}

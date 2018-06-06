using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lazarus.Models
{
    public partial class TaiKhoan
    {
        [StringLength(10)]
        public string TaiKhoanId { get; set; }
        [Required]
        [StringLength(200)]
        public string Email { get; set; }
        [Required]
        [StringLength(200)]
        public string MatKhau { get; set; }
        [StringLength(200)]
        public string HinhAnh { get; set; }
        [Required]
        [StringLength(100)]
        public string Ho { get; set; }
        [Required]
        [StringLength(100)]
        public string Ten { get; set; }
        [StringLength(10)]
        public string MaLoaiTaiKhoan { get; set; }
        [StringLength(10)]
        public string MaCuaHang { get; set; }
        [StringLength(10)]
        public string MaCongTy { get; set; }
        [Required]
        [StringLength(30)]
        public string TrangThai { get; set; }

        [ForeignKey("MaCongTy")]
        [InverseProperty("TaiKhoan")]
        public CongTy MaCongTyNavigation { get; set; }
        [ForeignKey("MaCuaHang")]
        [InverseProperty("TaiKhoan")]
        public CuaHang MaCuaHangNavigation { get; set; }
        [ForeignKey("MaLoaiTaiKhoan")]
        [InverseProperty("TaiKhoan")]
        public LoaiTaiKhoan MaLoaiTaiKhoanNavigation { get; set; }
        [InverseProperty("MaTaiKhoanNavigation")]
        public TaiKhoanPremium TaiKhoanPremium { get; set; }
    }
}

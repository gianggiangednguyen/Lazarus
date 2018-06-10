using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lazarus.CustomValidations;

namespace Lazarus.Models
{
    public partial class TaiKhoan
    {
        [StringLength(10)]
        public string TaiKhoanId { get; set; }
        [Required]
        [EmailCheckValidation]
        [StringLength(200)]
        public string Email { get; set; }
        [Required]
        [StringLength(200, ErrorMessage = "Mật khẩu có ít nhất là 8 kí tự, tối đa là 200", MinimumLength = 8)]
        [RegularExpression("^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{8,200}$", ErrorMessage = "Mật khẩu phải có ít nhất 1 kí tự chữ hoa và thường, và 1 kí tự số")]
        public string MatKhau { get; set; }
        [Required]
        [StringLength(200)]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu không trùng")]
        public string NhapLaiMatKhau { get; set; }
        [StringLength(200)]
        public string HinhAnh { get; set; }
        [Required(ErrorMessage = "Không thể trống")]
        [StringLength(100)]
        public string Ho { get; set; }
        [Required(ErrorMessage = "Không thể trống")]
        [StringLength(100)]
        public string Ten { get; set; }
        [StringLength(10)]
        public string MaLoaiTaiKhoan { get; set; }
        [StringLength(10)]
        public string MaCuaHang { get; set; }
        [StringLength(10)]
        public string MaCongTy { get; set; }
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

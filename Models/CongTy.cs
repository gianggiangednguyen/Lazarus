using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lazarus.Models
{
    public partial class CongTy
    {
        public CongTy()
        {
            TaiKhoan = new HashSet<TaiKhoan>();
        }

        [StringLength(10)]
        public string CongTyId { get; set; }
        [Required]
        [StringLength(200)]
        public string TenCongTy { get; set; }
        [Required]
        [StringLength(200)]
        public string DiaChi { get; set; }
        [StringLength(30)]
        public string TrangThai { get; set; }

        [InverseProperty("MaCongTyNavigation")]
        public ICollection<TaiKhoan> TaiKhoan { get; set; }
    }
}

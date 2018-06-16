using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lazarus.Models
{
    public partial class LoaiTaiKhoan
    {
        public LoaiTaiKhoan()
        {
            TaiKhoan = new HashSet<TaiKhoan>();
        }

        [StringLength(10)]
        public string LoaiTaiKhoanId { get; set; }
        [Required]
        public string TenLoaiTaiKhoan { get; set; }
        [StringLength(30)]
        public string TrangThai { get; set; }

        [InverseProperty("MaLoaiTaiKhoanNavigation")]
        public ICollection<TaiKhoan> TaiKhoan { get; set; }
    }
}

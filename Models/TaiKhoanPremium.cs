using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lazarus.Models
{
    public partial class TaiKhoanPremium
    {
        [Key]
        [StringLength(10)]
        public string MaTaiKhoan { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime NgayBatDau { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime NgayKetThuc { get; set; }

        [ForeignKey("MaTaiKhoan")]
        [InverseProperty("TaiKhoanPremium")]
        public TaiKhoan MaTaiKhoanNavigation { get; set; }
    }
}

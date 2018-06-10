using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Lazarus.Models;

namespace Lazarus.CustomValidations
{
    public class EmailCheckValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _context = (LazarusDbContext)validationContext.GetService(typeof(LazarusDbContext));

            var checkingVariable = _context.TaiKhoan.FirstOrDefault(i => i.Email == (string)value);

            if (checkingVariable == null)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Email đã tồn tại");
        }
    }
}

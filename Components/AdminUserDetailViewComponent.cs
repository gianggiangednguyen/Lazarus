using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Lazarus.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lazarus.Components
{
    public class AdminUserDetailViewComponent : ViewComponent
    {
        private readonly LazarusDbContext _context;

        public AdminUserDetailViewComponent(LazarusDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var tk = await _context.TaiKhoan.Include(p=>p.MaLoaiTaiKhoanNavigation).Where(t => t.TaiKhoanId == HttpContext.User.FindFirst(ClaimTypes.Sid).Value).FirstOrDefaultAsync();

            return View(tk);
        }
    }
}

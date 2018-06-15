using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Lazarus.Models;

namespace Lazarus.Controllers
{
    [Authorize(Policy = "ManagerPolicy")]
    public class ManagerController : Controller
    {
        private readonly LazarusDbContext _context;

        public ManagerController(LazarusDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var id = HttpContext.User.FindFirst(ClaimTypes.Sid).Value;

            var tk = await _context.TaiKhoan.Where(o => o.TaiKhoanId == id).Include(p => p.MaLoaiTaiKhoanNavigation).FirstOrDefaultAsync();

            return View(tk);
        }
    }
}
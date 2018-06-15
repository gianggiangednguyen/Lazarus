using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Lazarus.Models;
using Lazarus.Data;

namespace Lazarus.Controllers
{
    [Authorize(Policy = "AdminPolicy")]
    public class ManageAccountsController : Controller
    {
        private readonly LazarusDbContext _context;

        public ManageAccountsController(LazarusDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? page, string currentSort, string searchEmail, string searchName)
        {
            ViewData["CurrentSort"] = currentSort;
            ViewData["EmailSort"] = currentSort == "Email_Aces" ? "Email_Desc" : "Email_Aces";
            ViewData["NameSort"] = currentSort == "Name_Aces" ? "Name_Desc" : "Name_Aces";

            var lstTk = from taiKhoan in _context.TaiKhoan.Include(p => p.MaLoaiTaiKhoanNavigation)
                        select taiKhoan;

            if (!string.IsNullOrEmpty(searchEmail))
            {
                lstTk = (from tk in lstTk
                         where tk.Email.Contains(searchEmail)
                         select tk);

                if (!string.IsNullOrEmpty(searchName))
                {
                    lstTk = (from tk in lstTk
                             where tk.Email.Contains(searchName)
                             select tk)
                            .Union(
                        from tk in lstTk
                        where tk.HoTen.Contains(searchName)
                        select tk);
                }
            }

            switch (currentSort)
            {
                case "Email_Aces":
                    lstTk = lstTk.OrderBy(o => o.Email);
                    break;
                case "Email_Desc":
                    lstTk = lstTk.OrderByDescending(o => o.Email);
                    break;
                case "Name_Aces":
                    lstTk = lstTk.OrderBy(o => o.HoTen);
                    break;
                case "Name_Desc":
                    lstTk = lstTk.OrderByDescending(o => o.HoTen);
                    break;
                default:
                    break;
            }

            int size = 15;

            return View(await PagedList<TaiKhoan>.CreateAsync(lstTk.AsNoTracking(), page ?? 1, size));
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var tk = await _context.TaiKhoan.Include(t => t.MaLoaiTaiKhoanNavigation)
                    .Where(o => o.TaiKhoanId == id).FirstOrDefaultAsync();

            if (tk == null)
            {
                return NotFound();
            }

            return View(tk);
        }
    }
}
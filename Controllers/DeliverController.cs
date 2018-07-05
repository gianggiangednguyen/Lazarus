using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Lazarus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Lazarus.Data;

namespace Lazarus.Controllers
{
    [Authorize(Policy = "DeliverPolicy")]
    public class DeliverController : Controller
    {
        private readonly LazarusDbContext _context;

        public DeliverController(LazarusDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? page)
        {
            var items = (from tt in _context.ThongTinGiaoHang.Include(a => a.MaHoaDonNavigation)
                         where tt.TrangThai == "Đang chờ giao"
                         orderby tt.MaHoaDonNavigation.NgayLap
                         select tt).Union(
                        from tt in _context.ThongTinGiaoHang.Include(a => a.MaHoaDonNavigation)
                        where tt.TrangThai == "Đã giao"
                        orderby tt.MaHoaDonNavigation.NgayLap
                        select tt);

            return View(await PagedList<ThongTinGiaoHang>.CreateAsync(items, page ?? 1, 15));
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var item = await _context.ThongTinGiaoHang.Where(a => a.ThongTinId == id).SingleOrDefaultAsync();

            if(item == null)
            {
                return NotFound();
            }

            item.TrangThai = "Đã giao";
            _context.Update(item);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
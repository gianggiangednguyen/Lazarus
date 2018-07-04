using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Lazarus.Data;
using Lazarus.Models;

namespace Lazarus.Controllers
{
    [Authorize(Policy = "ShopManagerPolicy")]
    public class OrderManageController : Controller
    {
        private readonly LazarusDbContext _context;

        public OrderManageController(LazarusDbContext context)
        {
            _context = context;
        }

        public string ShopId
        {
            get
            {
                var userid = HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
                var shopid = _context.TaiKhoan.SingleOrDefault(a => a.TaiKhoanId == userid).MaCuaHang;
                return shopid;
            }
        }

        public async Task<IActionResult> Index(int? page, string currentSort, string searchString, string status)
        {
            if (string.IsNullOrEmpty(ShopId))
            {
                return RedirectToAction("ShopCreate", "ShopManage");
            }

            var list = from ord in _context.HoaDon.Include(a => a.ChiTietHoaDon).ThenInclude(b => b.MaSanPhamNavigation)
                       where ord.ChiTietHoaDon.Any(a => a.MaSanPhamNavigation.MaCuaHang == ShopId)
                       select ord;

            ViewBag.CurrentSort = currentSort;
            ViewBag.SortById = currentSort == "ById" ? "ById_desc" : "ById";
            ViewBag.SortByDate = currentSort == "ByDate" ? "ByDate_desc" : "ByDate";
            ViewBag.SortByStatus = currentSort == "ByStatus" ? "ByStatus_desc" : "ByStatus";

            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.SearchString = searchString;
                list = list.Where(a => a.HoaDonId.ToLower().Contains(searchString.ToLower()));
            }

            if (!string.IsNullOrEmpty(status))
            {
                ViewBag.Status = status;
                list = list.Where(a => a.TrangThai.ToLower().Contains(status.ToLower()));
            }

            switch (currentSort)
            {
                case "ById":
                    list = list.OrderBy(a => a.HoaDonId);
                    break;
                case "ById_desc":
                    list = list.OrderByDescending(a => a.HoaDonId);
                    break;
                case "ByDate":
                    list = list.OrderBy(a => a.NgayLap);
                    break;
                case "ByDate_desc":
                    list = list.OrderByDescending(a => a.NgayLap);
                    break;
                case "ByStatus":
                    list = list.OrderBy(a => a.TrangThai);
                    break;
                case "ByStatus_desc":
                    list = list.OrderByDescending(a => a.TrangThai);
                    break;
                default:
                    break;
            }

            return View(await PagedList<HoaDon>.CreateAsync(list, page ?? 1, 15));
        }

        public async Task<IActionResult> Confirm(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            ViewBag.ShopId = ShopId;

            var item = (from ord in _context.HoaDon.Include(a => a.ChiTietHoaDon).ThenInclude(b => b.MaSanPhamNavigation)
                        where ord.HoaDonId == id
                        select ord);

            return View(await item.SingleOrDefaultAsync());
        }

        [HttpPost]
        [ActionName("Confirm")]
        public async Task<IActionResult> ConfirmPost(string id, string spid)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var item = await (from cthd in _context.ChiTietHoaDon.Where(a => a.MaHoaDon == id)
                                                    .Include(a => a.MaHoaDonNavigation)
                                                    .Include(a => a.MaSanPhamNavigation)
                              where cthd.MaHoaDon == id && cthd.MaSanPham == spid
                              select cthd).SingleOrDefaultAsync();

            if (item.TrangThai != "Đang chờ giao" && item.TrangThai != "Đã xóa")
            {
                item.TrangThai = "Đang chờ giao";
            }

            await TryUpdateModelAsync(item);

            var items = await (from hd in _context.HoaDon.Include(a => a.ChiTietHoaDon).ThenInclude(b => b.MaSanPhamNavigation)
                               where hd.HoaDonId == id
                               select hd).SingleOrDefaultAsync();

            if (items.ChiTietHoaDon.All(a => a.TrangThai == "Đang chờ giao"))
            {
                items.TrangThai = "Đang chờ giao";
                if(!_context.ThongTinGiaoHang.Any(a=>a.MaHoaDon == items.HoaDonId))
                {
                    var thongtingh = new ThongTinGiaoHang
                    {
                        MaHoaDon = items.HoaDonId,
                        DiaChi = items.DiaChiGiao,
                        ThongTinId = RandomString.GenerateRandomString(_context.ThongTinGiaoHang.Select(a => a.ThongTinId)),
                        TongTien = items.TongTien,
                        TrangThai = items.TrangThai
                    };
                    await _context.AddAsync(thongtingh);
                }
                await TryUpdateModelAsync(items);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Confirm", new { id });
        }
    }
}
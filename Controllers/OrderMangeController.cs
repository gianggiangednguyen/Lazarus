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
    public class OrderMangeController : Controller
    {
        private readonly LazarusDbContext _context;

        public OrderMangeController(LazarusDbContext context)
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

        public async Task<IActionResult> Index(int? page, string currentSort, string searchString)
        {
            if (string.IsNullOrEmpty(ShopId))
            {
                return RedirectToAction("ShopCreate", "ShopManage");
            }

            var list = from ord in _context.HoaDon
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

        public async Task<IActionResult> Confirm()
        {
            return View();
        }
    }
}
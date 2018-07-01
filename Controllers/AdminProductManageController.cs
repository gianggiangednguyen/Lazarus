using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Lazarus.Models;
using Lazarus.Data;

namespace Lazarus.Controllers
{
    [Authorize(Policy = "AdminPolicy")]
    public class AdminProductManageController : Controller
    {
        private readonly LazarusDbContext _context;

        public AdminProductManageController(LazarusDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? page, string currentSort, string searchString, string filterByType, string filterByShop, string filterByStatus)
        {
            ViewBag.LoaiSanPham = LoaiSanPhamList();
            ViewBag.CuaHang = CuaHangList();

            var list = from sp in _context.SanPham.Include(a => a.MaLoaiSanPhamNavigation).Include(a => a.SanPhamCuaHang).ThenInclude(b => b.MaCuaHangNavigation)
                       select sp;
            var spid = "01rBnqj5mN";
            var test = _context.SanPhamCuaHang.SingleOrDefault(a => a.MaSanPham == spid).MaCuaHangNavigation.TenCuaHang;

            ViewBag.CurrentSort = currentSort;
            ViewBag.SortByName = currentSort == "ByName" ? "ByName_desc" : "ByName";
            ViewBag.SortByType = currentSort == "ByType" ? "ByType_desc" : "ByType";
            ViewBag.SortByShop = currentSort == "ByShop" ? "ByShop_desc" : "ByShop";
            ViewBag.SortByStatus = currentSort == "ByStatus" ? "ByStatus_desc" : "ByStatus";

            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.SearchString = searchString;
                list = list.Where(a => a.TenSanPham.ToLower().Contains(searchString.ToLower()));
            }

            if (!string.IsNullOrEmpty(filterByType))
            {
                ViewBag.FilterByType = filterByType;
                list = list.Where(a => a.MaLoaiSanPham == filterByType);
            }

            if (!string.IsNullOrEmpty(filterByShop))
            {
                ViewBag.FilterByType = filterByType;
                //list = list.Where(a => a.SanPhamCuaHang.Any(b => b.MaCuaHang == filterByShop));
                list = from items in list
                       where items.SanPhamCuaHang.Any(a => a.MaCuaHang == filterByShop)
                       select items;
            }
            
            if(!string.IsNullOrEmpty(filterByStatus))
            {
                ViewBag.FilterByStatus = filterByStatus;
                list = list.Where(a => a.TrangThai == filterByStatus);
            }

            switch (currentSort)
            {
                case "ByName":
                    list = list.OrderBy(a => a.TenSanPham);
                    break;
                case "ByName_desc":
                    list = list.OrderByDescending(a => a.TenSanPham);
                    break;
                case "ByType":
                    list = list.OrderBy(a => a.MaLoaiSanPham);
                    break;
                case "ByType_desc":
                    list = list.OrderByDescending(a => a.MaLoaiSanPham);
                    break;
                case "ByShop":
                    list = from items in list
                           let shop = items.SanPhamCuaHang
                           orderby shop.Select(a => a.MaCuaHangNavigation.TenCuaHang)
                           select items;
                    break;
                case "ByShop_desc":
                    list = from items in list
                           let shop = items.SanPhamCuaHang
                           orderby shop.Select(a => a.MaCuaHangNavigation.TenCuaHang) descending
                           select items;
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

            return View(await PagedList<SanPham>.CreateAsync(list, page ?? 1, 15));
        }

        public List<SelectListItem> LoaiSanPhamList()
        {
            var list = new List<SelectListItem>();
            var items = _context.LoaiSanPham.ToList();

            foreach (var item in items)
            {
                list.Add(new SelectListItem(item.TenLoaiSanPham, item.LoaiSanPhamId));
            }

            return list;
        }

        public List<SelectListItem> CuaHangList()
        {
            var list = new List<SelectListItem>();
            var items = _context.CuaHang.ToList();

            foreach (var item in items)
            {
                list.Add(new SelectListItem(item.TenCuaHang, item.CuaHangId));
            }

            return list;
        }
    }
}
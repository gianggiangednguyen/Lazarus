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
using Microsoft.AspNetCore.Http;
using System.IO;

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

            var list = from sp in _context.SanPham.Include(a => a.MaLoaiSanPhamNavigation).Include(b => b.MaCuaHangNavigation)
                       select sp;

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
                list = list.Where(a => a.MaCuaHang == filterByShop);
            }

            if (!string.IsNullOrEmpty(filterByStatus))
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
                    list = list.OrderBy(a => a.MaCuaHangNavigation.TenCuaHang);
                    break;
                case "ByShop_desc":
                    list = list.OrderByDescending(a => a.MaCuaHangNavigation.TenCuaHang);
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

        public async Task<IActionResult> Edit(string id)
        {
            ViewBag.LoaiSanPham = LoaiSanPhamList();
            ViewBag.CuaHang = CuaHangList();

            var sp = await _context.SanPham.Where(a => a.SanPhamId == id).SingleOrDefaultAsync();

            return View(sp);
        }

        [HttpPost]
        [ActionName("Edit")]
        public async Task<IActionResult> EditPost(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sp = await _context.SanPham.Where(a => a.SanPhamId == id).SingleOrDefaultAsync();

            if (HttpContext.Request.Form.Files.Count > 0)
            {
                IFormFile img = HttpContext.Request.Form.Files.First();
                var ext = Path.GetExtension(img.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/ProductImages/", $"{sp.SanPhamId}{ext}");
                sp.HinhAnh = $"{sp.SanPhamId}{ext}";
                using (var steam = new FileStream(path, FileMode.Create))
                {
                    await img.CopyToAsync(steam);
                }
            }

            if (sp == null)
            {
                return NotFound();
            }

            await TryUpdateModelAsync(sp,
                                    "",
                                    a => a.TenSanPham, a => a.GiaBan, a => a.MoTa, a => a.SoLuong, a => a.MaLoaiSanPham, a => a.MaCuaHang);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var sp = await _context.SanPham.Where(a => a.SanPhamId == id).SingleOrDefaultAsync();
            sp.TrangThai = "Đã xóa";
            _context.SanPham.Update(sp);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
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
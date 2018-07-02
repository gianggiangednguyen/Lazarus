using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Lazarus.Models;
using Lazarus.Data;

namespace Lazarus.Controllers
{
    public class AdminShopManageController : Controller
    {
        private readonly LazarusDbContext _context;

        public AdminShopManageController(LazarusDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? page, string currentSort, string searchString, string filterByStatus)
        {
            var shops = from ch in _context.CuaHang.Include(a => a.TaiKhoan)
                        select ch;

            ViewBag.CurrentSort = currentSort;
            ViewBag.SortByName = currentSort == "ByName" ? "ByName_desc" : "ByName";
            ViewBag.SortByStatus = currentSort == "ByStatus" ? "ByStatus_desc" : "ByStatus";

            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.SearchString = searchString;
                shops = shops.Where(a => a.TenCuaHang.ToLower().Contains(searchString.ToLower()));
            }

            if (!string.IsNullOrEmpty(filterByStatus))
            {
                ViewBag.FilterByStatus = filterByStatus;
                shops = shops.Where(a => a.TrangThai == filterByStatus);
            }

            switch (currentSort)
            {
                case "ByName":
                    shops = shops.OrderBy(a => a.TenCuaHang);
                    break;
                case "ByName_desc":
                    shops = shops.OrderByDescending(a => a.TenCuaHang);
                    break;
                case "ByStatus":
                    shops = shops.OrderBy(a => a.TrangThai);
                    break;
                case "ByStatus_desc":
                    shops = shops.OrderByDescending(a => a.TrangThai);
                    break;
                default:
                    break;
            }

            return View(await PagedList<CuaHang>.CreateAsync(shops, page ?? 1, 15));
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var shop = await _context.CuaHang.SingleOrDefaultAsync(a => a.CuaHangId == id);

            if (shop == null)
            {
                return NotFound();
            }

            return View(shop);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var shop = await _context.CuaHang.SingleOrDefaultAsync(a => a.CuaHangId == id);

            if (shop == null)
            {
                return NotFound();
            }

            return View(shop);
        }

        [HttpPost]
        [ActionName("Edit")]
        public async Task<IActionResult> EditPost(string id)
        {
            var shopToUpdate = await _context.CuaHang.SingleOrDefaultAsync(a => a.CuaHangId == id);

            await TryUpdateModelAsync(shopToUpdate, "",
                                        a => a.TenCuaHang, a => a.TrangThai);
            await _context.SaveChangesAsync();

            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CuaHang model)
        {
            model.CuaHangId = RandomString.GenerateRandomString(_context.CuaHang.Select(a => a.CuaHangId));

            if (ModelState.IsValid)
            {
                await _context.CuaHang.AddAsync(model);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var shop = await _context.CuaHang.SingleOrDefaultAsync(a => a.CuaHangId == id);

            if (shop == null)
            {
                return NotFound();
            }

            shop.TrangThai = "Đã xóa";
            _context.CuaHang.Update(shop);
            await _context.SaveChangesAsync();

            return View();
        }
    }
}
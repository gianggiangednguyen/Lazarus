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
    [Authorize(Policy = "AdminPolicy")]
    public class AccountTypeController : Controller
    {
        private readonly LazarusDbContext _context;

        public AccountTypeController(LazarusDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? page, string currentSort, string searchString)
        {
            var lstLoaiTk = from tk in _context.LoaiTaiKhoan
                            select tk;

            ViewBag.CurrentSort = currentSort;
            ViewBag.SortByName = currentSort == "ByName" ? "ByName_desc" : "ByName";
            ViewBag.SortByStatus = currentSort == "ByStatus" ? "ByStatus_desc" : "ByStatus";

            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.SearchString = searchString;
                lstLoaiTk = lstLoaiTk.Where(o => o.TenLoaiTaiKhoan.Contains(searchString));
            }

            switch (currentSort)
            {
                case "ByName":
                    lstLoaiTk = lstLoaiTk.OrderBy(k => k.TenLoaiTaiKhoan);
                    break;
                case "ByName_desc":
                    lstLoaiTk = lstLoaiTk.OrderByDescending(k => k.TenLoaiTaiKhoan);
                    break;
                case "ByStatus":
                    lstLoaiTk = lstLoaiTk.OrderBy(k => k.TrangThai);
                    break;
                case "ByStatus_desc":
                    lstLoaiTk = lstLoaiTk.OrderByDescending(k => k.TrangThai);
                    break;
                default:
                    break;
            }

            return View(await PagedList<LoaiTaiKhoan>.CreateAsync(lstLoaiTk.AsNoTracking(), page ?? 1, 15));
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var loaitk = await _context.LoaiTaiKhoan.SingleOrDefaultAsync(o => o.LoaiTaiKhoanId == id);

            if (loaitk == null)
            {
                return NotFound();
            }

            return View(loaitk);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var loaitk = await _context.LoaiTaiKhoan.SingleOrDefaultAsync(o => o.LoaiTaiKhoanId == id);

            if (loaitk == null)
            {
                return NotFound();
            }

            return View(loaitk);
        }

        [ActionName("Edit")]
        [HttpPost]
        public async Task<IActionResult> EditPost(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var loaitkToUpdate = await _context.LoaiTaiKhoan.SingleOrDefaultAsync(o => o.LoaiTaiKhoanId == id);

            await TryUpdateModelAsync(loaitkToUpdate,
                "",
                p => p.TenLoaiTaiKhoan, p => p.TrangThai);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            return View();
        }

        [ActionName("Create")]
        [HttpPost]
        public async Task<IActionResult> CreatePost(LoaiTaiKhoan model)
        {
            model.LoaiTaiKhoanId = RandomString.GenerateRandomString(_context.LoaiTaiKhoan.Select(o => o.LoaiTaiKhoanId));
            await _context.AddAsync(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaitk = await _context.LoaiTaiKhoan.SingleOrDefaultAsync(a => a.LoaiTaiKhoanId == id);

            if (loaitk == null)
            {
                return NotFound();
            }
            else
            {
                if(loaitk.TrangThai != "System")
                {
                    loaitk.TrangThai = "Đã xóa";
                    _context.LoaiTaiKhoan.Update(loaitk);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index");
        }
    }
}
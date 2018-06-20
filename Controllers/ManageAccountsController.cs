using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Lazarus.Models;
using Lazarus.Data;
using System.Linq.Expressions;
using System.Text;

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
            ViewBag.CurrentSort = currentSort;
            ViewBag.EmailSort = currentSort == "ByEmail" ? "ByEmail_desc" : "ByEmail";
            ViewBag.NameSort = currentSort == "ByName" ? "ByName_desc" : "ByName";
            ViewBag.TypeSort = currentSort == "ByType" ? "ByType_desc" : "ByType";
            ViewBag.StatusSort = currentSort == "ByStatus" ? "ByStatus_desc" : "ByStatus";

            var lstTk = from taiKhoan in _context.TaiKhoan.Include(p => p.MaLoaiTaiKhoanNavigation)
                        select taiKhoan;

            if (!string.IsNullOrEmpty(searchEmail))
            {
                ViewBag.SearchEmail = searchEmail;
                lstTk = lstTk.Where(a => a.Email.Contains(searchEmail));
            }

            if (!string.IsNullOrEmpty(searchName))
            {
                ViewBag.SearchName = searchName;
                lstTk = lstTk.Where(a => a.HoTen.Contains(searchName));
            }

            switch (currentSort)
            {
                case "ByEmail":
                    lstTk = lstTk.OrderBy(o => o.Email);
                    break;
                case "ByEmail_desc":
                    lstTk = lstTk.OrderByDescending(o => o.Email);
                    break;
                case "ByName":
                    lstTk = lstTk.OrderBy(o => o.HoTen);
                    break;
                case "ByName_desc":
                    lstTk = lstTk.OrderByDescending(o => o.HoTen);
                    break;
                case "ByType":
                    lstTk = lstTk.OrderBy(o => o.MaLoaiTaiKhoanNavigation.TenLoaiTaiKhoan);
                    break;
                case "ByType_desc":
                    lstTk = lstTk.OrderByDescending(o => o.MaLoaiTaiKhoanNavigation.TenLoaiTaiKhoan);
                    break;
                case "ByStatus":
                    lstTk = lstTk.OrderBy(o => o.TrangThai);
                    break;
                case "ByStatus_desc":
                    lstTk = lstTk.OrderByDescending(o => o.TrangThai);
                    break;
                default:
                    break;
            }

            return View(await PagedList<TaiKhoan>.CreateAsync(lstTk.AsNoTracking(), page ?? 1, 15));
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

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tk = await _context.TaiKhoan.Where(o => o.TaiKhoanId == id).FirstOrDefaultAsync();

            if (tk == null)
            {
                return NotFound();
            }

            ViewBag.LoaiTaiKhoanList = await LoaiTaiKhoanList();

            return View(tk);
        }

        [HttpPost]
        [ActionName("Edit")]
        public async Task<IActionResult> EditPost(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var tkToUpdate = await _context.TaiKhoan.SingleOrDefaultAsync(o => o.TaiKhoanId == id);

            await TryUpdateModelAsync<TaiKhoan>(tkToUpdate,
                "",
                p => p.Ho, p => p.Ten, p => p.TrangThai, p => p.MaLoaiTaiKhoan);
            await _context.SaveChangesAsync();

            return View("Details", tkToUpdate);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.LoaiTaiKhoanList = await LoaiTaiKhoanList();

            return View();
        }

        [HttpPost]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(TaiKhoan model)
        {
            ModelState.Remove("NhapLaiMatKhau");
            ModelState.Remove("MatKhau");
            if (ModelState.IsValid)
            {
                model.TaiKhoanId = RandomString.GenerateRandomString(_context.TaiKhoan.Select(o => o.TaiKhoanId));

                await _context.TaiKhoan.AddAsync(model);
                await _context.SaveChangesAsync();

                return View(model);
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("Lỗi xảy ra");

            ViewBag.LoaiTaiKhoanList = await LoaiTaiKhoanList();
            ViewBag.InsertError = sb.ToString();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id, int? page)
        {
            var tkToDelete = await _context.TaiKhoan.SingleOrDefaultAsync(t => t.TaiKhoanId == id);
            tkToDelete.TrangThai = "Deleted";
            _context.TaiKhoan.Update(tkToDelete);
            await _context.SaveChangesAsync();

            var lstTk = _context.TaiKhoan.Include(p => p.MaLoaiTaiKhoanNavigation);

            return View("Index", await PagedList<TaiKhoan>.CreateAsync(lstTk.AsNoTracking(), page ?? 1, 15));
        }

        public async Task<List<SelectListItem>> LoaiTaiKhoanList()
        {
            var list = new List<SelectListItem>();

            var items = await(from loaitk in _context.LoaiTaiKhoan
                              select loaitk).ToListAsync();

            foreach (var item in items)
            {
                list.Add(new SelectListItem(item.TenLoaiTaiKhoan, item.LoaiTaiKhoanId));
            }

            return list;
        }
    }
}
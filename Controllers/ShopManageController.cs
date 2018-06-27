using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Lazarus.Models;
using Lazarus.Data;

namespace Lazarus.Controllers
{
    [Authorize(Policy = "ShopManagerPolicy")]
    public class ShopManageController : Controller
    {
        private readonly LazarusDbContext _context;

        public ShopManageController(LazarusDbContext context)
        {
            _context = context;
        }

        public string ShopId
        {
            get
            {
                var userid = HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
                var shopid = _context.TaiKhoan.Where(a => a.TaiKhoanId == userid).Select(a => a.MaCuaHang).SingleOrDefault();
                return shopid;
            }
        }
        //TODO: search stirng review
        public async Task<IActionResult> Index(int? page, string searchString)
        {
            if (string.IsNullOrEmpty(ShopId))
            {
                ViewBag.Mes = "Click vào đây để tạo ra một shop mới!";
                return View();
            }

            var shop = await _context.CuaHang.Where(a => a.CuaHangId == ShopId).FirstOrDefaultAsync();
            var products = _context.SanPham.Include(a => a.MaLoaiSanPhamNavigation).Where(a => a.MaCuaHang == ShopId);

            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.SearchString = searchString;
                products = products.Where(a => a.TenSanPham.Contains(searchString));
            }

            ViewBag.ShopId = ShopId;

            if (shop.TrangThai == "Ngưng họa động")
            {
                ViewBag.ShopStatus = "Ngưng họa động";
            }


            return View(await PagedList<SanPham>.CreateAsync(products, page ?? 1, 10));
        }

        public IActionResult ShopCreate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ShopCreate(CuaHang model)
        {
            model.CuaHangId = RandomString.GenerateRandomString(_context.CuaHang.Select(o => o.CuaHangId));
            model.TrangThai = "Hoạt động";
            var userid = HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
            var user = await _context.TaiKhoan.Where(o => o.TaiKhoanId == userid).SingleOrDefaultAsync();
            user.MaCuaHang = model.CuaHangId;

            await _context.AddAsync(model);
            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ShopEdit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ch = await _context.CuaHang.Where(a => a.CuaHangId == id).SingleOrDefaultAsync();

            if (ch == null)
            {
                return NotFound();
            }

            return View(ch);
        }

        [HttpPost]
        [ActionName("ShopEdit")]
        public async Task<IActionResult> ShopEditPost(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chToUpdate = await _context.CuaHang.Where(a => a.CuaHangId == id).SingleOrDefaultAsync();

            if (chToUpdate == null)
            {
                return NotFound();
            }

            await TryUpdateModelAsync(chToUpdate,
                                    "",
                                    a => a.TenCuaHang);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ShopStatusChange(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var ch = await _context.CuaHang.Where(a => a.CuaHangId == id).SingleOrDefaultAsync();
            if (ch.TrangThai == "Ngưng hoạt động")
            {
                ch.TrangThai = "Hoạt động";
            }
            else
            {
                ch.TrangThai = "Ngưng họa động";
            }
            _context.CuaHang.Update(ch);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ProductCreate()
        {
            ViewBag.ItemList = await LoaiSanPham();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProductCreate(SanPham model)
        {
            model.SanPhamId = RandomString.GenerateRandomString(_context.SanPham.Select(a => a.SanPhamId));
            model.MaCuaHang = ShopId;
            model.NgayThem = DateTime.Now;
            model.TrangThai = "Hoạt động";
            if (HttpContext.Request.Form.Files.Count > 0)
            {
                IFormFile img = HttpContext.Request.Form.Files.First();

                if (!string.IsNullOrEmpty(img.FileName))
                {
                    var ext = Path.GetExtension(img.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/ProductImages/", $"{model.SanPhamId}{ext}");
                    model.HinhAnh = $"{model.SanPhamId}{ext}";

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await img.CopyToAsync(stream);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                await _context.AddAsync(model);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditProduct(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sp = await _context.SanPham.Where(a => a.SanPhamId == id).SingleOrDefaultAsync();

            if (sp == null)
            {
                return NotFound();
            }

            ViewBag.ItemList = await LoaiSanPham();
            return View(sp);
        }

        [HttpPost]
        [ActionName("EditProduct")]
        public async Task<IActionResult> EditProductPost(string id)
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
                                    a => a.TenSanPham, a => a.GiaBan, a => a.MoTa, a => a.SoLuong, a => a.MaLoaiSanPham);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var sp = await _context.SanPham.Where(a => a.SanPhamId == id).SingleOrDefaultAsync();
            sp.TrangThai = "Đã xóa";
            _context.SanPham.Update(sp);
            await _context.SaveChangesAsync();

            return View("Index", await PagedList<SanPham>.CreateAsync(_context.SanPham.Where(a => a.MaCuaHang == ShopId), 1, 10));
        }

        public async Task<List<SelectListItem>> LoaiSanPham()
        {
            var list = new List<SelectListItem>();

            var exceptitem = _context.LoaiSanPham.Where(a => a.TrangThai == "Đã xóa");
            var items = await _context.LoaiSanPham.Except(exceptitem).ToListAsync();

            foreach (var item in items)
            {
                list.Add(new SelectListItem(item.TenLoaiSanPham, item.LoaiSanPhamId));
            }

            return list;
        }
    }
}
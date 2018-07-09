using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Lazarus.Models;
using Lazarus.Data;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Lazarus.Controllers
{
    public class HomeController : Controller
    {
        public readonly LazarusDbContext _context;

        public HomeController(LazarusDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                var value = HttpContext.User.FindFirst(ClaimTypes.Role).Value;
            }
            return View();
        }


        [Authorize(Policy = "NormalUserPolicy")]
        public IActionResult Subscription()
        {
            return View();
        }

        [Authorize(Policy = "NormalUserPolicy")]
        [HttpPost]
        public IActionResult Subscription(string value)
        {
            return RedirectToAction("ConfirmSubscription", "Home", new { id = HttpContext.User.FindFirst(ClaimTypes.Sid).Value, value });
        }

        [Authorize(Policy = "NormalUserPolicy")]
        public IActionResult ConfirmSubscription()
        {
            string str = Request.Query["value"].ToString();
            string val;
            switch (str)
            {
                case "100":
                    val = "100.000 VND";
                    break;
                case "500":
                    val = "500.000 VND";
                    break;
                case "1000":
                    val = "1.000.000 VND";
                    break;
                default:
                    val = "?????";
                    break;
            }
            ViewData["price"] = val;

            return View();
        }

        [Authorize(Policy = "NormalUserPolicy")]
        [HttpPost]
        public async Task<IActionResult> ConfirmSubscription(string id, string value)
        {
            var tk = await _context.TaiKhoan.Where(p => p.TaiKhoanId == id).SingleOrDefaultAsync();

            if (tk != null)
            {
                if (tk.NgayGiaHan > DateTime.Now)
                {
                    ViewData["view"] = "Thời gian Premium còn thời hạn.";

                    return View();
                }
                else
                {
                    if (value == "100")
                    {
                        tk.NgayGiaHan = DateTime.Now.AddMonths(1);
                    }
                    else if (value == "500")
                    {
                        tk.NgayGiaHan = DateTime.Now.AddMonths(6);
                    }
                    else if (value == "1000")
                    {
                        tk.NgayGiaHan = DateTime.Now.AddYears(1);
                    }

                    tk.MaLoaiTaiKhoan = "SM";
                    _context.TaiKhoan.Update(tk);
                    await _context.SaveChangesAsync();
                }

                ViewData["view"] = "Thành công!";

                return View();
            }

            ViewData["view"] = "Lỗi xảy ra!";

            return View();
        }

        public async Task<IActionResult> AccountDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var tk = await _context.TaiKhoan.Where(m => m.TaiKhoanId == id).FirstOrDefaultAsync();

            if (tk != null)
            {
                return View(tk);
            }

            return View();
        }

        public async Task<IActionResult> AccountEdit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                NotFound();
            }

            var tk = await _context.TaiKhoan.Where(m => m.TaiKhoanId == id).FirstOrDefaultAsync();

            if (tk != null)
            {
                return View(tk);
            }

            return View();
        }

        [ActionName("AccountEdit")]
        [HttpPost]
        public async Task<IActionResult> AccountEditPost(string id)
        {
            var modelToUpdate = await _context.TaiKhoan.Where(o => o.TaiKhoanId == id).FirstOrDefaultAsync();
            if (HttpContext.Request.Form.Files.Count > 0)
            {
                IFormFile file = HttpContext.Request.Form.Files.First();

                if (!string.IsNullOrEmpty(file.FileName))
                {
                    var ext = Path.GetExtension(file.FileName);

                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/AccountImages/", $"{modelToUpdate.TaiKhoanId}{ext}");

                    modelToUpdate.HinhAnh = $"{modelToUpdate.TaiKhoanId}{ext}";

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
            }
            //TryUpdateModel se check modelstate
            await TryUpdateModelAsync(modelToUpdate, "", a => a.Ten, a => a.Ho);
            await _context.SaveChangesAsync();

            return View(modelToUpdate);
        }

        public async Task<IActionResult> ProductDetail(string id)
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

            return View(sp);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(string id, double? qty)
        {
            List<ChiTietHoaDon> list = HttpContext.Session.GetSessionObject<List<ChiTietHoaDon>>("Cart");
            if (list == null)
            {
                list = new List<ChiTietHoaDon>();
            }

            if (list.Any(a => a.MaSanPham == id))
            {
                var sp = list.Find(a => a.MaSanPham == id);
                //var spnav = await _context.SanPham.SingleOrDefaultAsync(a => a.SanPhamId == id);
                //list.Remove(sp);
                sp.SoLuong += qty ?? 0;
                sp.TongTien = sp.DonGia * (decimal?)qty ?? 0;
                //list.Add(new ChiTietHoaDon { MaSanPham = sp.MaSanPham, DonGia = sp.DonGia, SoLuong = qty ?? 0, TongTien = sp.TongTien, TrangThai = "Đang chờ", MaSanPhamNavigation = spnav });
            }
            else
            {
                var sp = await _context.SanPham.Where(a => a.SanPhamId == id).SingleOrDefaultAsync();
                list.Add(new ChiTietHoaDon { MaSanPham = sp.SanPhamId, DonGia = sp.GiaBan, SoLuong = qty ?? 0, TongTien = (sp.GiaBan * (decimal?)qty ?? 0), TrangThai = "Đang chờ", MaSanPhamNavigation = sp });
            }

            HttpContext.Session.SetSessionObject("Cart", list);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> RemoveFromCart(string id)
        {
            var list = HttpContext.Session.GetSessionObject<List<ChiTietHoaDon>>("Cart");

            if (list != null)
            {
                var item = list.Find(a => a.MaSanPham == id);
                await Task.FromResult<bool>(list.Remove(item));

                HttpContext.Session.SetSessionObject("Cart", list);
            }

            return RedirectToAction("Index");
        }

        [Authorize(Policy = "NormalUserPolicy")]
        public IActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Checkout")]
        [Authorize(Policy = "NormalUserPolicy")]
        public async Task<IActionResult> CheckoutPost()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var list = HttpContext.Session.GetSessionObject<List<ChiTietHoaDon>>("Cart");
            decimal? tt = 0;
            if (list == null)
            {
                return RedirectToAction("Index");
            }
            foreach (var item in list)
            {
                tt += item.TongTien ?? 0;
            }

            var hoadon = new HoaDon
            {
                HoaDonId = RandomString.GenerateRandomString(_context.HoaDon.Select(a => a.HoaDonId)),
                ChiTietHoaDon = list,
                MaTaiKhoan = HttpContext.User.FindFirst(ClaimTypes.Sid).Value,
                NgayLap = DateTime.Now,
                TongTien = tt,
                TrangThai = "Đang chờ",
            };
            foreach (var item in list)
            {
                item.MaHoaDon = hoadon.HoaDonId;
                item.MaSanPhamNavigation = null;

                var spToUpdate = await _context.SanPham.SingleOrDefaultAsync(a => a.SanPhamId == item.MaSanPham);
                spToUpdate.SoLuong -= item.SoLuong;
                _context.Update(spToUpdate);
            }

            await _context.AddAsync(hoadon);
            await _context.SaveChangesAsync();
            HttpContext.Session.SetSessionObject("Cart", new List<ChiTietHoaDon>());

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> UpdateCartItem(string id, double? qty)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Checkout");
            }

            List<ChiTietHoaDon> list = HttpContext.Session.GetSessionObject<List<ChiTietHoaDon>>("Cart");
            if (list != null)
            {
                var sp = list.SingleOrDefault(a => a.MaSanPham == id);
                var spnav = await _context.SanPham.SingleOrDefaultAsync(a => a.SanPhamId == id);
                if (sp.SoLuong != (qty ?? 1))
                {
                    list.Remove(sp);
                    sp.SoLuong = qty ?? 1;
                    sp.TongTien = sp.DonGia * (decimal?)sp.SoLuong ?? 0;

                    list.Add(new ChiTietHoaDon { MaSanPham = id, DonGia = spnav.GiaBan, SoLuong = sp.SoLuong, TongTien = sp.TongTien, MaSanPhamNavigation = spnav });
                    HttpContext.Session.SetSessionObject("Cart", list);
                }
            }

            return RedirectToAction("Checkout");
        }

        public async Task<IActionResult> OrderReview(string orderid)
        {
            ViewBag.OrderId = OrderId();
            if (string.IsNullOrEmpty(orderid))
            {
                return View(new List<ChiTietHoaDon>());
            }

            var item = await _context.ChiTietHoaDon.Where(a => a.MaHoaDon == orderid)
                                        .Include(a => a.MaHoaDonNavigation)
                                        .Include(a => a.MaSanPhamNavigation)
                                        .ToListAsync();

            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> CancellingOrder(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("OrderReview");
            }

            var item = await _context.HoaDon.Include(a => a.ChiTietHoaDon).SingleOrDefaultAsync(a => a.HoaDonId == id);
            item.TrangThai = "Đã xóa";
            foreach (var i in item.ChiTietHoaDon)
            {
                i.TrangThai = "Đã xóa";
            }

            await TryUpdateModelAsync(item);
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderReview");
        }

        public List<SelectListItem> OrderId()
        {
            var list = new List<SelectListItem>();
            foreach (var item in _context.HoaDon.AsEnumerable())
            {
                list.Add(new SelectListItem(item.HoaDonId, item.HoaDonId));
            }
            return list;
        }
    }
}
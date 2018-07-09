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

        public async Task<IActionResult> SendToDeliver(string id)
        {
            ViewBag.ShopId = ShopId;

            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var cthd = await (from items in _context.ChiTietHoaDon.Include(a => a.MaSanPhamNavigation)
                              where items.MaHoaDon == id
                              && items.MaSanPhamNavigation.MaCuaHang == ShopId
                              select items).ToListAsync();

            var hd = await (from item in _context.HoaDon.Include(a => a.ChiTietHoaDon).ThenInclude(b => b.MaSanPhamNavigation)
                            where item.HoaDonId == id
                            select new HoaDon
                            {
                                HoaDonId = item.HoaDonId,
                                MaTaiKhoan = item.MaTaiKhoan,
                                NgayLap = item.NgayLap,
                                TongTien = item.TongTien,
                                TrangThai = item.TrangThai,
                                ChiTietHoaDon = cthd
                            }).SingleOrDefaultAsync();

            var tk = _context.TaiKhoan.Where(a => a.TaiKhoanId == hd.MaTaiKhoan).SingleOrDefault();

            ViewBag.Email = tk.Email;


            if (hd == null)
            {
                return NotFound();
            }

            return View(hd);
        }

        [HttpPost]
        public async Task<IActionResult> SendToDeliver(string id, string address, decimal? fee)
        {
            var hd = await (from item in _context.HoaDon
                            where item.HoaDonId == id
                            select item).SingleOrDefaultAsync();

            var cthd = await (from items in _context.ChiTietHoaDon.Include(a => a.MaSanPhamNavigation)
                              where items.MaHoaDon == id
                              && items.MaSanPhamNavigation.MaCuaHang == ShopId
                              select items).ToListAsync();

            decimal? tt = 0;

            if (cthd.Any(a => a.TrangThai == "Đã xóa" && a.TrangThai == "Đang chờ giao" && a.TrangThai == "Đã giao"))
            {
                return View(hd);
            }

            foreach (var item in cthd)
            {
                tt += item.TongTien ?? 0;
                item.TrangThai = "Đang chờ giao";

                _context.Update(item);
            }

            tt += fee ?? 0;
            var gh = new ThongTinGiaoHang
            {
                ThongTinId = RandomString.GenerateRandomString(_context.ThongTinGiaoHang.Select(a => a.ThongTinId)),
                DiaChi = address,
                MaHoaDon = id,
                PhiVanChuyen = fee ?? 0,
                SoTienPhaiThu = tt,
                TrangThai = "Đang chờ giao",
            };

            await _context.AddAsync(gh);

            var cthdToCheck = await (from items in _context.ChiTietHoaDon.Include(a => a.MaSanPhamNavigation)
                                     where items.MaHoaDon == id
                                     select items).ToListAsync();

            if (cthdToCheck.All(a => a.TrangThai == "Đang chờ giao"))
            {
                hd.TrangThai = "Đang chờ giao";

                _context.Update(hd);
            }

            await _context.SaveChangesAsync();

            return View(hd);
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrderDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var cthd = await (from items in _context.ChiTietHoaDon.Include(a => a.MaSanPhamNavigation)
                              where items.MaHoaDon == id
                              && items.MaSanPhamNavigation.MaCuaHang == ShopId
                              select items).ToListAsync();

            var oldhd = await (from item in _context.HoaDon
                               where item.HoaDonId == id
                               select item).SingleOrDefaultAsync();

            if (cthd.Any(a => a.TrangThai == "Đã xóa" && a.TrangThai == "Đang chờ giao" && a.TrangThai == "Đã giao"))
            {
                return RedirectToAction("Confirm", new { id });
            }

            var newcthd = new List<ChiTietHoaDon>();
            decimal? tt = 0;

            if (cthd != null)
            {
                foreach (var item in cthd)
                {
                    newcthd.Add(item);
                    tt += item.TongTien;
                    _context.Remove(item);
                }

                var newhd = new HoaDon
                {
                    HoaDonId = RandomString.GenerateRandomString(_context.HoaDon.Select(a => a.HoaDonId)),
                    MaTaiKhoan = oldhd.MaTaiKhoan,
                    NgayLap = DateTime.Now,
                    TrangThai = "Đã xóa",
                    TongTien = tt ?? 0,
                };

                foreach (var item in newcthd)
                {
                    item.MaHoaDon = newhd.HoaDonId;
                    item.TrangThai = "Đã xóa";
                }

                newhd.ChiTietHoaDon = newcthd;

                await _context.AddAsync(newhd);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Confirm", new { id });
        }

        //[HttpPost]
        //[ActionName("Confirm")]
        //public async Task<IActionResult> ConfirmPost(string id, string spid)
        //{
        //    if (string.IsNullOrEmpty(id))
        //    {
        //        return NotFound();
        //    }

        //    var item = await (from cthd in _context.ChiTietHoaDon.Where(a => a.MaHoaDon == id)
        //                                            .Include(a => a.MaHoaDonNavigation)
        //                                            .Include(a => a.MaSanPhamNavigation)
        //                      where cthd.MaHoaDon == id && cthd.MaSanPham == spid
        //                      select cthd).FirstOrDefaultAsync();

        //    if (item.TrangThai != "Đang chờ giao" && item.TrangThai != "Đã xóa")
        //    {
        //        item.TrangThai = "Đang chờ giao";
        //    }

        //    await TryUpdateModelAsync(item);

        //    var items = await (from hd in _context.HoaDon.Include(a => a.ChiTietHoaDon).ThenInclude(b => b.MaSanPhamNavigation)
        //                       where hd.HoaDonId == id
        //                       select hd).SingleOrDefaultAsync();

        //    if (items.ChiTietHoaDon.All(a => a.TrangThai == "Đang chờ giao"))
        //    {
        //        items.TrangThai = "Đang chờ giao";
        //        if(!_context.ThongTinGiaoHang.Any(a=>a.MaHoaDon == items.HoaDonId))
        //        {
        //            var thongtingh = new ThongTinGiaoHang
        //            {
        //                MaHoaDon = items.HoaDonId,
        //                ThongTinId = RandomString.GenerateRandomString(_context.ThongTinGiaoHang.Select(a => a.ThongTinId)),
        //                TrangThai = items.TrangThai
        //            };
        //            await _context.AddAsync(thongtingh);
        //        }
        //        await TryUpdateModelAsync(items);
        //    }

        //    await _context.SaveChangesAsync();

        //    return RedirectToAction("Confirm", new { id });
        //}
    }
}
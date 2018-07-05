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
    public class AdminOrderManageController : Controller
    {
        private readonly LazarusDbContext _context;

        public AdminOrderManageController(LazarusDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? page, string currentSort, string searchString, string filterByStatus)
        {
            var list = from ord in _context.HoaDon.Include(a => a.ChiTietHoaDon).ThenInclude(b => b.MaSanPhamNavigation)
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

            if (!string.IsNullOrEmpty(filterByStatus))
            {
                ViewBag.Status = filterByStatus;
                list = list.Where(a => a.TrangThai.ToLower().Contains(filterByStatus.ToLower()));
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

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var hd = from items in _context.HoaDon.Include(a => a.ChiTietHoaDon).ThenInclude(a => a.MaSanPhamNavigation)
                     where items.HoaDonId == id
                     select items;

            return View(await hd.SingleOrDefaultAsync());
        }

        public async Task<IActionResult> ChangeOrder(string id, string idsp)
        {
            ViewBag.HoaDonList = HoaDonList();
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(idsp))
            {
                return NotFound();
            }

            var cthd = await (from item in _context.ChiTietHoaDon.Include(a=>a.MaSanPhamNavigation)
                                 where item.MaHoaDon == id && item.MaSanPham == idsp
                                 select item).SingleOrDefaultAsync();

            return View(cthd);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeOrder(string id, string idsp, string newOrder)
        {
            ViewBag.HoaDonList = HoaDonList();

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(idsp))
            {
                return NotFound();
            }

            var oldcthd = await (from item in _context.ChiTietHoaDon
                                 where item.MaHoaDon == id && item.MaSanPham == idsp
                                 select item).SingleOrDefaultAsync();

            var oldhd = await (from item in _context.HoaDon
                               where item.HoaDonId == id
                               select item).SingleOrDefaultAsync();

            if (string.IsNullOrEmpty(newOrder))
            {
                if (oldcthd != null && oldhd != null)
                {
                    var hd = new HoaDon
                    {
                        HoaDonId = RandomString.GenerateRandomString(_context.HoaDon.Select(a => a.HoaDonId)),
                        MaTaiKhoan = oldhd.MaTaiKhoan,
                        DiaChiGiao = oldhd.DiaChiGiao,
                        NgayLap = oldhd.NgayLap,
                        TrangThai = oldhd.TrangThai,
                    };

                    var newcthd = new ChiTietHoaDon
                    {
                        MaHoaDon = hd.HoaDonId,
                        MaSanPham = oldcthd.MaSanPham,
                        DonGia = oldcthd.DonGia,
                        SoLuong = oldcthd.SoLuong,
                        TongTien = oldcthd.TongTien,
                        TrangThai = oldcthd.TrangThai
                    };

                    hd.TongTien = newcthd.TongTien;
                    hd.ChiTietHoaDon = new ChiTietHoaDon[] { newcthd };

                    _context.Remove(oldcthd);
                    await _context.AddAsync(hd);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                if (oldcthd != null)
                {
                    var newhd = await (from hd in _context.HoaDon.Include(a => a.ChiTietHoaDon)
                                       where hd.HoaDonId == newOrder
                                       select hd).SingleOrDefaultAsync();
                    if (newhd != null)
                    {
                        if (newhd.ChiTietHoaDon.Any(a => a.MaSanPham != idsp))
                        {
                            var cthdToUpdate = await (from cthd in _context.ChiTietHoaDon
                                                      where cthd.MaHoaDon == newOrder && cthd.MaSanPham == idsp
                                                      select cthd).SingleOrDefaultAsync();
                            cthdToUpdate.SoLuong += oldcthd.SoLuong;
                            cthdToUpdate.TongTien = (decimal?)cthdToUpdate.SoLuong * cthdToUpdate.DonGia ?? 0;
                            _context.Update(cthdToUpdate);
                            newhd.TongTien = 0;
                            foreach (var item in newhd.ChiTietHoaDon)
                            {
                                newhd.TongTien += item.TongTien;
                            }

                            _context.Remove(oldcthd);

                            oldhd.TongTien = 0;
                            foreach(var item in oldhd.ChiTietHoaDon)
                            {
                                oldhd.TongTien += (item.TongTien ?? 0);
                            }

                            _context.Update(newhd);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            var cthdToAdd = new ChiTietHoaDon
                            {
                                MaHoaDon = newhd.HoaDonId,
                                MaSanPham = oldcthd.MaSanPham,
                                DonGia = oldcthd.DonGia,
                                SoLuong = oldcthd.SoLuong,
                                TongTien = oldcthd.TongTien,
                                TrangThai = oldcthd.TrangThai
                            };

                            _context.Remove(oldcthd);

                            oldhd.TongTien = 0;
                            foreach (var item in oldhd.ChiTietHoaDon)
                            {
                                oldhd.TongTien += (item.TongTien ?? 0);
                            }

                            await _context.AddAsync(cthdToAdd);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }

            return RedirectToAction("Edit", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeOrderStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var item = await _context.HoaDon.Where(a => a.HoaDonId == id).SingleOrDefaultAsync();

            if(item == null)
            {
                return NotFound();
            }

            if(item.TrangThai != "Đã xóa")
            {
                item.TrangThai = "Đã xóa";
            }
            else
            {
                item.TrangThai = "Đang chờ";
            }

            _context.Update(item);
            await _context.SaveChangesAsync();

            return RedirectToAction("Edit", new { id });
        }

        public List<SelectListItem> SanPhamList()
        {
            var list = new List<SelectListItem>();

            foreach (var item in _context.SanPham.ToList())
            {
                list.Add(new SelectListItem(item.TenSanPham, item.SanPhamId));
            }

            return list;
        }

        public List<SelectListItem> HoaDonList()
        {
            var list = new List<SelectListItem>();

            var items = from hd in _context.HoaDon
                        orderby hd.NgayLap descending
                        where hd.TrangThai != "Đã xóa" && hd.TrangThai != "Đã giao"
                        select hd;

            foreach (var item in items)
            {
                list.Add(new SelectListItem($"Mã hóa đơn: {item.HoaDonId}, Ngày lập: {item.NgayLap}", item.HoaDonId));
            }

            return list;
        }
    }
}
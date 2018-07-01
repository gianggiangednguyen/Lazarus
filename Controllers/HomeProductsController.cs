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
    public class HomeProductsController : Controller
    {
        private readonly LazarusDbContext _context;

        public HomeProductsController(LazarusDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var list = from items in _context.LoaiSanPham.Include(a => a.SanPham)
                       select new LoaiSanPham
                       {
                           LoaiSanPhamId = items.LoaiSanPhamId,
                           TenLoaiSanPham = items.TenLoaiSanPham,
                           TrangThai = items.TrangThai,
                           SanPham =  (from sps in items.SanPham
                                      orderby sps.NgayThem descending
                                      select sps).Take(3).ToList()
                       };

            return View(list);
        }

        public async Task<IActionResult> Catalogue(string id)
        {
            var loaisp = await _context.LoaiSanPham.Include(a => a.SanPham).Where(a => a.LoaiSanPhamId == id).SingleOrDefaultAsync();

            return View(loaisp);
        }
    }
}
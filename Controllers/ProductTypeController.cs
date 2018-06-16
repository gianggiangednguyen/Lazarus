using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Lazarus.Data;
using Lazarus.Models;

namespace Lazarus.Controllers
{
    [Authorize(Policy = "AdminPolicy")]
    public class ProductTypeController : Controller
    {
        private readonly LazarusDbContext _context;

        public ProductTypeController(LazarusDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? page, string currentSort, string searchString)
        {
            var lstLoaiSp = from loaisp in _context.LoaiSanPham
                            select loaisp;

            ViewBag.CurrentSort = currentSort;
            ViewBag.SortByName = currentSort == "ByName" ? "ByName_desc" : "ByName";
            ViewBag.SortByStatus = currentSort == "ByStatus" ? "ByStatus_desc" : "ByStatus";

            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.SearchString = searchString;
                lstLoaiSp = lstLoaiSp.Where(a => a.LoaiSanPhamId.Contains(searchString));
            }

            switch(searchString)
            {
                case "ByName":
                    lstLoaiSp = lstLoaiSp.OrderBy(a => a.TenLoaiSanPham);
                    break;
                case "ByName_desc":
                    lstLoaiSp = lstLoaiSp.OrderByDescending(a => a.TenLoaiSanPham);
                    break;
                case "ByStatus":
                    lstLoaiSp = lstLoaiSp.OrderBy(a => a.TrangThai);
                    break;
                case "ByStatus_desc":
                    lstLoaiSp = lstLoaiSp.OrderByDescending(a => a.TrangThai);
                    break;
                default:
                    break;
            }

            return View(await PagedList<LoaiSanPham>.CreateAsync(lstLoaiSp, page ?? 1, 15));
        }
    }
}
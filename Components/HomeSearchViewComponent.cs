using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lazarus.Models;
using Lazarus.Data;


namespace Lazarus.Components
{
    public class HomeSearchViewComponent :ViewComponent
    {
        private readonly LazarusDbContext _context;

        public HomeSearchViewComponent(LazarusDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            ViewBag.LoaiSanPhamList = LoaiSanPhamList();

            return await Task.FromResult<IViewComponentResult>(View());
        }

        public List<SelectListItem> LoaiSanPhamList()
        {
            var list = new List<SelectListItem>();
            var items = _context.LoaiSanPham.Where(a => a.TrangThai != "Đã xóa").ToList();

            foreach (var item in items)
            {
                list.Add(new SelectListItem(item.TenLoaiSanPham, item.LoaiSanPhamId));
            }

            return list;
        }
    }
}

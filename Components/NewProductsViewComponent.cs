using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Lazarus.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lazarus.Components
{
    public class NewProductsViewComponent : ViewComponent
    {
        private readonly LazarusDbContext _context;

        public NewProductsViewComponent(LazarusDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var list = (from sp in _context.SanPham.Include(a => a.MaCuaHangNavigation)
                        where sp.TrangThai != "Đã xóa"
                        && (sp.MaCuaHangNavigation.TrangThai != "Đã xóa" || sp.MaCuaHangNavigation.TrangThai != "Ngưng hoạt động")
                        orderby sp.NgayThem descending
                        select sp).Take(12);

            return await Task.FromResult<IViewComponentResult>(View(list));
        }
    }
}

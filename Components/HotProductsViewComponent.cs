﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Lazarus.Data;
using Lazarus.Models;

namespace Lazarus.Components
{
    public class HotProductsViewComponent : ViewComponent
    {
        private readonly LazarusDbContext _context;

        public HotProductsViewComponent(LazarusDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var ids = (from sp in _context.ChiTietHoaDon.Include(t => t.MaSanPhamNavigation)
                       where sp.MaSanPhamNavigation.TrangThai != "Đã xóa"
                       group sp by sp.MaSanPham into grp
                       let o = new { id = grp.Key, count = grp.Count() }
                       orderby o.count
                       select o.id).Take(8);

            var list = new List<SanPham>();

            foreach (var id in ids)
            {
                var spid = id;
                var sp = await _context.SanPham.Where(a => a.SanPhamId == spid).SingleOrDefaultAsync();
                list.Add(sp);
            }

            return View(list);
        }
    }
}

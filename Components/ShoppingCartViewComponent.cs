using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.EntityFrameworkCore;
using Lazarus.Data;
using Lazarus.Models;

namespace Lazarus.Components
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly LazarusDbContext _context;

        public ShoppingCartViewComponent(LazarusDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var list = HttpContext.Session.GetSessionObject<List<ChiTietHoaDon>>("Cart");

            if (list != null)
            {
                return await Task.FromResult<IViewComponentResult>(View(list));
            }

            return await Task.FromResult<IViewComponentResult>(View(new List<ChiTietHoaDon>()));
        }
    }
}

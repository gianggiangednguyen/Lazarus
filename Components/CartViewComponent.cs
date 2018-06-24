using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Lazarus.Models;
using Lazarus.Data;

namespace Lazarus.Components
{
    public class CartViewComponent : ViewComponent
    {
        private readonly LazarusDbContext _context;

        public CartViewComponent(LazarusDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cart = HttpContext.Session.GetSessionObject<List<ChiTietHoaDon>>("Cart");

            return await Task.FromResult<IViewComponentResult>(View(cart));
        }
    }
}

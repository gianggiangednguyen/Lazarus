using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
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


            return View();
        }
    }
}

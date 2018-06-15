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
    public class LoginFormViewComponent : ViewComponent
    {
        private readonly LazarusDbContext _context;

        public LoginFormViewComponent(LazarusDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}

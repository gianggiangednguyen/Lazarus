using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Lazarus.Models;

namespace Lazarus.Controllers
{
    public class HomeController : Controller
    {
        public readonly LazarusDbContext _context;

        public HomeController(LazarusDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            //TODO: Copy this lul
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                var value = HttpContext.User.FindFirst(ClaimTypes.Role).Value;
            }
            return View();
        }


        [Authorize(Policy = "NormalUser")]
        public IActionResult Subscription()
        {
            return View();
        }
    }
}
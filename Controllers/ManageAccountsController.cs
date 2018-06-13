using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Lazarus.Models;

namespace Lazarus.Controllers
{
    [Authorize(Policy = "Admin")]
    public class ManageAccountsController : Controller
    {
        private readonly LazarusDbContext _context;

        public ManageAccountsController(LazarusDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
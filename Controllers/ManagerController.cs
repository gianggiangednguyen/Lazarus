using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Lazarus.Models;

namespace Lazarus.Controllers
{
    [Authorize(Policy = "Manager")]
    public class ManagerController : Controller
    {
        private readonly LazarusDbContext _context;

        public ManagerController(LazarusDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
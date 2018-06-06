using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lazarus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Lazarus.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<TaiKhoan> _userManager;
        private readonly SignInManager<TaiKhoan> _signInManager;

        public AccountController(UserManager<TaiKhoan> userManager, SignInManager<TaiKhoan> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(TaiKhoan model)
        {
            if (ModelState.IsValid)
            {
                var tk = new TaiKhoan
                {
                    Email = model.Email,
                    TrangThai = "Unverify"
                };

                var result = await _userManager.CreateAsync(tk, model.MatKhau);
                if (result.Succeeded)
                {

                }
            }

            return View(model);
        }
    }
}
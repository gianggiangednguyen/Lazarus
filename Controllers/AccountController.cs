using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
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
                var tk = new ApplicationUser()
                {
                    Id = model.TaiKhoanId,
                    Email = model.Email,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(tk, model.MatKhau);
                if (result.Succeeded)
                {
                    var m = new MailMessage(new MailAddress("lazarus@noreply.com", "Confimation"),
                        new MailAddress(tk.Email))
                    {
                        Subject = "Account Confirmation",
                        Body =
                            $"Click <a href=\"{Url.Action("ConfirmEmail", "Account", new {id = tk.Id, code = tk.Email})}\">here</a> to complete the registration",
                        IsBodyHtml = true
                    };

                    var smtp = new SmtpClient("smtp.lazarus.com")
                    {
                        Credentials = new NetworkCredential("sender@lazarus.com", "12345"),
                        EnableSsl = true
                    };
                    smtp.Send(m);
                    return RedirectToAction("Confirm", "Account", new {email = model.Email});
                }
            }

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Confirm(string email)
        {
            ViewBag["Email"] = email;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string id, string code)
        {
            if (id == null && code == null)
            {
                RedirectToAction("Index", "Home");
            }

            var tk = await _userManager.FindByIdAsync(id);
            if (tk == null)
            {
                throw new ApplicationException($"Không thể tìm thấy user với {id}");
            }

            var result = await _userManager.ConfirmEmailAsync(tk, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }
    }
}
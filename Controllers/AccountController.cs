using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Lazarus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lazarus.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly LazarusDbContext _context;

        // Inject DbContext vao controller
        public AccountController(LazarusDbContext context)
        {
            _context = context;
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
                var tk = new TaiKhoan()
                {
                    TaiKhoanId = "",
                    Email = model.Email,
                    TrangThai = "Unathorize"
                };

                var result = await _context.AddAsync(model);
                if (result.State == EntityState.Added)
                {
                    var m = new MailMessage(new MailAddress("lazarus@noreply.com", "Confimation"),
                        new MailAddress(tk.Email))
                    {
                        Subject = "Account Confirmation",
                        Body =
                            $"Click <a href=\"{Url.Action("ConfirmEmail", "Account", new { id = tk.TaiKhoanId, code = tk.Email })}\">here</a> to complete the registration",
                        IsBodyHtml = true
                    };

                    var smtp = new SmtpClient("smtp.lazarus.com")
                    {
                        Credentials = new NetworkCredential("sender@lazarus.com", "12345"),
                        EnableSsl = true
                    };
                    smtp.Send(m);
                    return RedirectToAction("Confirm", "Account", new { email = model.Email });
                }
                else
                {
                    TempData.Add("Error", "Unauthorize");
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

            var tk = await _context.TaiKhoan.SingleOrDefaultAsync(i => i.TaiKhoanId == id);
            if (tk == null)
            {
                return NotFound();
            }

            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }
    }
}
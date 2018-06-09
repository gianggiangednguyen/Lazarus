using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Security.Claims;
using Lazarus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Lazarus.Data;

namespace Lazarus.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly LazarusDbContext _context;

        private TaiKhoan taiKhoan;

        // Inject DbContext vao controller
        public AccountController(LazarusDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(TaiKhoan model)
        {
            //TODO: Authentical

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, model.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(claimsPrincipal);

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
                model.TaiKhoanId = RandomString.CreateRandomString;
                model.TrangThai = "Unverify";

                taiKhoan = new TaiKhoan
                {
                    TaiKhoanId = model.TaiKhoanId,
                    Ho = model.Ho,
                    Ten = model.Ten,
                    Email = model.Email,
                    MatKhau = model.MatKhau,
                    NhapLaiMatKhau = model.NhapLaiMatKhau,
                    TrangThai = model.TrangThai
                };

                var m = new MailMessage(new MailAddress("lazarus@noreply.com"), new MailAddress(taiKhoan.Email))
                {
                    Subject = "Account Confirmation",
                    Body = $"Click <a href=\"{Url.Action("ConfirmEmail", "Account", new { model = taiKhoan })}\">here</a> to complete the registration",
                    IsBodyHtml = true
                };

                var smtp = new SmtpClient("smtp.gmail.com")
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("chibaoho2@gmail.com", "songlongbatbo366511"),
                    Port = 25,
                    EnableSsl = true
                };

                await smtp.SendMailAsync(m);
                return RedirectToAction("Confirm", "Account", new { email = model.Email });
            }

            TempData.Add("Error", "Password");
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
        public async Task<IActionResult> ConfirmEmail(TaiKhoan model)
        {
            var checkingModel = await _context.TaiKhoan.FindAsync(model);

            if (checkingModel != null)
            {
                if (model.TrangThai == "Verified")
                {
                    return RedirectToAction("Error", "Account");
                }
                else
                {
                    await _context.TaiKhoan.AddAsync(model);
                    _context.SaveChanges();
                }
            }

            return View();
        }

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
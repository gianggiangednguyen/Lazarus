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
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(TaiKhoan model)
        {
            var tk = await (from taiKhoan in _context.TaiKhoan.Include(a => a.MaLoaiTaiKhoanNavigation).Include(b => b.TaiKhoanPremium.MaTaiKhoanNavigation)
                            where taiKhoan.Email == model.Email
                                && taiKhoan.MatKhau == model.MatKhau
                            select taiKhoan).FirstOrDefaultAsync();

            if (tk != null)
            {
                //Tao ra 1 claim
                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Sid, tk.TaiKhoanId),
                    new Claim(ClaimTypes.Email, tk.Email),
                    new Claim(ClaimTypes.Role, "UnverifiedUser")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); //gan claim vao cookie

                if (tk.TrangThai == "Verified")
                {
                    claimsIdentity.RemoveClaim(claimsIdentity.FindFirst(ClaimTypes.Role));
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "NormalUser"));

                    if (tk.MaLoaiTaiKhoan == "AD")
                    {
                        claimsIdentity.RemoveClaim(claimsIdentity.FindFirst(ClaimTypes.Role));
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                    }
                    else if (tk.MaLoaiTaiKhoan == "SM" && tk.TaiKhoanPremium.NgayKetThuc < DateTime.Now)
                    {
                        claimsIdentity.RemoveClaim(claimsIdentity.FindFirst(ClaimTypes.Role));
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "ShopManager"));
                    }
                }

                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity); //claimprincipal
                await HttpContext.SignInAsync(claimsPrincipal);

                return RedirectToAction("Index", "Home");

            }

            ViewData["error"] = "Tài khoản hoặc mật khẩu không đúng";
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
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
                try
                {
                    model.TaiKhoanId = RandomString.GenerateRandomString(_context.TaiKhoan.Select(o => o.TaiKhoanId));
                    model.TrangThai = "Unverified";
                    model.MaLoaiTaiKhoan = "NU";
                    await _context.AddAsync(model);
                    await _context.SaveChangesAsync();

                    var m = new MailMessage(new MailAddress("lazarus@noreply.com"), new MailAddress(model.Email))
                    {
                        Subject = "Account Confirmation",
                        Body = $"Click <a href=\"{Url.Action("ConfirmEmail", "Account", new { id = model.TaiKhoanId }, Request.Scheme)}\">here</a> to complete the registration",
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
                }
                catch (DbUpdateException)
                {
                    ViewData["ex"] = "DB error LuL";
                }

                return RedirectToAction("Confirm", "Account", new { email = model.Email });
            }

            TempData.Add("Error", "Password");
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Confirm(string email)
        {
            ViewData["Email"] = email;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string id)
        {
            var tk = await _context.TaiKhoan.FirstOrDefaultAsync(a => a.TaiKhoanId == id);

            if (tk != null)
            {
                if (tk.TrangThai == "Verified")
                {
                    return RedirectToAction("Error", "Account");
                }
                else
                {
                    tk.TrangThai = "Verified";

                    _context.Update(tk);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Success", "Account");
                }
            }
            return RedirectToAction("Error", "Account");
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
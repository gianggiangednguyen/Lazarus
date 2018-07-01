using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Lazarus.Models;
using Lazarus.Data;
using Microsoft.AspNetCore.Http;

namespace Lazarus.Controllers
{
    [Authorize(Policy = "ManagerPolicy")]
    public class AdminAccountController : Controller
    {
        private readonly LazarusDbContext _context;

        public AdminAccountController(LazarusDbContext context)
        {
            _context = context;
        }

        public string UserId
        {
            get
            {
                var userid = HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
                return userid;
            }
        }

        public async Task<IActionResult> Index()
        {
            var user = await _context.TaiKhoan.Where(a => a.TaiKhoanId == UserId).SingleOrDefaultAsync();

            return View(user);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _context.TaiKhoan.Where(a => a.TaiKhoanId == id).SingleOrDefaultAsync();

            return View(user);
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost()
        {
            var userToUpdate = await _context.TaiKhoan.Where(a => a.TaiKhoanId == UserId).SingleOrDefaultAsync();

            if (HttpContext.Request.Form.Files.Count > 0)
            {
                IFormFile hinhanh = HttpContext.Request.Form.Files.First();

                string ext = Path.GetExtension(hinhanh.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/AccountImages/", $"{userToUpdate.TaiKhoanId}{ext}");

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await hinhanh.CopyToAsync(stream);
                }

                userToUpdate.HinhAnh = $"{userToUpdate.TaiKhoanId}{ext}";
            }

            await TryUpdateModelAsync(userToUpdate,
                                        "",
                                        a => a.Ho, a => a.Ten);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public IActionResult PasswordChange()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PasswordChange(TaiKhoan model, string oldPass)
        {
            var userToUpdate = await _context.TaiKhoan.Where(a => a.TaiKhoanId == UserId).SingleOrDefaultAsync();

            if (userToUpdate != null)
            {
                if (userToUpdate.MatKhau == oldPass)
                {
                    ModelState.Remove("Ho");
                    ModelState.Remove("Ten");
                    ModelState.Remove("Email");
                    if (ModelState.IsValid)
                    {
                        await TryUpdateModelAsync(userToUpdate, "",
                                                    a => a.MatKhau);
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Index");
                    }

                    return View();
                }
            }

            ViewBag.OldPasswordError = "Mật khẩu không đúng";
            return View();
        }
    }
}
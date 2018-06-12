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
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Http;

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

        [Authorize(Policy = "NormalUser")]
        [HttpPost]
        public IActionResult Subscription(string value)
        {
            return RedirectToAction("ConfirmSubscription", "Home", new { id = HttpContext.User.FindFirst(ClaimTypes.Sid).Value, value });
        }

        [Authorize(Policy = "NormalUser")]
        public IActionResult ConfirmSubscription()
        {
            string str = Request.Query["value"].ToString();
            string val;
            switch (str)
            {
                case "100":
                    val = "100.000 VND";
                    break;
                case "500":
                    val = "500.000 VND";
                    break;
                case "1000":
                    val = "1.000.000 VND";
                    break;
                default:
                    val = "?????";
                    break;
            }
            ViewData["price"] = val;

            return View();
        }

        [Authorize(Policy = "NormalUser")]
        [HttpPost]
        public async Task<IActionResult> ConfirmSubscription(string id, string value)
        {
            var tk = await _context.TaiKhoan.Include(n => n.TaiKhoanPremium.MaTaiKhoanNavigation).Where(p => p.TaiKhoanId == id).FirstOrDefaultAsync();

            if (tk != null)
            {
                if (tk.TaiKhoanPremium == null)
                {
                    TaiKhoanPremium newobj = new TaiKhoanPremium
                    {
                        MaTaiKhoan = id,
                        NgayBatDau = DateTime.Now
                    };

                    if (value == "100")
                    {
                        newobj.NgayKetThuc = DateTime.Now.AddMonths(1);
                    }
                    else if (value == "500")
                    {
                        newobj.NgayKetThuc = DateTime.Now.AddMonths(6);
                    }
                    else
                    {
                        newobj.NgayKetThuc = DateTime.Now.AddYears(1);
                    }

                    await _context.AddAsync(newobj);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    if (tk.TaiKhoanPremium.NgayKetThuc > DateTime.Now)
                    {
                        ViewData["view"] = "Thời gian Premium còn thời hạn.";

                        return View();
                    }
                    else
                    {
                        TaiKhoanPremium obj = new TaiKhoanPremium
                        {
                            MaTaiKhoan = id,
                            NgayBatDau = DateTime.Now
                        };

                        if (value == "100")
                        {
                            obj.NgayKetThuc = DateTime.Now.AddMonths(1);
                        }
                        else if (value == "500")
                        {
                            obj.NgayKetThuc = DateTime.Now.AddMonths(6);
                        }
                        else
                        {
                            obj.NgayKetThuc = DateTime.Now.AddYears(1);
                        }

                        _context.Update(obj);
                        await _context.SaveChangesAsync();
                    }
                }

                ViewData["view"] = "Thành công!";

                return View();
            }

            ViewData["view"] = "Lỗi xảy ra!";

            return View();
        }

        public async Task<IActionResult> AccountDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var tk = await _context.TaiKhoan.Where(m => m.TaiKhoanId == id).Include(t => t.MaCongTyNavigation).FirstOrDefaultAsync();

            if (tk != null)
            {
                return View(tk);
            }

            return View();
        }

        public async Task<IActionResult> AccountEdit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                NotFound();
            }

            var tk = await _context.TaiKhoan.Where(m => m.TaiKhoanId == id).Include(t => t.MaCongTyNavigation).FirstOrDefaultAsync();

            if (tk != null)
            {
                return View(tk);
            }

            return View();
        }

        [ActionName("AccountEdit")]
        [HttpPost]
        public async Task<IActionResult> AccountEditPost(string id)
        {
            var modelToUpdate = await _context.TaiKhoan.Where(o => o.TaiKhoanId == id).FirstOrDefaultAsync();
            IFormFile file = HttpContext.Request.Form.Files.First();

            if (!string.IsNullOrEmpty(file.FileName))
            {
                var ext = Path.GetExtension(file.FileName);

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/AccountImages/", $"{modelToUpdate.TaiKhoanId}{ext}");

                modelToUpdate.HinhAnh = $"{modelToUpdate.TaiKhoanId}{ext}";

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            //TryUpdateModel se check modelstate
            await TryUpdateModelAsync(modelToUpdate, "", a => a.Ten, a => a.Ho);
            await _context.SaveChangesAsync();

            return View(modelToUpdate);
        }
    }
}
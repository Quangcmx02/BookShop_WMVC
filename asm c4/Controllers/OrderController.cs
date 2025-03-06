using asm_c4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace asm_c4.Controllers
{
    public class OrderController : Controller
    {
        private readonly QuanLySachContext _context;
        public OrderController(QuanLySachContext context)
        {
            _context = context;
        }
        private bool IsUserLoggedIn()
        {
            var userSession = HttpContext.Session.GetString("UserSession");
            return !string.IsNullOrEmpty(userSession);
        }

        // Lấy thông tin đơn hàng của người dùng hiện tại
        public IActionResult Index()
        {
            var userSession = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(userSession))
            {
                return RedirectToAction("Login", "Account"); // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
            }

            var currentUser = JsonConvert.DeserializeObject<User>(userSession);

            var orders = _context.DonHangs
                .Where(o => o.UserId == currentUser.UserId)
                .Include(o => o.DonHangChiTiets)

                .ThenInclude(od => od.Sach)
                .Include(o => o.DonHangChiTiets)
                .ThenInclude(od => od.Combo) // Bao gồm combo nếu có
                  .OrderByDescending(d => d.NgayDat)
                .ToList();

            return View(orders);
        }

    }
}

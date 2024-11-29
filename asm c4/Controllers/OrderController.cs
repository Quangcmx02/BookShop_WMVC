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

            // Deserialize người dùng từ session
            var currentUser = JsonConvert.DeserializeObject<User>(userSession);

            // Lấy danh sách đơn hàng của người dùng từ cơ sở dữ liệu
            var orders = _context.DonHangs
                .Where(o => o.UserId == currentUser.UserId)
                .Include(o => o.DonHangChiTiets) // Lấy thông tin chi tiết đơn hàng
                .ThenInclude(od => od.Sach) // Lấy thông tin sản phẩm của đơn hàng
                .ToList();

            // Trả về view và truyền danh sách đơn hàng cho view
            return View(orders);
        }
    }
}

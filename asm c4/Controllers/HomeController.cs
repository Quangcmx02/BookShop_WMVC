using Microsoft.AspNetCore.Mvc;
using System.Linq;
using X.PagedList;
using asm_c4.Models;
using X.PagedList.Extensions;
namespace asm_c4.Controllers
{
    public class HomeController : Controller
    {
        private readonly QuanLySachContext _context;

        public HomeController(QuanLySachContext context)
        {
            _context = context;
        }

        public IActionResult Index(int pageSach = 1, int pageCombo = 1)
        {
            // Số sản phẩm hiển thị
            int pageSize = 8;

            // Lấy sách (trạng thái đang kinh doanh)
            var sachList = _context.Saches
                .Where(s => s.TrangThai) // Lọc sách đang kinh doanh
                .OrderBy(s => s.TenSach)
                .ToPagedList(pageSach, pageSize);

            // Lấy combo (trạng thái đang kinh doanh)
            var comboList = _context.Combos
                .Where(c => c.TrangThai) // Lọc combo đang kinh doanh
                .OrderBy(c => c.TenCombo)
                .ToPagedList(pageCombo, pageSize);

            // Truyền cả hai danh sách vào ViewBag
            ViewBag.SachList = sachList;
            ViewBag.ComboList = comboList;

            return View();
        }
    }
}

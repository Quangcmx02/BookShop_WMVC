using asm_c4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace asm_c4.Controllers
{
    public class AdminController : Controller
    {
        private readonly QuanLySachContext _context;

        public AdminController(QuanLySachContext context)
        {
            _context = context;
        }
        private bool IsAdmin()
        {
            var userSession = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(userSession))
            {
                return false;
            }

            var currentUser = JsonConvert.DeserializeObject<User>(userSession);
            return currentUser.VaiTro == "Admin";
        }

        public IActionResult BookManagement()
        {
            var userSession = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(userSession))
            {
                return RedirectToAction("Login", "Account");
            }

            var books = _context.Saches.Include(s => s.DanhMuc).ToList();
            return View(books);
        }


        [HttpGet]
        public IActionResult CreateBook()
        {

            var userSession = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(userSession))
            {
                return RedirectToAction("Login", "Account");
            }
            ViewData["DanhMucId"] = new SelectList(_context.DanhMucs, "DanhMucId", "DanhMucId");
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> CreateBook([Bind("SachId,TenSach,MoTa,HinhAnh,GiaTien,SoLuong,TrangThai,DanhMucId")] Sach sach)
        {
            var userSession = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(userSession))
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                _context.Add(sach);
                await _context.SaveChangesAsync();
                return RedirectToAction("BookManagement");
            }
            ViewData["DanhMucId"] = new SelectList(_context.DanhMucs, "DanhMucId", "DanhMucId", sach.DanhMucId);
            return View(sach);
        }


        [HttpGet]
        public IActionResult EditBook(int id)
        {
            var userSession = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(userSession))
            {
                return RedirectToAction("Login", "Account");
            }
            var sach = _context.Saches.Find(id);
            if (sach == null)
            {
                return NotFound();
            }

            ViewBag.TrangThaiOptions = new SelectList(new[]
            {
        new { Value = "true", Text = "Đang kinh doanh" },
        new { Value = "false", Text = "Ngừng kinh doanh" }
    }, "Value", "Text", sach.TrangThai);
            ViewData["DanhMucId"] = new SelectList(_context.DanhMucs, "DanhMucId", "DanhMucId", sach.DanhMucId);
            return View(sach);
        }


        [HttpPost]
        public IActionResult EditBook(Sach sach)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Saches.Update(sach);
                    _context.SaveChanges();
                    return RedirectToAction("BookManagement", "Admin"); // Đảm bảo action và controller đúng
                }
                catch (Exception ex)
                {
                    // Log lỗi nếu cần
                    ModelState.AddModelError(string.Empty, "Có lỗi khi cập nhật sách: " + ex.Message);
                }
            }

            ViewData["DanhMucId"] = new SelectList(_context.DanhMucs, "DanhMucId", "DanhMucId", sach.DanhMucId);
            return View(sach);
        }

        // Xóa sách
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var userSession = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(userSession))
            {
                return RedirectToAction("Login", "Account");
            }
            var sach = _context.Saches.Find(id);
            if (sach == null)
            {
                return NotFound();
            }
            _context.Saches.Remove(sach);
            _context.SaveChanges();
            return RedirectToAction("BookManagement");
        }
        public IActionResult AccountManagement()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var users = _context.Users.ToList();
            return View(users);
        }

        // Tạo tài khoản mới
        [HttpGet]
        public IActionResult CreateAccount()
        {
            var userSession = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(userSession))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public IActionResult CreateAccount(User user)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("AccountManagement");
            }

            return View(user);
        }

        // Sửa thông tin tài khoản
        [HttpGet]
        public IActionResult EditAccount(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        public IActionResult EditAccount(User user)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                _context.Users.Update(user);
                _context.SaveChanges();
                return RedirectToAction("AccountManagement");
            }

            return View(user);
        }

        // Xóa tài khoản
        [HttpGet]
        public IActionResult DeleteAccount(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.VaiTro == "Admin")
            {
                TempData["ErrorMessage"] = "Không thể xóa tài khoản admin.";
                return RedirectToAction(nameof(Index));
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
            return RedirectToAction("AccountMangement");
        }
        [HttpGet]
        public IActionResult CategoryManagement()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách danh mục từ cơ sở dữ liệu
            var categories = _context.DanhMucs.ToList();
            return View(categories);
        }

        // Tạo danh mục mới
        [HttpGet]
        public IActionResult CreateCategory()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public IActionResult CreateCategory(DanhMuc category)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                _context.DanhMucs.Add(category);
                _context.SaveChanges();
                return RedirectToAction(nameof(CategoryManagement));
            }

            return View(category);
        }

        // Sửa danh mục
        [HttpGet]
        public IActionResult EditCategory(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var category = _context.DanhMucs.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        public IActionResult EditCategory(DanhMuc category)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                _context.DanhMucs.Update(category);
                _context.SaveChanges();
                return RedirectToAction(nameof(CategoryManagement));
            }

            return View(category);
        }

        // Xóa danh mục
        [HttpGet]
        public IActionResult DeleteCategory(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var category = _context.DanhMucs.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            // Kiểm tra nếu danh mục đang được sử dụng bởi sách
            if (category.Saches.Any())
            {
                TempData["ErrorMessage"] = "Không thể xóa danh mục đang được sử dụng.";
                return RedirectToAction(nameof(CategoryManagement));
            }

            _context.DanhMucs.Remove(category);
            _context.SaveChanges();
            return RedirectToAction(nameof(CategoryManagement));
        }
        public IActionResult OrderManagement()
        {
            // Kiểm tra xem người dùng có phải admin hay không
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy tất cả đơn hàng từ cơ sở dữ liệu, bao gồm cả chi tiết đơn hàng
            var orders = _context.DonHangs
                                 .Include(d => d.DonHangChiTiets)
                                 .ThenInclude(d => d.Sach)  // Bao gồm sách trong chi tiết đơn hàng
                                 .Include(d => d.User)      // Bao gồm thông tin người dùng
                                 .ToList();

            return View(orders);
        }
        public IActionResult ViewOrder(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var order = _context.DonHangs
                                .Include(d => d.DonHangChiTiets)
                                .ThenInclude(d => d.Sach)  // Bao gồm sách trong chi tiết đơn hàng
                                .FirstOrDefault(d => d.DonHangId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        public IActionResult Dashboard()
        {
            return View();
        }
    }


}

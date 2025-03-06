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
            ViewData["TenDanhMuc"] = new SelectList(_context.DanhMucs, "TenDanhMuc", "TenDanhMuc");
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
            ViewData["DanhMucId"] = new SelectList(_context.DanhMucs, "TenDanhMuc", "TenDanhMuc", sach.DanhMuc.TenDanhMuc);
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
            ViewData["TenDanhMuc"] = new SelectList(_context.DanhMucs, "TenDanhMuc", "TenDanhMuc");
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

            ViewData["DanhMucId"] = new SelectList(_context.DanhMucs, "TenDanhMuc", "TenDanhMuc", sach.DanhMuc.TenDanhMuc);
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
                                     .ThenInclude(d => d.Sach) // Bao gồm sách trong chi tiết đơn hàng
                                 .Include(d => d.User)       // Bao gồm thông tin người dùng
                                 .OrderByDescending(d => d.NgayDat) // Sắp xếp theo ngày đặt mới nhất
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
                                .Include(d => d.DonHangChiTiets)
                                .ThenInclude(d => d.Combo) // Bao g?m combo trong chi ti?t don h?ng
                                .FirstOrDefault(d => d.DonHangId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string newStatus)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var order = _context.DonHangs.FirstOrDefault(o => o.DonHangId == orderId);
            if (order == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái đơn hàng
            order.TrangThai = newStatus;
            _context.SaveChanges();

            TempData["Message"] = "Cập nhật trạng thái thành công!";
            return RedirectToAction("ViewOrder", new { id = orderId });
        }

        public IActionResult Dashboard()
        {
            return View();
        }
        public async Task<IActionResult> ComboManagement()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var combos = await _context.Combos.Include(c => c.ComboBooks).ToListAsync();
            return View(combos);
        }

        [HttpGet]
        public IActionResult CreateCombo()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCombo([Bind("TenCombo,MoTa,Gia,TrangThai,LinkImages,Quantity")] Combo combo)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                _context.Combos.Add(combo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ComboManagement));
            }
            return View(combo);
        }

        [HttpGet]
        public IActionResult EditCombo(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var combo = _context.Combos.Find(id);
            if (combo == null)
            {
                return NotFound();
            }
            return View(combo);
        }

        [HttpPost]
        public async Task<IActionResult> EditCombo(int id, [Bind("ComboId,TenCombo,MoTa,Gia,TrangThai,LinkImages,Quantity")] Combo combo)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != combo.ComboId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(combo);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(ComboManagement));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Không thể cập nhật combo: " + ex.Message);
                }
            }
            return View(combo);
        }

        [HttpGet]
        public IActionResult DeleteCombo(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var combo = _context.Combos.Find(id);
            if (combo == null)
            {
                return NotFound();
            }

            _context.Combos.Remove(combo);
            _context.SaveChanges();
            return RedirectToAction(nameof(ComboManagement));
        }
        public IActionResult ComboDetails(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }
            var combo = _context.Combos
                .Where(c => c.ComboId == id && c.TrangThai)
                .Select(c => new
                {
                    ComboId = c.ComboId,
                    TenCombo = c.TenCombo,
                    MoTa = c.MoTa,
                    GiaTien = c.Gia,
                    Books = c.ComboBooks.Select(cb => new
                    {
                        SachId = cb.Sach.SachId,
                        TenSach = cb.Sach.TenSach,
                        MoTa = cb.Sach.MoTa,
                        HinhAnh = cb.Sach.HinhAnh,
                        GiaTien = cb.Sach.GiaTien,
                        TrangThai = cb.Sach.TrangThai,
                        DanhMuc = cb.Sach.DanhMuc.TenDanhMuc, // Tên danh mục
                        QuantityInCombo = cb.ComboBookDetails
                            .Where(cbd => cbd.ComboBooksId == cb.ComboBooksId)
                            .Select(cbd => cbd.QuantityBookInCombo)
                            .FirstOrDefault() // Số lượng sách trong combo
                    })
                })
                .FirstOrDefault();

            if (combo == null)
            {
                return NotFound();
            }

            return View(combo);
        }

        [HttpPost]
        public IActionResult CapNhatSoLuong(int comboId, int sachId, int soLuongMoi)
        {

            var comboBookDetail = _context.ComboBookDetails
                .FirstOrDefault(cbd => cbd.ComboBooks.ComboId == comboId && cbd.ComboBooks.SachId == sachId);

            if (comboBookDetail == null)
            {
                return NotFound();
            }

            comboBookDetail.QuantityBookInCombo = soLuongMoi;
            _context.SaveChanges();

            return RedirectToAction("ComboDetails", new { id = comboId });
        }
        [HttpPost]
        public IActionResult XoaSachTrongCombo(int comboId, int sachId)
        {
            var comboBook = _context.ComboBooks
                .FirstOrDefault(cb => cb.ComboId == comboId && cb.SachId == sachId);

            if (comboBook == null)
            {
                return NotFound();
            }

            _context.ComboBooks.Remove(comboBook);
            _context.SaveChanges();

            return RedirectToAction("ComboDetails", new { id = comboId });
        }
        public IActionResult ThemSachVaoCombo(int comboId)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }
            var combo = _context.Combos.FirstOrDefault(c => c.ComboId == comboId);
            if (combo == null)
            {
                return NotFound();
            }

            // Lấy danh sách sách chưa có trong combo
            var sachChuaCo = _context.Saches
                .Where(s => !_context.ComboBooks.Any(cb => cb.ComboId == comboId && cb.SachId == s.SachId))
                .Select(s => new
                {
                    SachId = s.SachId,
                    TenSach = s.TenSach
                })
                .ToList();

            ViewBag.ComboId = comboId;
            return View(sachChuaCo);
        }
        [HttpPost]
        public IActionResult ThemSachVaoCombo(int comboId, int sachId, int soLuong)
        {
            var combo = _context.Combos.FirstOrDefault(c => c.ComboId == comboId);
            if (combo == null)
            {
                return NotFound();
            }

            // Thêm sách vào combo
            var comboBook = new ComboBook
            {
                ComboId = comboId,
                SachId = sachId
            };

            _context.ComboBooks.Add(comboBook);
            _context.SaveChanges();

            // Thêm số lượng sách vào chi tiết combo
            var comboBookDetail = new ComboBookDetail
            {
                ComboBooksId = comboBook.ComboBooksId,
                QuantityBookInCombo = soLuong
            };

            _context.ComboBookDetails.Add(comboBookDetail);
            _context.SaveChanges();

            return RedirectToAction("ComboDetails", new { id = comboId });
        }
        public IActionResult OrderStatistics()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }
            // Lấy tất cả các đơn hàng từ cơ sở dữ liệu
            var orders = _context.DonHangs.ToList();

            // Tạo thống kê đơn hàng theo tháng
            var orderStats = orders
                .GroupBy(o => o.NgayDat.Day)
                .Select(g => new
                {
                    Day = g.Key,
                    TotalAmount = g.Sum(o => o.TongTien)
                })
                .OrderBy(o => o.Day)
                .ToList();

            // Truyền dữ liệu thống kê vào View
            return View(orderStats);
        }



    }
}

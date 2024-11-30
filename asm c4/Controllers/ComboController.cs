using asm_c4.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using X.PagedList;
using X.PagedList.Extensions;

namespace asm_c4.Controllers
{
    public class ComboController : Controller
    {
        private readonly QuanLySachContext _context;

        public ComboController(QuanLySachContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách combo
        public IActionResult Index(string searchQuery, string sortOrder, int? page)
        {
            int pageSize = 8; // Số combo trên mỗi trang
            int pageNumber = page ?? 1; // Trang hiện tại

            var combos = _context.Combos.Where(c => c.TrangThai == true);

            if (!string.IsNullOrEmpty(searchQuery))
            {
                combos = combos.Where(c => c.TenCombo.Contains(searchQuery));
                ViewBag.SearchQuery = searchQuery;
            }

            // Xử lý sắp xếp giá
            switch (sortOrder)
            {
                case "price_desc":
                    combos = combos.OrderByDescending(c => c.Gia);
                    break;
                case "price_asc":
                    combos = combos.OrderBy(c => c.Gia);
                    break;
                default:
                    combos = combos.OrderBy(c => c.ComboId); // Sắp xếp theo ID mặc định
                    break;
            }

            ViewBag.SortOrder = sortOrder;

            return View(combos.ToPagedList(pageNumber, pageSize));
        }

        // Hiển thị chi tiết combo (bao gồm danh sách sách trong combo)
        public IActionResult Details(int id)
        {
            var combo = _context.Combos
                .Where(c => c.ComboId == id && c.TrangThai)
                .Select(c => new
                {
                    Combo = c,
                    Books = c.ComboBooks.Select(cb => new
                    {
                        SachId = cb.Sach.SachId,
                        TenSach = cb.Sach.TenSach,
                        MoTa = cb.Sach.MoTa,
                        HinhAnh = cb.Sach.HinhAnh,
                        GiaTien = cb.Sach.GiaTien,
                        TrangThai = cb.Sach.TrangThai,
                        DanhMuc = cb.Sach.DanhMuc.TenDanhMuc, // Nếu bạn muốn hiển thị tên danh mục sách
                        QuantityInCombo = cb.ComboBookDetails
                            .Where(cbd => cbd.ComboBooksId == cb.ComboBooksId)
                            .Select(cbd => cbd.QuantityBookInCombo)
                            .FirstOrDefault() // Lấy số lượng sách trong combo
                    })
                })
                .FirstOrDefault();

            if (combo == null)
            {
                return NotFound();
            }

            ViewBag.Books = combo.Books;
            return View(combo.Combo);
        }
        [HttpPost]
        public IActionResult AddComboToCart(int comboId, int quantity)
        {
            // Kiểm tra combo có tồn tại và trạng thái hoạt động hay không
            var combo = _context.Combos.FirstOrDefault(c => c.ComboId == comboId && c.TrangThai);
            if (combo == null || quantity <= 0)
            {
                return NotFound();
            }

            // Kiểm tra người dùng đăng nhập qua session
            var userSession = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(userSession))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin người dùng từ session
            var currentUser = JsonConvert.DeserializeObject<User>(userSession);
            int userId = currentUser.UserId;

            // Kiểm tra giỏ hàng của người dùng
            var gioHang = _context.GioHangs.FirstOrDefault(g => g.UserId == userId);
            if (gioHang == null)
            {
                // Tạo mới giỏ hàng nếu chưa có
                gioHang = new GioHang { UserId = userId };
                _context.GioHangs.Add(gioHang);
                _context.SaveChanges();
            }

            // Kiểm tra chi tiết giỏ hàng
            var existingItem = _context.GioHangChiTiets
                .FirstOrDefault(g => g.GioHangId == gioHang.GioHangId && g.ComboId == combo.ComboId);

            int totalQuantity = existingItem != null ? (existingItem.SoLuongCombo ?? 0) + quantity : quantity;


            // Kiểm tra số lượng combo nếu có giới hạn
            if (combo.Quantity.HasValue && totalQuantity > combo.Quantity.Value)
            {
                ViewBag.ErrorMessage = $"Bạn chỉ có thể thêm tối đa {combo.Quantity.Value - (existingItem?.SoLuongCombo ?? 0)} combo nữa.";
                return RedirectToAction("Index", "Cart"); // Chuyển hướng đến trang giỏ hàng với thông báo lỗi
            }

            if (existingItem != null)
            {
                // Nếu combo đã tồn tại, cập nhật số lượng
                existingItem.SoLuongCombo = totalQuantity;
            }
            else
            {
                // Nếu combo chưa tồn tại, thêm mới vào chi tiết giỏ hàng
                var gioHangChiTiet = new GioHangChiTiet
                {
                    GioHangId = gioHang.GioHangId,
                    ComboId = combo.ComboId,
                    SoLuongCombo = quantity,
                    DonGia = combo.Gia
                };

                _context.GioHangChiTiets.Add(gioHangChiTiet);
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            _context.SaveChanges();

            return RedirectToAction("Index", "Cart");
        }


    }
}


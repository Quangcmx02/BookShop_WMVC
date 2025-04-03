using asm_c4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;

namespace asm_c4.Controllers
{
    public class CartController : Controller
    {
        private readonly QuanLySachContext _context;

        public CartController(QuanLySachContext context)
        {
            _context = context;
        }

        // Hiển thị giỏ hàng
        public IActionResult Index()
        {
            var userSession = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(userSession))
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = JsonConvert.DeserializeObject<User>(userSession);
            int userId = currentUser.UserId;

            var cart = _context.GioHangs
                .Include(g => g.GioHangChiTiets)
                    .ThenInclude(g => g.Sach)
                .Include(g => g.GioHangChiTiets)
                    .ThenInclude(g => g.Combo) // Thêm phần này để include Combo
                .FirstOrDefault(g => g.UserId == userId);

            if (cart == null)
            {
                return View(new List<GioHangChiTiet>());
            }

            // Trả về giỏ hàng, có thể bao gồm cả sách và combo
            return View(cart.GioHangChiTiets);
        }

        [HttpPost]
        public IActionResult UpdateCart([FromBody] UpdateCartRequest request)
        {
            var userSession = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(userSession))
            {
                return Json(new { success = false, message = "Bạn chưa đăng nhập!" });
            }

            var currentUser = JsonConvert.DeserializeObject<User>(userSession);
            int userId = currentUser.UserId;

            var cart = _context.GioHangs
                .Include(g => g.GioHangChiTiets)
                    .ThenInclude(g => g.Sach)
                .Include(g => g.GioHangChiTiets)
                    .ThenInclude(g => g.Combo)
                .FirstOrDefault(g => g.UserId == userId);

            if (cart == null)
            {
                return Json(new { success = false, message = "Giỏ hàng không tồn tại." });
            }

            GioHangChiTiet? cartItem;
            if (request.IsCombo)
            {
                cartItem = cart.GioHangChiTiets.FirstOrDefault(c => c.ComboId == request.Id);
            }
            else
            {
                cartItem = cart.GioHangChiTiets.FirstOrDefault(c => c.SachId == request.Id);
            }

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng." });
            }

            if (request.QuantitySach > 0 || request.QuantityCombo > 0)
            {
                if (request.IsCombo)
                {
                    var combo = _context.Combos.FirstOrDefault(c => c.ComboId == request.Id);
                    if (combo != null)
                    {
                        if (request.QuantityCombo > (combo.Quantity ?? 0))
                        {
                            request.QuantityCombo = combo.Quantity ?? 0;
                            return Json(new { success = false, message = "Số lượng combo vượt quá giới hạn!" });
                        }
                        cartItem.SoLuongCombo = request.QuantityCombo;
                    }
                }
                else
                {
                    var product = _context.Saches.FirstOrDefault(p => p.SachId == request.Id);
                    if (product != null)
                    {
                        if (request.QuantitySach > product.SoLuong)
                        {
                            request.QuantitySach = product.SoLuong;
                            return Json(new { success = false, message = "Số lượng sách vượt quá giới hạn!" });
                        }
                        cartItem.SoLuongSach = request.QuantitySach;
                    }
                }
            }
            else
            {
                _context.GioHangChiTiets.Remove(cartItem);
            }

            _context.SaveChanges();
            return Json(new { success = true });
        }

        public class UpdateCartRequest
        {
            public int Id { get; set; }
            public int QuantityCombo { get; set; }
            public int QuantitySach { get; set; }
            public bool IsCombo { get; set; }
        }


        [HttpPost]
        public IActionResult RemoveFromCart(int id, bool isCombo = false)
        {
            // Kiểm tra session người dùng
            var userSession = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(userSession))
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = JsonConvert.DeserializeObject<User>(userSession);
            int userId = currentUser.UserId;

            // Lấy giỏ hàng của người dùng
            var cart = _context.GioHangs
                .Include(g => g.GioHangChiTiets)
                    .ThenInclude(g => g.Sach)
                .Include(g => g.GioHangChiTiets)
                    .ThenInclude(g => g.Combo) // Thêm phần này để include Combo
                .FirstOrDefault(g => g.UserId == userId);

            if (cart == null)
            {
                ViewBag.ErrorMessage = "Giỏ hàng không tồn tại.";
                return RedirectToAction("Index");
            }

            // Tìm mục giỏ hàng cần xóa
            GioHangChiTiet? cartItem = null;

            if (isCombo)
            {
                cartItem = cart.GioHangChiTiets.FirstOrDefault(c => c.ComboId == id);
            }
            else
            {
                cartItem = cart.GioHangChiTiets.FirstOrDefault(c => c.SachId == id);
            }

            if (cartItem == null)
            {
                ViewBag.ErrorMessage = "Không tìm thấy sản phẩm hoặc combo cần xóa trong giỏ hàng.";
                return RedirectToAction("Index");
            }

            // Xóa mục khỏi giỏ hàng
            _context.GioHangChiTiets.Remove(cartItem);
            _context.SaveChanges();

            ViewBag.SuccessMessage = "Sản phẩm đã được xóa khỏi giỏ hàng.";
            return RedirectToAction("Index");
        }
        [HttpPost]
        [HttpPost]
        public IActionResult PlaceOrder(List<int> selectedItems, List<int> selectedComboItems)
        {
            var userSession = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(userSession))
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = JsonConvert.DeserializeObject<User>(userSession);
            int userId = currentUser.UserId;

            var cart = _context.GioHangs
                .Where(gh => gh.UserId == userId)
                .Select(gh => new
                {
                    GioHangId = gh.GioHangId,
                    CartItems = gh.GioHangChiTiets.Select(ghct => new
                    {
                        ghct.GioHangChiTietId,
                        ghct.SachId,
                        ghct.SoLuongSach,
                        ghct.ComboId,
                        ghct.SoLuongCombo,
                        ghct.DonGia
                    }).ToList()
                })
                .FirstOrDefault();

            if (cart == null || (selectedItems == null && selectedComboItems == null) ||
                (!selectedItems.Any() && !selectedComboItems.Any()))
            {
                ViewBag.ErrorMessage = "Vui lòng chọn ít nhất một sản phẩm để đặt hàng.";
                return RedirectToAction("Index");
            }

            // Tính tổng tiền và tạo danh sách chi tiết đơn hàng
            decimal totalAmount = 0;
            var orderDetails = new List<DonHangChiTiet>();

            // Xử lý sách được chọn
            foreach (var itemId in selectedItems)
            {
                var cartItem = cart.CartItems.FirstOrDefault(ci =>
                    ci.SachId.HasValue && ci.SachId.Value == itemId);

                if (cartItem != null)
                {
                    var book = _context.Saches.FirstOrDefault(s => s.SachId == cartItem.SachId);
                    if (book != null)
                    {
                        decimal itemTotal = cartItem.SoLuongSach.Value * cartItem.DonGia;
                        totalAmount += itemTotal;

                        orderDetails.Add(new DonHangChiTiet
                        {
                            SachId = cartItem.SachId,
                            SoLuongSach = cartItem.SoLuongSach,
                            DonGia = cartItem.DonGia,
                            ThanhTien = itemTotal
                        });

                        // Cập nhật số lượng sách trong kho
                        book.SoLuong -= cartItem.SoLuongSach.Value;
                        _context.Saches.Update(book);
                    }
                }
            }

            // Xử lý combo được chọn
            // Xử lý combo được chọn
            foreach (var itemId in selectedComboItems)
            {
                var cartItem = cart.CartItems.FirstOrDefault(ci =>
                    ci.ComboId.HasValue && ci.ComboId.Value == itemId);

                if (cartItem != null)
                {
                    var combo = _context.Combos.FirstOrDefault(c => c.ComboId == cartItem.ComboId);
                    if (combo != null)
                    {
                        decimal itemTotal = cartItem.SoLuongCombo.Value * cartItem.DonGia;
                        totalAmount += itemTotal;

                        orderDetails.Add(new DonHangChiTiet
                        {
                            ComboId = cartItem.ComboId,
                            SoLuongCombo = cartItem.SoLuongCombo,
                            DonGia = cartItem.DonGia,
                            ThanhTien = itemTotal
                        });

                        // Cập nhật số lượng combo
                        combo.Quantity -= cartItem.SoLuongCombo.Value;
                        _context.Combos.Update(combo);

                        // Lấy danh sách sách trong combo
                        var comboBooks = _context.ComboBooks
                            .Where(cb => cb.ComboId == cartItem.ComboId)
                            .ToList();

                        foreach (var comboBook in comboBooks)
                        {
                            // Lấy chi tiết combo (QuantityBookInCombo)
                            var comboBookDetail = _context.ComboBookDetails
                                .FirstOrDefault(cbd => cbd.ComboBooksId == comboBook.ComboBooksId);

                            if (comboBookDetail != null)
                            {
                                var book = _context.Saches.FirstOrDefault(s => s.SachId == comboBook.SachId);
                                if (book != null)
                                {
                                    // Trừ số lượng sách
                                    int totalBooksToSubtract = cartItem.SoLuongCombo.Value * comboBookDetail.QuantityBookInCombo;
                                    book.SoLuong -= totalBooksToSubtract;

                                    if (book.SoLuong < 0)
                                    {
                                        // Xử lý nếu sách không đủ số lượng
                                        ViewBag.ErrorMessage = $"Không đủ sách {book.TenSach} để đặt hàng combo.";
                                        return RedirectToAction("Index");
                                    }

                                    _context.Saches.Update(book);
                                }
                            }
                        }
                    }
                }
            }


            // Tạo đơn hàng mới
            var order = new DonHang
            {
                UserId = userId,
                NgayDat = DateTime.Now,
                TongTien = totalAmount,
                TrangThai = "Chờ xác nhận",
                DonHangChiTiets = orderDetails
            };

            _context.DonHangs.Add(order);

            // Xóa các sản phẩm đã đặt từ giỏ hàng
            foreach (var itemId in selectedItems.Concat(selectedComboItems))
            {
                var cartItem = cart.CartItems.FirstOrDefault(ci =>
                    (ci.SachId.HasValue && ci.SachId.Value == itemId) ||
                    (ci.ComboId.HasValue && ci.ComboId.Value == itemId));

                if (cartItem != null)
                {
                    _context.GioHangChiTiets.Remove(
                        _context.GioHangChiTiets.FirstOrDefault(ghct => ghct.GioHangChiTietId == cartItem.GioHangChiTietId)
                    );
                }
            }

            _context.SaveChanges();

            // Chuyển hướng tới trang thông báo đã đặt hàng thành công
            return RedirectToAction("OrderConfirmation");
        }







        public IActionResult OrderConfirmation()
        {
            return View();
        }
    }
}

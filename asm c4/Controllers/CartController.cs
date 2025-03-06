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
        public IActionResult UpdateCart(int id, int quantityCombo,int quantitySach, bool isCombo = false)
        {
            var userSession = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(userSession))
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = JsonConvert.DeserializeObject<User>(userSession);
            int userId = currentUser.UserId;

            // Lấy giỏ hàng của người dùng hiện tại
            var cart = _context.GioHangs
                .Include(g => g.GioHangChiTiets)
                    .ThenInclude(g => g.Sach)
                .Include(g => g.GioHangChiTiets)
                    .ThenInclude(g => g.Combo)
                .FirstOrDefault(g => g.UserId == userId);

            if (cart == null)
            {
                ViewBag.ErrorMessage = "Giỏ hàng không tồn tại.";
                return RedirectToAction("Index");
            }

            // Tìm mục giỏ hàng cần cập nhật
            GioHangChiTiet? cartItem;
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
                ViewBag.ErrorMessage = "Không tìm thấy mục cần cập nhật trong giỏ hàng.";
                return RedirectToAction("Index");
            }

            if (quantitySach > 0|| quantityCombo>0)
            {
                if (isCombo)
                {
                    var combo = _context.Combos.FirstOrDefault(c => c.ComboId == id);
                    if (combo != null)
                    {
                        if (quantityCombo > (combo.Quantity ?? 0))
                        {
                            quantityCombo = combo.Quantity ?? 0;
                            ViewBag.ErrorMessage = "Số lượng yêu cầu vượt quá số lượng combo có sẵn.";
                        }
                        cartItem.SoLuongCombo = quantityCombo; // Đúng thuộc tính
                    }
                }
                else
                {
                    var product = _context.Saches.FirstOrDefault(p => p.SachId == id);
                    if (product != null)
                    {
                        if (quantitySach > product.SoLuong)
                        {
                            quantitySach = product.SoLuong;
                            ViewBag.ErrorMessage = "Số lượng yêu cầu vượt quá số lượng có sẵn.";
                        }
                        cartItem.SoLuongSach = quantitySach;
                    }
                }

            }
            else
            {
                // Xóa mục khỏi giỏ hàng nếu số lượng là 0
                _context.GioHangChiTiets.Remove(cartItem);
            }

            // Lưu thay đổi
            _context.SaveChanges();

            return RedirectToAction("Index");
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

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
        public IActionResult UpdateCart(int id, int quantity, bool isCombo = false)
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

            if (quantity > 0)
            {
                if (isCombo)
                {
                    // Cập nhật số lượng combo
                    var combo = _context.Combos.FirstOrDefault(c => c.ComboId == id);
                    if (combo != null)
                    {
                        if (quantity > (combo.Quantity ?? 0))
                        {
                            quantity = combo.Quantity ?? 0;
                            ViewBag.ErrorMessage = "Số lượng yêu cầu vượt quá số lượng combo có sẵn.";
                        }
                        cartItem.SoLuong = quantity;
                    }
                }
                else
                {
                    // Cập nhật số lượng sách
                    var product = _context.Saches.FirstOrDefault(p => p.SachId == id);
                    if (product != null)
                    {
                        if (quantity > product.SoLuong)
                        {
                            quantity = product.SoLuong;
                            ViewBag.ErrorMessage = "Số lượng yêu cầu vượt quá số lượng có sẵn.";
                        }
                        cartItem.SoLuong = quantity;
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

        //[HttpPost]
        //public IActionResult PlaceOrder(List<int> selectedItems)
        //{
        //    var userSession = HttpContext.Session.GetString("UserSession");
        //    if (string.IsNullOrEmpty(userSession))
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    var currentUser = JsonConvert.DeserializeObject<User>(userSession);
        //    int userId = currentUser.UserId;

        //    var cart = _context.GioHangs
        //   .Where(gh => gh.UserId == userId)
        //   .Select(gh => new
        //   {
        //       GioHangId = gh.GioHangId,
        //       User = gh.User,
        //       CartItems = gh.GioHangChiTiets.Select(ghct => new
        //       {
        //           GioHangChiTietId = ghct.GioHangChiTietId,
        //           Sach = ghct.Sach != null ? new
        //           {
        //               SachId = ghct.Sach.SachId,
        //               TenSach = ghct.Sach.TenSach,
        //               GiaTien = ghct.Sach.GiaTien,
        //               MoTa = ghct.Sach.MoTa,
        //               HinhAnh = ghct.Sach.HinhAnh
        //           } : null,
        //           Combo = ghct.Combo != null ? new
        //           {
        //               ComboId = ghct.Combo.ComboId,
        //               TenCombo = ghct.Combo.TenCombo,
        //               Gia = ghct.Combo.Gia,
        //               MoTa = ghct.Combo.MoTa,
        //               LinkImages = ghct.Combo.LinkImages,
        //               ComboBooks = ghct.Combo.ComboBooks.Select(cb => new
        //               {
        //                   ComboBooksId = cb.ComboBooksId,
        //                   SachId = cb.Sach.SachId,
        //                   TenSach = cb.Sach.TenSach,
        //                   QuantityBookInCombo = cb.ComboBookDetails.FirstOrDefault().QuantityBookInCombo
        //               })
        //           } : null,
        //           SoLuong = ghct.SoLuong,
        //           DonGia = ghct.DonGia
        //       })
        //   })
        //   .FirstOrDefault();

        //    if (cart == null || selectedItems == null || !selectedItems.Any())
        //    {
        //        ViewBag.ErrorMessage = "Vui lòng chọn ít nhất một sản phẩm để đặt hàng.";
        //        return RedirectToAction("Index");
        //    }

        //    // Tính tổng tiền cho đơn hàng
        //    decimal totalAmount = 0;
        //    var orderDetails = new List<DonHangChiTiet>();

        //    foreach (var item in cart.GioHangChiTiets.Where(c => c.SachId.HasValue && selectedItems.Contains(c.SachId.Value)))
        //    {
        //        var product = _context.Saches.FirstOrDefault(p => p.SachId == item.SachId);
        //        if (product != null)
        //        {
        //            decimal itemTotal = item.SoLuong * item.DonGia;
        //            totalAmount += itemTotal;

        //            // Thêm chi tiết đơn hàng
        //            orderDetails.Add(new DonHangChiTiet
        //            {
        //                SachId = item.SachId.Value, // Đảm bảo SachId có giá trị
        //                SoLuong = item.SoLuong,
        //                DonGia = item.DonGia,
        //                ThanhTien = itemTotal
        //            });

        //            // Cập nhật số lượng sản phẩm trong kho
        //            product.SoLuong -= item.SoLuong; // Giảm số lượng sản phẩm trong kho
        //            _context.Saches.Update(product); // Cập nhật vào cơ sở dữ liệu
        //        }
        //    }

        //    // Tạo đơn hàng mới
        //    var order = new DonHang
        //    {
        //        UserId = userId,
        //        NgayDat = DateTime.Now,
        //        TongTien = totalAmount,
        //        TrangThai = "Chờ xác nhận", // Bạn có thể thay đổi trạng thái tùy theo yêu cầu
        //        DonHangChiTiets = orderDetails
        //    };

        //    // Lưu đơn hàng và các chi tiết
        //    _context.DonHangs.Add(order);
        //    _context.SaveChanges();

        //    // Xóa các sản phẩm đã đặt từ giỏ hàng
        //    foreach (var item in cart.GioHangChiTiets.Where(c => c.SachId.HasValue && selectedItems.Contains(c.SachId.Value)).ToList())
        //    {
        //        _context.GioHangChiTiets.Remove(item);
        //    }

        //    _context.SaveChanges();

        //    // Chuyển hướng tới trang thông báo đã đặt hàng thành công
        //    return RedirectToAction("OrderConfirmation");
        //}





        public IActionResult OrderConfirmation()
        {
            return View();
        }
    }
}

using asm_c4.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using X.PagedList;
using X.PagedList.Extensions;

namespace asm_c4.Controllers
{
    public class ProductController : Controller
    {
        private readonly QuanLySachContext _context;

        public ProductController(QuanLySachContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách sách cho khách hàng
        public IActionResult Index(string searchQuery, string sortOrder, int? page)
        {
            int pageSize = 8; // Số sản phẩm trên mỗi trang
            int pageNumber = page ?? 1; // Trang hiện tại

            var products = _context.Saches
                .Where(s => s.TrangThai == true);

            if (!string.IsNullOrEmpty(searchQuery))
            {
                // Tìm kiếm theo tên sách, giá tiền, hoặc tên danh mục
                products = products.Where(s =>
                    s.TenSach.Contains(searchQuery) || // Tìm theo tên sách
                    s.GiaTien.ToString().Contains(searchQuery) || // Tìm theo giá (chuỗi)
                    (s.DanhMuc != null && s.DanhMuc.TenDanhMuc.Contains(searchQuery)) // Tìm theo tên danh mục
                );

                ViewBag.SearchQuery = searchQuery;
            }

            // Xử lý sắp xếp giá
            switch (sortOrder)
            {
                case "price_desc":
                    products = products.OrderByDescending(s => s.GiaTien);
                    break;
                case "price_asc":
                    products = products.OrderBy(s => s.GiaTien);
                    break;
                default:
                    products = products.OrderBy(s => s.SachId); // Sắp xếp theo ID mặc định
                    break;
            }

            ViewBag.SortOrder = sortOrder;

            return View(products.ToPagedList(pageNumber, pageSize));
        }


        // Hiển thị chi tiết sách
        public IActionResult Details(int id)
        {
            var product = _context.Saches.FirstOrDefault(p => p.SachId == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        private bool SachExists(int id)
        {
            return _context.Saches.Any(e => e.SachId == id);
        }
        // Thêm vào giỏ hàng


        [HttpPost]
        public IActionResult AddToCart(int id, int quantity)
        {
            var product = _context.Saches.FirstOrDefault(p => p.SachId == id);
            if (product == null || quantity <= 0)
            {
                return NotFound();
            }

            var userSession = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(userSession))
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = JsonConvert.DeserializeObject<User>(userSession);
            int userId = currentUser.UserId;

            // Kiểm tra xem giỏ hàng đã tồn tại chưa
            var gioHang = _context.GioHangs.FirstOrDefault(g => g.UserId == userId);
            if (gioHang == null)
            {
                // Tạo giỏ hàng mới
                gioHang = new GioHang { UserId = userId };
                _context.GioHangs.Add(gioHang);
                _context.SaveChanges(); // Lưu giỏ hàng mới vào cơ sở dữ liệu
            }

            // Lưu chi tiết giỏ hàng
            var existingItem = _context.GioHangChiTiets
                .FirstOrDefault(g => g.GioHangId == gioHang.GioHangId && g.SachId == product.SachId);

            int totalQuantity = existingItem != null ? (existingItem.SoLuongSach ?? 0) + quantity : quantity;


            // Kiểm tra xem tổng số lượng có vượt quá số lượng trong kho không
            if (totalQuantity > product.SoLuong)
            {
                ViewBag.ErrorMessage = $"Bạn chỉ có thể thêm tối đa {product.SoLuong - (existingItem?.SoLuongSach ?? 0)} sản phẩm nữa.";
                return RedirectToAction("Index", "Cart"); // Chuyển hướng đến trang giỏ hàng với thông báo lỗi
            }

            if (existingItem != null)
            {
                // Nếu sản phẩm đã có trong giỏ hàng, cập nhật số lượng
                existingItem.SoLuongSach = totalQuantity;
            }
            else
            {
                // Nếu không, thêm sản phẩm mới vào giỏ hàng
                var gioHangChiTiet = new GioHangChiTiet
                {
                    GioHangId = gioHang.GioHangId,
                    SachId = product.SachId,
                    SoLuongSach = quantity,
                    DonGia = product.GiaTien
                };

                _context.GioHangChiTiets.Add(gioHangChiTiet);
            }

            _context.SaveChanges(); // Lưu tất cả thay đổi vào cơ sở dữ liệu

            return RedirectToAction("Index","Cart");
        }




    }
}

using asm_c4.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace asm_c4.Controllers
{
    public class AccountController : Controller
    {
        private readonly QuanLySachContext _context;

        public AccountController(QuanLySachContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                HttpContext.Session.SetString("UserSession", JsonConvert.SerializeObject(user));

                if (user.VaiTro == "Admin")
                {
                    return RedirectToAction("Index", "Home");
                }
                else if (user.VaiTro == "User")
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Message = "Tên đăng nhập hoặc mật khẩu không đúng!";
            return View();
        }

        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback"),
                Items = { { "LoginProvider", GoogleDefaults.AuthenticationScheme } }
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCallback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
            {
                return RedirectToAction("Login");
            }

            var emailClaim = authenticateResult.Principal.FindFirst(ClaimTypes.Email);
            var nameClaim = authenticateResult.Principal.FindFirst(ClaimTypes.Name);

            if (emailClaim == null || nameClaim == null)
            {
                return RedirectToAction("Login");
            }

            var email = emailClaim.Value;
            var name = nameClaim.Value;

            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                // Nếu người dùng chưa tồn tại, tạo mới với vai trò mặc định
                user = new User
                {
                    Username = name,
                    Email = email,
                    Password = "GOOGLE_AUTH",
                    VaiTro = "User"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // Lưu thông tin vào Session
            HttpContext.Session.SetString("UserSession", JsonConvert.SerializeObject(user));

            return RedirectToAction("Index", "Product");
        }

        public ActionResult Index()
        {
            var userSession = HttpContext.Session.GetString("UserSession");

            if (string.IsNullOrEmpty(userSession))
            {
                return Content("Bạn chưa đăng nhập.");
            }

            var currentUser = JsonConvert.DeserializeObject<User>(userSession);

            if (currentUser == null || currentUser.VaiTro != "Admin")
            {
                return Content("Bạn không có quyền xem danh sách này.");
            }

            return View(_context.Users);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa session
            return SignOut(new AuthenticationProperties { RedirectUri = "/" }, CookieAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string password, string confirmPassword, string email, int phone)
        {
            if (password != confirmPassword)
            {
                ViewBag.Message = "Mật khẩu và xác nhận mật khẩu không khớp!";
                return View();
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.Username == username);
            if (existingUser != null)
            {
                ViewBag.Message = "Tên đăng nhập đã tồn tại!";
                return View();
            }

            var newUser = new User
            {
                Username = username,
                Password = password,
                Email = email,
                Phone = phone,
                VaiTro = "User"
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            ViewBag.Message = "Đăng ký thành công! Bạn có thể đăng nhập ngay.";
            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult AccountManage()
        {
            // Lấy thông tin người dùng từ session
            var userSession = HttpContext.Session.GetString("UserSession");

            if (string.IsNullOrEmpty(userSession))
            {
                return RedirectToAction("Login");
            }

            var currentUser = JsonConvert.DeserializeObject<User>(userSession);

            if (currentUser == null)
            {
                return RedirectToAction("Login");
            }

            // Trả về View và truyền dữ liệu người dùng hiện tại
            return View(currentUser);
        }

        [HttpPost]
        public IActionResult AccountManage(User updatedUser, string oldPassword, string newPassword, string confirmNewPassword)
        {
            // Lấy thông tin người dùng từ session
            var userSession = HttpContext.Session.GetString("UserSession");

            if (string.IsNullOrEmpty(userSession))
            {
                return RedirectToAction("Login");
            }

            var currentUser = JsonConvert.DeserializeObject<User>(userSession);

            if (currentUser == null)
            {
                return RedirectToAction("Login");
            }

            // Kiểm tra mật khẩu cũ trước khi cập nhật mật khẩu mới
            if (!string.IsNullOrEmpty(newPassword))
            {
                if (currentUser.Password != oldPassword)
                {
                    ViewBag.Message = "Mật khẩu cũ không đúng!";
                    return View(currentUser);
                }

                if (newPassword != confirmNewPassword)
                {
                    ViewBag.Message = "Mật khẩu mới và xác nhận mật khẩu không khớp!";
                    return View(currentUser);
                }

                // Cập nhật mật khẩu mới
                currentUser.Password = newPassword;
            }

            // Cập nhật thông tin số điện thoại nếu có thay đổi
            if (updatedUser.Phone != currentUser.Phone)
            {
                currentUser.Phone = updatedUser.Phone;
            }

            // Lưu thay đổi vào database
            _context.Users.Update(currentUser);
            _context.SaveChanges();

            // Cập nhật lại session với thông tin người dùng mới
            HttpContext.Session.SetString("UserSession", JsonConvert.SerializeObject(currentUser));

            ViewBag.Message = "Cập nhật thông tin thành công!";
            return View(currentUser);
        }




    }


}


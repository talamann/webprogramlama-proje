using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebProgramlama.Data.Interfaces;
using WebProgramlama.Data.Repositories;
using WebProgramlama.Models;

namespace WebProgramlama.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IStudentRepository _studentRepository;
        private readonly ITeacherRepository _teacherRepository;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, IStudentRepository studentRepository, ITeacherRepository teacherRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _studentRepository = studentRepository;
            _teacherRepository = teacherRepository;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    // Handle return URL FIRST (for [Authorize] redirects)
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    // Role-based redirection for direct login
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        if (roles.Contains("Student"))
                        {
                            return RedirectToAction("Index", "Student");
                        }
                        else if (roles.Contains("Teacher"))
                        {
                            return RedirectToAction("Index", "Teacher");
                        }
                    }

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Kullanıcı tipine göre rol atama
                    string role = model.UserType == UserType.Student ? "Student" : "Teacher";
                    await _userManager.AddToRoleAsync(user, role);

                    if (role == "Student")
                    {
                        var student = new Student(user.Id, model.FirstName + " " + model.LastName);
                        await _studentRepository.AddAsync(student);

                        // Sign in and redirect to Student dashboard
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "Student");
                    }
                    else if (role == "Teacher")
                    {
                        var teacher = new Teacher(user.Id, model.FirstName + " " + model.LastName);
                        await _teacherRepository.AddAsync(teacher);

                        // Sign in and redirect to Teacher dashboard
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "Teacher");
                    }
                }

                // If user creation failed, add errors to ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Return the view with validation errors instead of BadRequest
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
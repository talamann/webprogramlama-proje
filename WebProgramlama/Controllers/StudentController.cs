using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebProgramlama.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Student/Index.cshtml");
        }
    }
}
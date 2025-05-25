using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;
using WebProgramlama.Data.Interfaces;
using WebProgramlama.Models;

namespace WebProgramlama.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IAssignmentRepository _assignmentRepository;

        public StudentController(IStudentRepository studentRepository, IAssignmentRepository assignmentRepository)
        {
            _studentRepository = studentRepository;
            _assignmentRepository = assignmentRepository;
        }
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userName = User.Identity.Name;
            var assignments = await _assignmentRepository.GetByStudentIdAsync(userId);
            var a = new StudentDashboardViewModel();
            

            // Use the userId for database queries
            var student = await _studentRepository.GetByIdAsync(userId);
            a.Student = student;
            a.Assignments = assignments;
            return View(a);


        }
    }
}
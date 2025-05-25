using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebProgramlama.Data.Interfaces;
using WebProgramlama.Data.Repositories;
using WebProgramlama.Models;

namespace WebProgramlama.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IAssignmentRepository _assignmentRepository;

        public TeacherController(ITeacherRepository teacherRepository, IAssignmentRepository assignmentRepository, IStudentRepository studentRepository)
        {
            _teacherRepository = teacherRepository;
            _assignmentRepository = assignmentRepository;
            _studentRepository = studentRepository;
        }
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userName = User.Identity.Name;
            var assignments = await _assignmentRepository.GetByTeacherIdAsync(userId);
            var a = new TeacherDashboardViewModel();


            // Use the userId for database queries
            var student = await _teacherRepository.GetByIdAsync(userId);
            a.Teacher = student;
            a.Assignments = assignments;
            return View(a);
        }
        public async Task<IActionResult> CreateAssignment()
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var students = await _studentRepository.GetAllAsync();

            var viewModel = new CreateAssignmentViewModel
            {
                TeacherId = teacherId,
                Students = students
            };

            return View(viewModel);
        }

        // POST: Create assignment and assign to all students
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssignment(string teacherId, string title, string description,
            DateTime dueDate, int maxGrade = 100)
        {
            try
            {
                // Get all students
                var students = await _studentRepository.GetAllAsync();

                if (!students.Any())
                {
                    TempData["Error"] = "Sistemde kayýtlý öðrenci bulunmamaktadýr.";
                    return RedirectToAction("CreateAssignment");
                }

                var createdAssignments = new List<Guid>();

                // Create assignment for each student
                foreach (var student in students)
                {
                    var assignmentId = Guid.NewGuid();

                    var assignment = new Assignment
                    {
                        Id = assignmentId,
                        Description = $"{title}: {description}",
                        TeacherId = teacherId,
                        StudentId = student.Id
                    };

                    // Save assignment to database
                    await _assignmentRepository.AddAsync(assignment);

                    // Create folder for this assignment in project root
                    await CreateAssignmentFolderAsync(assignmentId);

                    createdAssignments.Add(assignmentId);
                }

                TempData["Success"] = $"Ödev baþarýyla oluþturuldu ve {students.Count()} öðrenciye atandý.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ödev oluþturulurken bir hata oluþtu: {ex.Message}";
                return RedirectToAction("CreateAssignment");
            }
        }
        private async Task CreateAssignmentFolderAsync(Guid assignmentId)
        {
            try
            {
                // Get current directory (project root)
                var projectRoot = Directory.GetCurrentDirectory();

                // Or get from assembly location
                // var projectRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                var assignmentsPath = Path.Combine(projectRoot, "Assignments");
                if (!Directory.Exists(assignmentsPath))
                {
                    Directory.CreateDirectory(assignmentsPath);
                }

                var assignmentFolderPath = Path.Combine(assignmentsPath, assignmentId.ToString());
                Directory.CreateDirectory(assignmentFolderPath);

                // Create README
                var readmePath = Path.Combine(assignmentFolderPath, "README.txt");
                await System.IO.File.WriteAllTextAsync(readmePath,
                $"Assignment ID: {assignmentId}\n" +
                $"Created: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                $"This folder is for student assignment submissions.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Folder creation error: {ex.Message}");
            }
        }
    }
}
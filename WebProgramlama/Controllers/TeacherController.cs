using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebProgramlama.Data.Interfaces;
using WebProgramlama.Data.Repositories;
using WebProgramlama.Models;
using WebProgramlama.Services;

namespace WebProgramlama.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly FlaskApiService _flaskApiService;

        public TeacherController(ITeacherRepository teacherRepository, IAssignmentRepository assignmentRepository,
            IStudentRepository studentRepository, FlaskApiService flaskApiService)
        {
            _teacherRepository = teacherRepository;
            _assignmentRepository = assignmentRepository;
            _studentRepository = studentRepository;
            _flaskApiService = flaskApiService;
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
                    TempData["Error"] = "Sistemde kayıtlı öğrenci bulunmamaktadır.";
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

                TempData["Success"] = $"Ödev başarıyla oluşturuldu ve {students.Count()} öğrenciye atandı.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ödev oluşturulurken bir hata oluştu: {ex.Message}";
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

        // Flask API Methods

        // Check plagiarism for a specific assignment
        [HttpPost]
        public async Task<IActionResult> CheckPlagiarism(Guid assignmentId)
        {
            try
            {
                var assignmentFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Assignments", assignmentId.ToString());

                if (!Directory.Exists(assignmentFolderPath))
                {
                    return Json(new { success = false, message = "Assignment folder not found" });
                }

                // Check if Flask API is running
                bool flaskHealthy = await _flaskApiService.IsHealthyAsync();
                if (!flaskHealthy)
                {
                    return Json(new { success = false, message = "Flask API is not running" });
                }

                var result = await _flaskApiService.CheckFolderForPlagiarismAsync(assignmentFolderPath);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        hasPlagiarism = result.HasPlagiarism,
                        message = result.Message,
                        comparisons = result.Comparisons.Select(c => new {
                            file1 = c.File1,
                            file2 = c.File2,
                            similarity = c.Similarity
                        })
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Check AI content for a specific assignment
        [HttpPost]
        public async Task<IActionResult> CheckAiContent(Guid assignmentId)
        {
            try
            {
                var assignmentFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Assignments", assignmentId.ToString());

                if (!Directory.Exists(assignmentFolderPath))
                {
                    return Json(new { success = false, message = "Assignment folder not found" });
                }

                // Check if Flask API is running
                bool flaskHealthy = await _flaskApiService.IsHealthyAsync();
                if (!flaskHealthy)
                {
                    return Json(new { success = false, message = "Flask API is not running" });
                }

                var result = await _flaskApiService.CheckFolderForAiAsync(assignmentFolderPath);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        hasAiContent = result.HasAiContent,
                        message = result.Message,
                        results = result.Results.Select(r => new {
                            fileName = r.FileName,
                            humanProbability = r.HumanProbability,
                            aiProbability = r.AiProbability,
                            error = r.Error
                        })
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Check both plagiarism and AI content for all assignments
        [HttpPost]
        public async Task<IActionResult> CheckAllAssignments()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var assignments = await _assignmentRepository.GetByTeacherIdAsync(userId);

                // Check if Flask API is running
                bool flaskHealthy = await _flaskApiService.IsHealthyAsync();
                if (!flaskHealthy)
                {
                    return Json(new { success = false, message = "Flask API is not running" });
                }

                var results = new List<object>();

                foreach (var assignment in assignments)
                {
                    var assignmentFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Assignments", assignment.Id.ToString());

                    if (!Directory.Exists(assignmentFolderPath))
                        continue;

                    // Check plagiarism
                    var plagiarismResult = await _flaskApiService.CheckFolderForPlagiarismAsync(assignmentFolderPath);

                    // Check AI content
                    var aiResult = await _flaskApiService.CheckFolderForAiAsync(assignmentFolderPath);

                    results.Add(new
                    {
                        assignmentId = assignment.Id,
                        description = assignment.Description,
                        plagiarism = new
                        {
                            success = plagiarismResult.Success,
                            hasPlagiarism = plagiarismResult.HasPlagiarism,
                            comparisons = plagiarismResult.Comparisons.Count
                        },
                        aiContent = new
                        {
                            success = aiResult.Success,
                            hasAiContent = aiResult.HasAiContent,
                            filesChecked = aiResult.Results.Count
                        }
                    });
                }

                return Json(new { success = true, results = results });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Get assignment details with file list
        [HttpGet]
        public async Task<IActionResult> GetAssignmentDetails(Guid assignmentId)
        {
            try
            {
                var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
                if (assignment == null)
                {
                    return Json(new { success = false, message = "Assignment not found" });
                }

                var assignmentFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Assignments", assignmentId.ToString());

                var files = new List<object>();
                if (Directory.Exists(assignmentFolderPath))
                {
                    var txtFiles = Directory.GetFiles(assignmentFolderPath, "*.txt");
                    files = txtFiles.Select(f => new {
                        name = Path.GetFileName(f),
                        size = new FileInfo(f).Length,
                        created = new FileInfo(f).CreationTime
                    }).ToList<object>();
                }

                return Json(new
                {
                    success = true,
                    assignment = new
                    {
                        id = assignment.Id,
                        description = assignment.Description,
                        teacherId = assignment.TeacherId,
                        studentId = assignment.StudentId
                    },
                    files = files
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
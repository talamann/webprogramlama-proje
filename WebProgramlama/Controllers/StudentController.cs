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
        [HttpGet]
        public async Task<IActionResult> UploadAssignment(string studentid)
        {
            var assignments = await _assignmentRepository.GetByStudentIdAsync(studentid);
            
            return View(assignments);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAssignment(Guid assignmentId, IFormFile uploadedFile)
        {
            try
            {
                // Get current user ID first (we'll need it for redirects)
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "Kullanýcý bilgileri alýnamadý.";
                    return RedirectToAction("UploadAssignment", new { studentid = userId });
                }

                // Validate file
                if (uploadedFile == null || uploadedFile.Length == 0)
                {
                    TempData["Error"] = "Lütfen bir dosya seçiniz.";
                    return RedirectToAction("UploadAssignment", new { studentid = userId });
                }

                // Check if file is .txt
                if (!uploadedFile.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Error"] = "Sadece .txt dosyalarý yüklenebilir.";
                    return RedirectToAction("UploadAssignment", new { studentid = userId });
                }

                // Find the original assignment
                var originalAssignment = await _assignmentRepository.GetByIdAsync(assignmentId);
                if (originalAssignment == null)
                {
                    TempData["Error"] = "Ödev bulunamadý.";
                    return RedirectToAction("UploadAssignment", new { studentid = userId });
                }

                
                

                // Create a duplicate assignment for the student's submission
                var studentAssignment = new Assignment
                {
                    Id = Guid.NewGuid(),
                    Description = originalAssignment.Description + " (Gönderildi)", // Mark as submitted
                    TeacherId = originalAssignment.TeacherId,
                    StudentId = userId,
                    kopyaTestResult = false // Will be updated after plagiarism check
                };

                // Save the student's assignment copy
                await _assignmentRepository.AddAsync(studentAssignment);

                // Save the uploaded file to the existing assignment folder
                var projectRoot = Directory.GetCurrentDirectory();
                var assignmentFolderPath = Path.Combine(projectRoot, "Assignments", assignmentId.ToString());

                // Create filename with student ID and timestamp
                var fileName = $"{userId}_{DateTime.Now:yyyyMMdd_HHmmss}_{uploadedFile.FileName}";
                var filePath = Path.Combine(assignmentFolderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(stream);
                }

                TempData["Success"] = "Ödeviniz baþarýyla yüklendi.";
                return RedirectToAction("UploadAssignment", new { studentid = userId });
            }
            catch (Exception ex)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                TempData["Error"] = $"Ödev yüklenirken bir hata oluþtu: {ex.Message}";
                return RedirectToAction("UploadAssignment", new { studentid = userId });
            }
        }
    }
}
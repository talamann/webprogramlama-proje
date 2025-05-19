using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebProgramlama.Models;

namespace WebProgramlama.Data.Interfaces
{
    public interface IAssignmentRepository
    {
        Task<IEnumerable<Assignment>> GetAllAsync();
        Task<Assignment> GetByIdAsync(Guid id);
        Task<IEnumerable<Assignment>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<Assignment>> GetByTeacherIdAsync(int teacherId);
        Task AddAsync(Assignment assignment);
        Task UpdateAsync(Assignment assignment);
        Task DeleteAsync(Guid id);
    }
}
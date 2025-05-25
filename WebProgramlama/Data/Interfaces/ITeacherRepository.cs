using System.Collections.Generic;
using System.Threading.Tasks;
using WebProgramlama.Models;

namespace WebProgramlama.Data.Interfaces
{
    public interface ITeacherRepository
    {
        Task<IEnumerable<Teacher>> GetAllAsync();
        Task<Teacher> GetByIdAsync(string id);
        Task AddAsync(Teacher teacher);
        Task UpdateAsync(Teacher teacher);
        Task DeleteAsync(string id);
    }
}
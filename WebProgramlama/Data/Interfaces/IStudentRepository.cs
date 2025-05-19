﻿using System.Collections.Generic;
using System.Threading.Tasks;
using WebProgramlama.Models;

namespace WebProgramlama.Data.Interfaces
{
    public interface IStudentRepository
    {
        Task<IEnumerable<Student>> GetAllAsync();
        Task<Student> GetByIdAsync(int id);
        Task AddAsync(Student student);
        Task UpdateAsync(Student student);
        Task DeleteAsync(int id);
    }
}
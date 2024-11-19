using MauiApp2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp2.Services;

public interface IDatabaseService
{
    public Task<int> SaveStudentAsync(Student student);
    public Task<List<Student>> GetAllStudentsAsync();
    public Task<List<Student>> GetFilteredStudentsAsync(double threshold);
}

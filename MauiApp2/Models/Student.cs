using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp2.Models;

public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public double Subject1Grade { get; set; }
    public double Subject2Grade { get; set; }
    public string Address { get; set; } = string.Empty;

    public double AverageGrade => (Subject1Grade + Subject2Grade) / 2;
}

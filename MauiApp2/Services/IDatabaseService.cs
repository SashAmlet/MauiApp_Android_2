using MauiApp2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp2.Services;

public interface IDatabaseService<T>
{
    Task<int> SaveAsync(T entity);
    Task<List<T>> GetAllAsync();
}


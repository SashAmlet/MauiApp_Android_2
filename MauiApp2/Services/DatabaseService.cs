using MauiApp2.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp2.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _databasePath;

    public DatabaseService()
    {
        _databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Students.db");
        CreateDatabase();
    }

    private void CreateDatabase()
    {
        try
        {
            using (var connection = new SqliteConnection($"Data Source={_databasePath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Students (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FullName TEXT NOT NULL,
                    Subject1Grade REAL NOT NULL,
                    Subject2Grade REAL NOT NULL,
                    Address TEXT NOT NULL
                )";
                command.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }

    public async Task<int> SaveStudentAsync(Student student)
    {
        try
        {
            using (var connection = new SqliteConnection($"Data Source={_databasePath}"))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                INSERT INTO Students (FullName, Subject1Grade, Subject2Grade, Address)
                VALUES ($fullName, $subject1Grade, $subject2Grade, $address)";
                command.Parameters.AddWithValue("$fullName", student.FullName);
                command.Parameters.AddWithValue("$subject1Grade", student.Subject1Grade);
                command.Parameters.AddWithValue("$subject2Grade", student.Subject2Grade);
                command.Parameters.AddWithValue("$address", student.Address);

                return await command.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            return 0;
        }
    }

    public async Task<List<Student>> GetAllStudentsAsync()
    {
        var students = new List<Student>();

        using (var connection = new SqliteConnection($"Data Source={_databasePath}"))
        {
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, FullName, Subject1Grade, Subject2Grade, Address FROM Students";

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    students.Add(new Student
                    {
                        Id = reader.GetInt32(0),
                        FullName = reader.GetString(1),
                        Subject1Grade = reader.GetDouble(2),
                        Subject2Grade = reader.GetDouble(3),
                        Address = reader.GetString(4)
                    });
                }
            }
        }

        return students;
    }

    public async Task<List<Student>> GetFilteredStudentsAsync(double threshold)
    {
        var filteredStudents = new List<Student>();

        using (var connection = new SqliteConnection($"Data Source={_databasePath}"))
        {
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
            SELECT Id, FullName, Subject1Grade, Subject2Grade, Address 
            FROM Students 
            WHERE (Subject1Grade + Subject2Grade) / 2 > $threshold";
            command.Parameters.AddWithValue("$threshold", threshold);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    filteredStudents.Add(new Student
                    {
                        Id = reader.GetInt32(0),
                        FullName = reader.GetString(1),
                        Subject1Grade = reader.GetDouble(2),
                        Subject2Grade = reader.GetDouble(3),
                        Address = reader.GetString(4)
                    });
                }
            }
        }

        return filteredStudents;
    }
}

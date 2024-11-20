using MauiApp2.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp2.Services;

public class DatabaseService<T> : IDatabaseService<T> where T : class, new()
{
    private readonly string _databasePath;

    public DatabaseService(string databaseName)
    {
        _databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{databaseName}.db");
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

                // Динамічне створення таблиці залежно від типу сутності
                if (typeof(T) == typeof(Student))
                {
                    command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Students (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        FullName TEXT NOT NULL,
                        Subject1Grade REAL NOT NULL,
                        Subject2Grade REAL NOT NULL,
                        Address TEXT NOT NULL
                    )";
                }
                else if (typeof(T) == typeof(UContact))
                {
                    command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS UContacts (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Phone TEXT NOT NULL,
                        Address TEXT NOT NULL
                    )";
                }

                command.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }

    public async Task<int> SaveAsync(T entity)
    {
        try
        {
            using (var connection = new SqliteConnection($"Data Source={_databasePath}"))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();

                if (entity is Student student)
                {
                    command.CommandText = @"
                    INSERT INTO Students (FullName, Subject1Grade, Subject2Grade, Address)
                    VALUES ($fullName, $subject1Grade, $subject2Grade, $address)";
                    command.Parameters.AddWithValue("$fullName", student.FullName);
                    command.Parameters.AddWithValue("$subject1Grade", student.Subject1Grade);
                    command.Parameters.AddWithValue("$subject2Grade", student.Subject2Grade);
                    command.Parameters.AddWithValue("$address", student.Address);
                }
                else if (entity is UContact contact)
                {
                    command.CommandText = @"
                    INSERT INTO UContacts (Name, Phone, Address)
                    VALUES ($name, $phone, $address)";
                    command.Parameters.AddWithValue("$name", contact.Name);
                    command.Parameters.AddWithValue("$phone", contact.Phone);
                    command.Parameters.AddWithValue("$address", contact.Address);
                }

                return await command.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            return 0;
        }
    }

    public async Task<List<T>> GetAllAsync()
    {
        var entities = new List<T>();

        using (var connection = new SqliteConnection($"Data Source={_databasePath}"))
        {
            await connection.OpenAsync();

            var command = connection.CreateCommand();

            if (typeof(T) == typeof(Student))
            {
                command.CommandText = "SELECT Id, FullName, Subject1Grade, Subject2Grade, Address FROM Students";
            }
            else if (typeof(T) == typeof(UContact))
            {
                command.CommandText = "SELECT Id, Name, Phone, Address FROM UContacts";
            }

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    if (typeof(T) == typeof(Student))
                    {
                        entities.Add(new Student
                        {
                            Id = reader.GetInt32(0),
                            FullName = reader.GetString(1),
                            Subject1Grade = reader.GetDouble(2),
                            Subject2Grade = reader.GetDouble(3),
                            Address = reader.GetString(4)
                        } as T);
                    }
                    else if (typeof(T) == typeof(UContact))
                    {
                        entities.Add(new UContact
                        {
                            Name = reader.GetString(1),
                            Phone = reader.GetString(2),
                            Address = reader.GetString(3)
                        } as T);
                    }
                }
            }
        }

        return entities;
    }
}


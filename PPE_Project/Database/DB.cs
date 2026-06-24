using PPE_Project.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using PPE_Project.Models;

namespace PPE_Project.Database
{
    public class DB
    {
        private static string dbPath = "ppe_system.db";                          // DB 파일 경로
        private static string connectionString = $"Data Source={dbPath};Version=3;";  // 연결 문자열

        public static void Initialize()
        {
            if (!File.Exists(dbPath))                                           // DB 파일이 없으면
            {
                SQLiteConnection.CreateFile(dbPath);                             // 새 DB 파일 생성
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string createEmployeesTable = @"
                    CREATE TABLE IF NOT EXISTS employees (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL,
                        position TEXT,
                        face_data BLOB
                    )";

                string createAttendanceTable = @"
                    CREATE TABLE IF NOT EXISTS attendance (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        employee_id INTEGER,
                        timestamp TEXT,
                        status TEXT,
                        ppe_status TEXT,
                        FOREIGN KEY (employee_id) REFERENCES employees(id)
                    )";

                using (var cmd = new SQLiteCommand(createEmployeesTable, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SQLiteCommand(createAttendanceTable, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void AddEmployee(string name, string position, byte[] faceData)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = "INSERT INTO employees (name, position, face_data) VALUES (@name, @position, @faceData)";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@position", position);
                    cmd.Parameters.AddWithValue("@faceData", faceData);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<Employee> GetEmployees()
        {
            var employees = new List<Employee>();   // 사원 목록을 담을 빈 리스트

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT id, name, position, face_data FROM employees";

                using (var cmd = new SQLiteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var employee = new Employee
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Position = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            FaceData = reader.IsDBNull(3) ? null : (byte[])reader[3]  // 얼굴 데이터 추가
                        };
                        employees.Add(employee);
                    }
                }
            }

            return employees;
        }

        public static void UpdateEmployee(int id, string name, string position)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = "UPDATE employees SET name = @name, position = @position WHERE id = @id";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@position", position);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteEmployee(int id)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = "DELETE FROM employees WHERE id = @id";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<(int id, byte[] faceData)> GetFaceData()
        {
            var result = new List<(int id, byte[] faceData)>();  // id랑 얼굴 데이터를 묶어서 담을 리스트

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT id, face_data FROM employees WHERE face_data IS NOT NULL";  // 얼굴 데이터 있는 사원만 조회

                using (var cmd = new SQLiteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        byte[] faceData = (byte[])reader[1];  // byte[]로 변환
                        result.Add((id, faceData));
                    }
                }
            }

            return result;
        }
    }
}
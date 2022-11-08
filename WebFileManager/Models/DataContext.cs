using MySqlConnector;
using ExcelDataReader;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace WebFileManager.Models
{
    /// <summary>
    /// Класс для чтения и записи данных из базы данных
    /// </summary>
    public class DataContext
    {
        public string ConnectionString { get; set; }

        public DataContext(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public List<FileModel> GetAllFileNames()
        {
            List<FileModel> list = new List<FileModel>();

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM files", conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new FileModel()
                        {
                            Id = reader.GetInt32("id"),
                            Filename = reader.GetString("filename"),
                        });
                    }
                }
            }

            return list;
        }

       
        public RenderModel RenderData(int file_id, int? curr_sheet = null, int? curr_class = null)
        {
            RenderModel renderModel = new RenderModel();
            renderModel.FileId = file_id;
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();

                string sql = "SELECT id, name, id_file FROM sheets WHERE id_file = " + file_id;
                MySqlCommand command = new MySqlCommand(sql, conn);
                MySqlDataReader reader = command.ExecuteReader();
                renderModel.Sheets = new List<SheetModel>();
                while (reader.Read())
                {
                    renderModel.Sheets.Add(new SheetModel()
                    {
                        Id = (int)reader[0],
                        Name = reader[1].ToString(),
                        IdInFile = (int)reader[2]
                    });
                }
                
                reader.Close(); 

                sql = "SELECT id, name, id_sheet FROM classes WHERE id_sheet = " + (curr_sheet ?? renderModel.Sheets[0].Id);
                command = new MySqlCommand(sql, conn);
                reader = command.ExecuteReader();
                renderModel.Classes = new List<ClassModel>();
                while (reader.Read())
                {
                    renderModel.Classes.Add(new ClassModel()
                    {
                        Id = (int)reader[0],
                        Name = reader[1].ToString(),
                        IdSheet = (int)reader[2]
                    });
                } 

                reader.Close();

                sql = "SELECT id_in_file, col1, col2, col3, col4, col5, col6, id_class FROM tbl WHERE id_class = " + (curr_class ?? renderModel.Classes[0].Id);
                command = new MySqlCommand(sql, conn);
                reader = command.ExecuteReader();
                renderModel.Rows = new List<RowModel>();
                while (reader.Read())
                {
                    renderModel.Rows.Add(new RowModel()
                    {
                        IdInFile = reader[0].ToString(),
                        Col1 = reader[1].ToString(),
                        Col2 = reader[2].ToString(),
                        Col3 = reader[3].ToString(),
                        Col4 = reader[4].ToString(),
                        Col5 = reader[5].ToString(),
                        Col6 = reader[6].ToString(),
                        IdClass = (int)reader[7]
                    });
                }
                
                reader.Close();
            }
            return renderModel;
        }
        //алгоритм можно оптимизировать, чтобы быстрее работал
        public void UploadFile(IFormFile file)
        {
            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                stream.Position = 0;
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    using (MySqlConnection conn = GetConnection())
                    {
                        conn.Open();
                        MySqlCommand comm = conn.CreateCommand();
                        //загружаем имя файла и запоминаем id
                        comm.CommandText = "INSERT INTO files(filename) VALUES(@_name)";
                        comm.Parameters.AddWithValue("@_name", file.FileName);
                        comm.ExecuteNonQuery();
                        comm.Parameters.Clear();

                        var last_file_id = comm.LastInsertedId;

                        do
                        {
                            //загружаем имя холста файла и запоминаем id
                            comm.CommandText = "INSERT INTO sheets(id_file, name) VALUES(@_id_file, @_name)";
                            comm.Parameters.AddWithValue("@_id_file", last_file_id);
                            comm.Parameters.AddWithValue("@_name", reader.Name);
                            comm.ExecuteNonQuery();
                            comm.Parameters.Clear();

                            var last_sheet_id = comm.LastInsertedId;
                            long? last_class_id = null;

                            while (reader.Read())
                            {
                                try
                                {
                                    var first_in_row = reader.GetValue(0).ToString();

                                    if (Regex.IsMatch(first_in_row, @"^(КЛАСС\s)")) 
                                    {
                                        //загружаем имя класс на холсте и запоминае id
                                        comm.CommandText = "INSERT INTO classes(id_sheet, name) VALUES(@_id_sheet, @_name)";
                                        comm.Parameters.AddWithValue("@_id_sheet", last_sheet_id);
                                        comm.Parameters.AddWithValue("@_name", first_in_row);
                                        comm.ExecuteNonQuery();
                                        last_class_id = comm.LastInsertedId;
                                        comm.Parameters.Clear();
                                    }
                                    else if (Regex.IsMatch(first_in_row, @"^(\d{4}|\d{2})$|(БАЛАНС)|(ПО\sКЛАССУ)"))
                                    {
                                        //загружаем строку класса
                                        comm.CommandText = "INSERT INTO tbl(id_class, id_in_file, col1, col2, col3, col4, col5, col6) VALUES(@_id_class, @_id_in_file, @_col1, @_col2, @_col3, @_col4, @_col5, @_col6)";
                                        comm.Parameters.AddWithValue("@_id_class", last_class_id);
                                        comm.Parameters.AddWithValue("@_id_in_file", first_in_row);
                                        comm.Parameters.AddWithValue("@_col1", reader.GetValue(1).ToString());
                                        comm.Parameters.AddWithValue("@_col2", reader.GetValue(2).ToString());
                                        comm.Parameters.AddWithValue("@_col3", reader.GetValue(3).ToString());
                                        comm.Parameters.AddWithValue("@_col4", reader.GetValue(4).ToString());
                                        comm.Parameters.AddWithValue("@_col5", reader.GetValue(3).ToString());
                                        comm.Parameters.AddWithValue("@_col6", reader.GetValue(4).ToString());
                                        comm.ExecuteNonQuery();
                                        comm.Parameters.Clear();
                                    }
                                }
                                catch
                                {
                                    //первый столбец не строчный - пропускаем
                                }
                                
                            }
                        } while (reader.NextResult());

                    }
                }
            }
        }
        
    }
}

using MySqlConnector;
using ExcelDataReader;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace WebFileManager.Models
{
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
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM webtest.files", conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new FileModel()
                        {
                            Filename = reader.GetString("filename"),
                        });
                    }
                }
            }

            return list;
        }

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
                        comm.CommandText = "INSERT INTO files(filename) VALUES(@name)";
                        comm.Parameters.AddWithValue("@name", file.FileName);
                        comm.ExecuteNonQuery();
                        var last_file_id = comm.LastInsertedId;

                        do
                        {
                            //загружаем имя холста файла и запоминаем id
                            comm.CommandText = "INSERT INTO sheets(id_file, name) VALUES(@id_file, @name)";
                            comm.Parameters.AddWithValue("@id_file", last_file_id);
                            comm.Parameters.AddWithValue("@name", reader.Name);
                            comm.ExecuteNonQuery();
                            var last_sheet_id = comm.LastInsertedId;

                            while (reader.Read())
                            {
                                try
                                {
                                    var first_in_row = reader.GetValue(0).ToString();
                                    if (Regex.IsMatch(first_in_row, @"\d{4}|\d{2}"))
                                    {

                                    }
                                    if (Regex.IsMatch(first_in_row, "ПО КЛАССУ"))
                                    {

                                    }
                                    if (Regex.IsMatch(first_in_row, "БАЛАНС"))
                                    {

                                    }
                                    if (Regex.IsMatch(first_in_row, "КЛАСС "))
                                    {

                                    }


                                }
                                catch
                                {
                                    //первый столбец не строчный - пропускаем
                                }
                                i++;
                            }
                        } while (reader.NextResult());

                    }
                }
            }
        }
        
    }
}

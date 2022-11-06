using MySqlConnector;

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
        
    }
}

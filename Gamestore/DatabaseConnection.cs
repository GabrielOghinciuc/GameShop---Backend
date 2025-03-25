using System.Data.SqlClient;
namespace Gamestore
{
    public class DatabaseConnection
    {
        private readonly string _connectionString;

        public DatabaseConnection()
        {
            _connectionString = "Server=DESKTOP-BV4NA5H;Database=YourDatabaseName;User Id=YourUsername;Password=YourPassword;";
        }

       
    }
}

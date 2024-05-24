using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WMS.views
{
    /// <summary>
    /// Interaction logic for workers_management.xaml
    /// </summary>
    public partial class workers_management : UserControl
    {
        string connectionString;
        public workers_management()
        {
            InitializeComponent();
            connectionString = GetDatabasePath();
            LoadData();
        }
        private void LoadData()
        {
            string query = "SELECT * FROM Users WHERE role <> 'admin'";
            using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                workersGrid.ItemsSource = reader; // Bind to DataGrid
            }
        }





        private string GetDatabasePath()
        {
            string databaseFileName = "users.db"; // Assuming the file is in the same directory as your app
            string currentDirectory = Directory.GetCurrentDirectory();
            string databasePath = System.IO.Path.Combine(currentDirectory, databaseFileName);

            if (!File.Exists(databasePath))
            {
                // Database file not found
                MessageBox.Show($"Database file not found at: {databasePath}");
                return null;
            }

            return $"Data Source={databasePath}";
        }
    }
}

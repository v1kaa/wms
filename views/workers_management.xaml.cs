using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
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
           // connectionString = GetDatabasePath();
            LoadData();
        }

        private void LoadData() //shows all workesr in table 
        {
            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT id,login,password,role FROM Users ";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        workersGrid.ItemsSource = dataTable.DefaultView; // Bind to DataGrid
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"problem while loading data : {ex.Message}");
            }
        }




        //private string GetDatabasePath() //returns a connection string for acces to database 
        //{
        //    string databaseFileName = "users.db"; // Assuming the file is in the same directory as your app
        //    string currentDirectory = Directory.GetCurrentDirectory();
        //    string databasePath = System.IO.Path.Combine(currentDirectory, databaseFileName);


        //    return $"Data Source={databasePath}";
        //}
    }
}

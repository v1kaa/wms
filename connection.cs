using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WMS
{
    public static class DatabaseConfig
    {
        public static string ConnectionString { get; private set; }

        static DatabaseConfig()
        {
            InitializeConnectionString();
        }

        private static void InitializeConnectionString()
        {
            string databaseFileName = "users.db"; // Assuming the file is in the same directory as your app
            string currentDirectory = Directory.GetCurrentDirectory();
            string databasePath = System.IO.Path.Combine(currentDirectory, databaseFileName);

            if (!File.Exists(databasePath))
            {
                // Database file not found
                MessageBox.Show($"Database file not found at: {databasePath}");
                ConnectionString = null;
                return;
            }

            ConnectionString = $"Data Source={databasePath}";
        }
    }


}

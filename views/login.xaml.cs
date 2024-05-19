using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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

using System.Runtime.InteropServices;
using System.Security;
using System.Net;

namespace WMS.views
{

    /// <summary>
    /// Interaction logic for login.xaml
    /// </summary>
    public partial class login : UserControl
    {
        string connectionString = "Data Source=G:\\c#\\WMS\\data\\users.db";
        public login()
        {
            InitializeComponent();

        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
           // AddNewUserToDatabase(UsernameTextBox.Text, PasswordBox.Password, RoleComboBox.Text);
            if (IfUserExist() && RoleComboBox.Text=="admin")
            {
                Window w = Window.GetWindow(this);
                w.Content=new admin_page();
            }
            else
            {
                MessageBox.Show(UsernameTextBox.Text +" "+ PasswordBox.Password +" "+ RoleComboBox.Text);
            }
               
        }
        private void AddNewUserToDatabase(string username, string password, string role)
        {
            string connectionString = "Data Source=G:\\c#\\WMS\\data\\users.db";
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string sql = "INSERT INTO users (login, password, role) VALUES (@login, @password, @role)";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    // Use parameters to prevent SQL injection
                    command.Parameters.AddWithValue("@login", username);
                    command.Parameters.AddWithValue("@password", password);
                    command.Parameters.AddWithValue("@role", role);

                    // Execute the command
                    int rowsAffected = command.ExecuteNonQuery();
                }
            }

        }

        private bool IfUserExist()
        {
            try
            {
                // Make sure connection string is correctly set
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT COUNT(*) FROM users WHERE login = @login AND password = @password AND role = @role";



                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        // Use parameters to prevent SQL injection
                        command.Parameters.AddWithValue("@login", UsernameTextBox.Text);
                        command.Parameters.AddWithValue("@password", PasswordBox.Password);
                        command.Parameters.AddWithValue("@role", RoleComboBox.Text);

                        // ExecuteScalar returns the first column of the first row in the result set returned by the query
                        int count = Convert.ToInt32(command.ExecuteScalar());

                        // Check if count is greater than 0, indicating user exists
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine("An error occurred: " + ex.Message);
                return false; // Or handle differently based on your application's logic
            }
        }









    }
}

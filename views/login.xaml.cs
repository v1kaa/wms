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
using System.IO;
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


        public login()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (IfUserExist() && RoleComboBox.Text == "Admin")
            {
                Window w = Window.GetWindow(this);
                w.Content = new admin_page();
            }
            else
            {
                MessageBox.Show(UsernameTextBox.Text + " " + PasswordBox.Password + " " + RoleComboBox.Text+ "dont have this data in database");
            }
        }

        private void AddNewUserToDatabase(string username, string password, string role)
        {
            using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
            {
                connection.Open();
                string sql = "INSERT INTO users (login, password, role) VALUES (@login, @password, @role)";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@login", username);
                    command.Parameters.AddWithValue("@password", password);
                    command.Parameters.AddWithValue("@role", role);

                    int rowsAffected = command.ExecuteNonQuery();
                }
            }
        }

        private bool IfUserExist()
        {
            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    string sql = "SELECT COUNT(*) FROM users WHERE login = @login AND password = @password AND role = @role";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@login", UsernameTextBox.Text);
                        command.Parameters.AddWithValue("@password", PasswordBox.Password);
                        command.Parameters.AddWithValue("@role", RoleComboBox.Text);

                        int count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return false;
            }
        }
    }
}

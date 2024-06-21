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
            // Check if user exists and retrieve the role
            var role = IfUserExist();
            if (role == null)
            {
                MessageBox.Show($"{UsernameTextBox.Text} {PasswordBox.Password} {RoleComboBox.Text} doesn't have this data in the database");
                return;
            }

            // Validate role and navigate to corresponding page
            Window w = Window.GetWindow(this);
            if (role == "admin")
            {
                w.Content = new admin_page();
            }
            else if (role == "Warehouse Clerk")
            {
                w.Content = new warehouse_clerk_page();
            }
            else if (role == "Logistics Clerk")
            {
                w.Content = new logistic_clerk_page();
            }
            else
            {
                MessageBox.Show($"{UsernameTextBox.Text} is not assigned a valid role.");
            }
        }

        // Modified IfUserExist method to return the role if the user exists
        private string IfUserExist()
        {
            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    string sql = "SELECT role FROM users WHERE login = @login AND password = @password";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@login", UsernameTextBox.Text);
                        command.Parameters.AddWithValue("@password", PasswordBox.Password);

                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            return result.ToString();
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return null;
            }
        }

    }
}
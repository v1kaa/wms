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
        int selectedId;
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

        private void workersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (workersGrid.SelectedItem != null)
            {
                var selectedRow = workersGrid.SelectedItem as DataRowView;
                if (selectedRow != null)
                {
                     selectedId = Convert.ToInt32(selectedRow["id"]);
                    LoadUserData(selectedId);
                }
            }
        }

        private void LoadUserData(int userId) //get selected row data into textBlocks
        {
            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT login, password, role FROM Users WHERE id = @id";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", userId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                LoginTextBox.Text = reader["login"].ToString();
                                PasswordTextBox.Text = reader["password"].ToString();
                                RoleComboBox.Text = reader["role"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Problem while loading data: {ex.Message}");
            }
        }
        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM Users WHERE id = @id";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", selectedId);
                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("User deleted successfully.");
                LoadData();// Refresh the DataGrid after deletion
                DefaltTextBlocksData();
            } 
            catch (Exception ex)
            {
                MessageBox.Show($"Problem while loading data: {ex.Message}");
            }
        }
        private void DefaltTextBlocksData()
        {
            LoginTextBox.Text = "Login";
            PasswordTextBox.Text="Password";
            RoleComboBox.Text = null;
        }

        private void AddNewUserButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = LoginTextBox.Text;
                string password = PasswordTextBox.Text;
                string role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
                {
                    MessageBox.Show("Please fill in all fields.");
                    return;
                }

                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Users (login, password, role) VALUES (@login, @password, @role)";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@login", login);
                        command.Parameters.AddWithValue("@password", password);
                        command.Parameters.AddWithValue("@role", role);

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("New user added successfully.");
                LoadData(); // Refresh the DataGrid to show the new user
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Problem while adding data: {ex.Message}");
            }
        }
        private void EditButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (workersGrid.SelectedItem != null)
                {
                    var selectedRow = workersGrid.SelectedItem as DataRowView;
                    if (selectedRow != null)
                    {
                        int userId = Convert.ToInt32(selectedRow["id"]);

                        // Retrieve selected user's details
                        string oldLogin = selectedRow["login"].ToString();
                        string oldPassword = selectedRow["password"].ToString();
                        string oldRole = selectedRow["role"].ToString();

                        // Assuming you have input fields for editing
                        string newLogin = LoginTextBox.Text;
                        string newPassword = PasswordTextBox.Text;
                        string newRole = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                        if (string.IsNullOrWhiteSpace(newLogin) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(newRole))
                        {
                            MessageBox.Show("Please fill in all fields.");
                            return;
                        }

                        // Update user's details in the database
                        using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                        {
                            connection.Open();
                            string query = "UPDATE Users SET login = @login, password = @password, role = @role WHERE id = @id";

                            using (var command = new SQLiteCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@login", newLogin);
                                command.Parameters.AddWithValue("@password", newPassword);
                                command.Parameters.AddWithValue("@role", newRole);
                                command.Parameters.AddWithValue("@id", userId);

                                command.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show("User information updated successfully.");
                        LoadData(); // Refresh the DataGrid to reflect the changes
                    }
                }
                else
                {
                    MessageBox.Show("Please select a user to edit.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Problem while editing data: {ex.Message}");
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

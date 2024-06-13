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

namespace WMS.views
{
    /// <summary>
    /// Interaction logic for supplier_management.xaml
    /// </summary>
    public partial class supplier_management : UserControl
    {
        int selectedId;
        public supplier_management()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData() //shows all suppliers in table 
        {
            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT id,name,address,telephone FROM suppliers ";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        suppliersGrid.ItemsSource = dataTable.DefaultView; // Bind to DataGrid
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"problem while loading data : {ex.Message}");
            }
        }
        private void suppliersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (suppliersGrid.SelectedItem != null)
            {
                var selectedRow = suppliersGrid.SelectedItem as DataRowView;
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
                    string query = "SELECT name, address, telephone FROM suppliers WHERE id = @id";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", userId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                NameTextBox.Text = reader["name"].ToString();
                                AddressTextBox.Text = reader["address"].ToString();
                                TelephoneTextBox.Text = reader["telephone"].ToString();
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
                    string query = "DELETE FROM suppliers WHERE id = @id";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", selectedId);
                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("supplier deleted successfully.");
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
            NameTextBox.Text = "Name";
            AddressTextBox.Text = "Address";
            TelephoneTextBox.Text = "Telephone";
        }
        private void AddNewUserButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = NameTextBox.Text;
                string address = AddressTextBox.Text;
                string telephone = TelephoneTextBox.Text;

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(telephone))
                {
                    MessageBox.Show("Please fill in all fields.");
                    return;
                }

                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO suppliers (name, address, telephone) VALUES (@name, @address, @telephone)";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@address", address);
                        command.Parameters.AddWithValue("@telephone", telephone);

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("New supplier added successfully.");
                LoadData(); // Refresh the DataGrid to show the new user
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Problem while adding data: {ex.Message}");
            }
        }

        private async void EditButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (suppliersGrid.SelectedItem != null)
                {
                    var selectedRow = suppliersGrid.SelectedItem as DataRowView;
                    if (selectedRow != null)
                    {
                        int userId = Convert.ToInt32(selectedRow["id"]);

                        // Retrieve selected supplier's details
                        string oldName = selectedRow["name"].ToString();
                        string oldAddress = selectedRow["address"].ToString();
                        string oldTelephone = selectedRow["telephone"].ToString();

                        // Assuming you have input fields for editing
                        string newName = NameTextBox.Text;
                        string newAddress = AddressTextBox.Text;
                        string newTelephone = TelephoneTextBox.Text;

                        if (string.IsNullOrWhiteSpace(newName) || string.IsNullOrWhiteSpace(newAddress) || string.IsNullOrWhiteSpace(newTelephone))
                        {
                            MessageBox.Show("Please fill in all fields.");
                            return;
                        }

                        // Update supplier's details in the database
                        using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                        {
                            await connection.OpenAsync();
                            string query = "UPDATE suppliers SET name = @name, address = @address, telephone = @telephone WHERE id = @id";

                            using (var command = new SQLiteCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@name", newName);
                                command.Parameters.AddWithValue("@address", newAddress);
                                command.Parameters.AddWithValue("@telephone", newTelephone);
                                command.Parameters.AddWithValue("@id", userId);

                                await command.ExecuteNonQueryAsync();
                            }
                        }

                        MessageBox.Show("Supplier information updated successfully.");
                        LoadData(); // Refresh the DataGrid to reflect the changes
                    }
                }
                else
                {
                    MessageBox.Show("Please select a supplier to edit.");
                }
            }
            catch (Exception ex)
            {
                // Consider logging the exception for future debugging
                // LogException(ex);
                MessageBox.Show($"Problem while editing data: {ex.Message}");
            }
        }

    }
}

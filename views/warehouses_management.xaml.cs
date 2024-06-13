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
    /// Interaction logic for warehouses_management.xaml
    /// </summary>
    public partial class warehouses_management : UserControl
    {
        public int selectedId;
        public warehouses_management()
        {
            InitializeComponent();
            UpdateWarehouseStock();
            LoadData();
            UpdateWarehouseStock();
        }

        private void LoadData() //shows all suppliers in table 
        {
            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT warehouse_id,warehouse_name,total_capacity,quantity_in_stock FROM warehouses ";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        warehousesGrid.ItemsSource = dataTable.DefaultView; // Bind to DataGrid
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"problem while loading data : {ex.Message}");
            }
        }
        private void warehousesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            {
                if (warehousesGrid.SelectedItem != null)
                {
                    var selectedRow = warehousesGrid.SelectedItem as DataRowView;
                    if (selectedRow != null)
                    {
                        selectedId = Convert.ToInt32(selectedRow["warehouse_id"]);
                        LoadWarehouseData(selectedId);
                        LoadProductsData(selectedId);
                    }
                }

            }
        }
        private void LoadProductsData(int warehouseId)
        {
            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT product_id, product_name, quantity_in_stock, description FROM products WHERE warehouse_id = @warehouseId";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@warehouseId", warehouseId);

                        using (var reader = command.ExecuteReader())
                        {
                            DataTable dataTable = new DataTable();
                            dataTable.Load(reader);

                            productsGrid.ItemsSource = dataTable.DefaultView; // Bind products to DataGrid
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products data: {ex.Message}");
            }
        }
        private void LoadWarehouseData(int userId) //get selected row data into textBlocks
        {
            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT warehouse_name, total_capacity, quantity_in_stock FROM warehouses WHERE warehouse_id = @id";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", userId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                WarehouseNameTextBox.Text = reader["warehouse_name"].ToString();
                                TotalCapacityTextBox.Text = reader["total_capacity"].ToString();

                                
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

        private void UpdateWarehouseStock()
        {
            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    string updateQuery = @"
                UPDATE warehouses
                SET quantity_in_stock = (
                    SELECT COALESCE(SUM(p.quantity_in_stock), 0)
                    FROM products p
                    WHERE p.warehouse_id = warehouses.warehouse_id
                )";

                    using (var command = new SQLiteCommand(updateQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating warehouse stock: {ex.Message}");
            }
        }
        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            if (warehousesGrid.SelectedItem != null)
            {
                var selectedRow = warehousesGrid.SelectedItem as DataRowView;
                if (selectedRow != null)
                {
                    int warehouseId = Convert.ToInt32(selectedRow["warehouse_id"]);

                    try
                    {
                        using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                        {
                            connection.Open();

                            // Check if there are products in the warehouse
                            string checkQuery = "SELECT COUNT(*) FROM products WHERE warehouse_id = @warehouseId";
                            using (var checkCommand = new SQLiteCommand(checkQuery, connection))
                            {
                                checkCommand.Parameters.AddWithValue("@warehouseId", warehouseId);
                                int productCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                                if (productCount > 0)
                                {
                                    MessageBox.Show("Cannot delete the warehouse because it contains products.");
                                    return;
                                }
                            }

                            // Delete the warehouse if there are no products
                            string deleteQuery = "DELETE FROM warehouses WHERE warehouse_id = @warehouseId";
                            using (var deleteCommand = new SQLiteCommand(deleteQuery, connection))
                            {
                                deleteCommand.Parameters.AddWithValue("@warehouseId", warehouseId);
                                deleteCommand.ExecuteNonQuery();
                            }

                            // Refresh the data grid
                            LoadData();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting warehouse: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("No warehouse selected.");
                }
            }
            else
            {
                MessageBox.Show("No warehouse selected.");
            }
        }


        private void EditButtonClick(object sender, RoutedEventArgs e)
        {
            // Check if a warehouse is selected
            if (warehousesGrid.SelectedItem == null)
            {
                MessageBox.Show("No warehouse selected.");
                return;
            }

            // Get the selected row
            var selectedRow = warehousesGrid.SelectedItem as DataRowView;
            if (selectedRow == null)
            {
                MessageBox.Show("Invalid selection.");
                return;
            }

            // Retrieve the warehouse ID from the selected row
            int warehouseId = Convert.ToInt32(selectedRow["warehouse_id"]);

            // Retrieve and validate input values
            string warehouseName = WarehouseNameTextBox.Text;
            int totalCapacity;

            if (string.IsNullOrWhiteSpace(warehouseName))
            {
                MessageBox.Show("Warehouse name cannot be empty.");
                return;
            }

            if (!int.TryParse(TotalCapacityTextBox.Text, out totalCapacity) || totalCapacity <= 0)
            {
                MessageBox.Show("Total capacity must be a positive integer.");
                return;
            }

            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();

                    // Update the selected warehouse in the database
                    string updateQuery = "UPDATE warehouses SET warehouse_name = @warehouseName, total_capacity = @totalCapacity WHERE warehouse_id = @warehouseId";
                    using (var command = new SQLiteCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@warehouseName", warehouseName);
                        command.Parameters.AddWithValue("@totalCapacity", totalCapacity);
                        command.Parameters.AddWithValue("@warehouseId", warehouseId);
                        command.ExecuteNonQuery();
                    }

                    // Refresh the data grid
                    LoadData();

                    // Clear the text boxes
                    DeffaultTextBoxex();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating warehouse: {ex.Message}");
            }
        }


        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            string warehouseName = WarehouseNameTextBox.Text;
            int totalCapacity;

            // Validate input
            if (string.IsNullOrWhiteSpace(warehouseName))
            {
                MessageBox.Show("Warehouse name cannot be empty.");
                return;
            }

            if (!int.TryParse(TotalCapacityTextBox.Text, out totalCapacity) || totalCapacity <= 0)
            {
                MessageBox.Show("Total capacity must be a positive integer.");
                return;
            }

            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();

                    // Insert the new warehouse into the warehouses table
                    string insertQuery = "INSERT INTO warehouses (warehouse_name, total_capacity, quantity_in_stock) VALUES (@warehouseName, @totalCapacity, 0)";
                    using (var command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@warehouseName", warehouseName);
                        command.Parameters.AddWithValue("@totalCapacity", totalCapacity);
                        command.ExecuteNonQuery();
                    }

                    // Refresh the data grid
                    LoadData();

                    // Clear the text boxes
                    DeffaultTextBoxex();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding warehouse: {ex.Message}");
            }
        }
        private void DeffaultTextBoxex()
        {
            WarehouseNameTextBox.Text = "Name:";
            TotalCapacityTextBox.Text = "Total Capacity";
        }

    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Security.Policy;
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
    /// Interaction logic for products_management.xaml
    /// </summary>
    public partial class products_management : UserControl
    {
        public int selectedId;
        public products_management()
        {
            InitializeComponent();
            LoadData();
            LoadWarehouses();

        }
        private void LoadData() //shows all suppliers in table 
        {
            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT product_id, product_name,quantity_in_stock,unit_price,description, warehouse_id FROM products";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        productsGrid.ItemsSource = dataTable.DefaultView; // Bind to DataGrid
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"problem while loading data : {ex.Message}");
            }
        }
        private void productsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            {
                if (productsGrid.SelectedItem != null)
                {
                    var selectedRow = productsGrid.SelectedItem as DataRowView;
                    if (selectedRow != null)
                    {
                        selectedId = Convert.ToInt32(selectedRow["product_id"]);
                        LoadProductData(selectedId);

                    }
                }

            }
        }
        private void LoadProductData(int productId)
        {
            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    // Join products with warehouses to get warehouse_name
                    string query = @"SELECT product_id, product_name, quantity_in_stock, unit_price, description, warehouse_id 
                         FROM products  where product_id = @productId";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@productId", productId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ProductNameTextBox.Text = reader["product_name"].ToString();
                                QuantityTextBox.Text = reader["quantity_in_stock"].ToString();
                                PriceTextBox.Text = reader["unit_price"].ToString();
                                DescriptionTextBox.Text = reader["description"].ToString();
                                WarehouseNameTextBox.Text = reader["warehouse_id"].ToString();
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

                    // Retrieve the quantity from the database
                    string quantityQuery = "SELECT quantity_in_stock FROM products WHERE product_id = @id";
                    int quantity;
                    using (var quantityCommand = new SQLiteCommand(quantityQuery, connection))
                    {
                        quantityCommand.Parameters.AddWithValue("@id", selectedId);
                        var result = quantityCommand.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            MessageBox.Show("Product not found.");
                            return;
                        }
                        quantity = Convert.ToInt32(result);
                    }

                    // Check if the quantity is 0 before deleting
                    if (quantity == 0)
                    {
                        string deleteQuery = "DELETE FROM products WHERE product_id = @id";
                        using (var command = new SQLiteCommand(deleteQuery, connection))
                        {
                            command.Parameters.AddWithValue("@id", selectedId);
                            command.ExecuteNonQuery();
                        }

                        MessageBox.Show("Product deleted successfully.");
                        LoadData(); // Refresh the DataGrid after deletion
                    }
                    else
                    {
                        MessageBox.Show("Cannot delete the product because quantity is not 0.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Problem while deleting product: {ex.Message}");
            }
        }


        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string produt_name = ProductNameTextBox.Text;
                string quantity = QuantityTextBox.Text;
                string price = PriceTextBox.Text;
                string description = DescriptionTextBox.Text;
                string warehouse_id = WarehouseNameTextBox.Text;


                if (string.IsNullOrWhiteSpace(produt_name) || string.IsNullOrWhiteSpace(quantity) || string.IsNullOrWhiteSpace(price) || string.IsNullOrWhiteSpace(warehouse_id))
                {
                    MessageBox.Show("Please fill in all neccecary fields.");
                    return;
                }
                if (FreeCapacityInWarehouse(Convert.ToInt16(warehouse_id)) < Convert.ToInt16(quantity))
                {
                    MessageBox.Show("there are no free place for this quantity in this warehouse");
                    return;
                }

                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO products (product_name,quantity_in_stock,unit_price,description, warehouse_id) VALUES (@name, @quantity, @price, @description, @warehouse_id)";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", produt_name);
                        command.Parameters.AddWithValue("@quantity", quantity);
                        command.Parameters.AddWithValue("@price", price);
                        command.Parameters.AddWithValue("@description", description);
                        command.Parameters.AddWithValue("@warehouse_id", warehouse_id);


                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("New product added successfully.");
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
                if (productsGrid.SelectedItem != null)
                {
                    var selectedRow = productsGrid.SelectedItem as DataRowView;
                    if (selectedRow != null)
                    {
                        int productId = Convert.ToInt32(selectedRow["product_id"]);

                        // Retrieve selected product's details
                        string oldProductName = selectedRow["product_name"].ToString();
                        string oldQuantity = selectedRow["quantity_in_stock"].ToString();
                        string oldPrice = selectedRow["unit_price"].ToString();
                        string oldDescription = selectedRow["description"].ToString();
                        string oldWarehouseId = selectedRow["warehouse_id"].ToString();

                        // Assuming you have input fields for editing
                        string newProductName = ProductNameTextBox.Text;
                        string newQuantity = QuantityTextBox.Text;
                        string newPrice = PriceTextBox.Text;
                        string newDescription = DescriptionTextBox.Text;
                        string newWarehouseId = WarehouseNameTextBox.Text;

                        if (string.IsNullOrWhiteSpace(newProductName) || FreeCapacityInWarehouse(Convert.ToInt32(newWarehouseId)) < (Convert.ToInt32(newQuantity) - Convert.ToInt32(oldQuantity)))
                        {
                            MessageBox.Show("Please fill in all fields correctly and ensure there is enough capacity in the warehouse.");
                            return;
                        }

                        // Check if the new quantity is less than the old quantity
                        if (Convert.ToInt32(newQuantity) < Convert.ToInt32(oldQuantity))
                        {
                            MessageBox.Show("The new quantity cannot be less than the existing quantity.");
                            return;
                        }

                        // Update product's details in the database
                        using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                        {
                            connection.Open();
                            string query = "UPDATE products SET product_name = @name, quantity_in_stock = @quantity, unit_price = @price, description = @description, warehouse_id = @warehouseId WHERE product_id = @id";

                            using (var command = new SQLiteCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@name", newProductName);
                                command.Parameters.AddWithValue("@quantity", newQuantity);
                                command.Parameters.AddWithValue("@price", newPrice);
                                command.Parameters.AddWithValue("@description", newDescription);
                                command.Parameters.AddWithValue("@warehouseId", newWarehouseId);
                                command.Parameters.AddWithValue("@id", productId);

                                command.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show("Product information updated successfully.");
                        LoadData(); // Refresh the DataGrid to reflect the changes
                    }
                }
                else
                {
                    MessageBox.Show("Please select a product to edit.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Problem while editing data: {ex.Message}");
            }
        }
        private int FreeCapacityInWarehouse(int warehouseId)
        {
            string sql = @"
        SELECT (total_capacity - quantity_in_stock) AS free_capacity
        FROM warehouses
        WHERE warehouse_id = @warehouseId;
    ";

            using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@warehouseId", warehouseId);
                    object result = command.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToInt32(result);
                }
            }
        }
        private void SendProductsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the selected product
                if (productsGrid.SelectedItem != null)
                {
                    var selectedRow = productsGrid.SelectedItem as DataRowView;
                    if (selectedRow != null)
                    {
                        int productId = Convert.ToInt32(selectedRow["product_id"]);
                        string productName = selectedRow["product_name"].ToString();
                        int currentQuantity = Convert.ToInt32(selectedRow["quantity_in_stock"]);
                        int currentWarehouseId = Convert.ToInt32(selectedRow["warehouse_id"]);

                        // Get the quantity to send and the destination warehouse
                        int quantityToSend;
                        if (int.TryParse(quantity_to_send.Text, out quantityToSend) && quantityToSend <= currentQuantity)
                        {
                            int destinationWarehouseId = Convert.ToInt32(warehouses.SelectedValue);

                            // Check if the destination warehouse has enough capacity
                            int freeCapacity = FreeCapacityInWarehouse(destinationWarehouseId);
                            if (freeCapacity >= quantityToSend)
                            {
                                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                                {
                                    connection.Open();

                                    // Update the quantity in the current warehouse
                                    string updateCurrentQuery = "UPDATE products SET quantity_in_stock = @newQuantity WHERE product_id = @productId AND warehouse_id = @currentWarehouseId";
                                    using (var updateCurrentCommand = new SQLiteCommand(updateCurrentQuery, connection))
                                    {
                                        updateCurrentCommand.Parameters.AddWithValue("@newQuantity", currentQuantity - quantityToSend);
                                        updateCurrentCommand.Parameters.AddWithValue("@productId", productId);
                                        updateCurrentCommand.Parameters.AddWithValue("@currentWarehouseId", currentWarehouseId);
                                        updateCurrentCommand.ExecuteNonQuery();
                                    }

                                    // Insert the product into the destination warehouse
                                    string insertDestinationQuery = "INSERT INTO products (product_name, quantity_in_stock, unit_price, description, warehouse_id) VALUES (@name, @quantity, @price, @description, @warehouseId)";
                                    using (var insertDestinationCommand = new SQLiteCommand(insertDestinationQuery, connection))
                                    {
                                        insertDestinationCommand.Parameters.AddWithValue("@name", productName);
                                        insertDestinationCommand.Parameters.AddWithValue("@quantity", quantityToSend);
                                        insertDestinationCommand.Parameters.AddWithValue("@price", selectedRow["unit_price"]);
                                        insertDestinationCommand.Parameters.AddWithValue("@description", selectedRow["description"]);
                                        insertDestinationCommand.Parameters.AddWithValue("@warehouseId", destinationWarehouseId);
                                        insertDestinationCommand.ExecuteNonQuery();
                                    }
                                }

                                MessageBox.Show($"{quantityToSend} units of {productName} sent to warehouse {warehouses.Text} successfully.");
                                LoadData(); // Refresh the DataGrid
                            }
                            else
                            {
                                MessageBox.Show("The destination warehouse does not have enough capacity.");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Please enter a valid quantity to send (less than or equal to the current quantity).");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select a product to send.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Problem while sending products: {ex.Message}");
            }
        }
        private void LoadWarehouses()
        {
            using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
            {
                connection.Open();
                string query = "SELECT warehouse_id, warehouse_name FROM warehouses ";
                using (var command = new SQLiteCommand(query, connection))
                {
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    warehouses.ItemsSource = dataTable.DefaultView;
                    warehouses.DisplayMemberPath = "warehouse_name";
                    warehouses.SelectedValuePath = "warehouse_id";
                }
            }
        }

    }
}

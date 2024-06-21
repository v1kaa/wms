using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
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
    /// Interaction logic for create_orders_export.xaml
    /// </summary>
    public partial class create_orders_export : UserControl
    {
        public int selectedId;
        public int selectedItemId;
        public string order_number;
        public int selectedOrderId;
        public create_orders_export()
        {
            InitializeComponent();
            

            LoadData();
            
            LoadOrdersData();
        }
        private void LoadData()
        {
            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT product_id, product_name, quantity_in_stock, unit_price, description, warehouse_id FROM products";

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
                MessageBox.Show($"Problem while loading data: {ex.Message}");
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

                    }
                }

            }
        }

        private void orderItemsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            {
                if (orderItemsGrid.SelectedItem != null)
                {
                    var selectedRow = orderItemsGrid.SelectedItem as DataRowView;
                    if (selectedRow != null)
                    {
                        selectedItemId = Convert.ToInt32(selectedRow["product_id"]);

                    }
                }

            }
        }

        private void OrdersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrdersGrid.SelectedItem == null)
            {
                return;
            }

            var selectedRow = OrdersGrid.SelectedItem as DataRowView;
            if (selectedRow == null)
            {
                return;
            }

            int selectedOrderId = Convert.ToInt32(selectedRow["order_id"]);

            // Load order details into orderItemsGrid
            LoadOrderDetails(selectedOrderId);

            // Update vehicle and order number text boxes
            VechicleTextBox.Text = selectedRow["vehicle"].ToString();
            OrderNumberTextBox.Text = selectedRow["order_number"].ToString();
        }







        private void DeleteItemsFromOrderAndReturnTowarehouse(object sender, RoutedEventArgs e)
        {
            if (orderItemsGrid.SelectedItem != null)
            {
                var selectedRow = orderItemsGrid.SelectedItem as DataRowView;
                if (selectedRow != null)
                {
                    int productId = Convert.ToInt32(selectedRow["product_id"]);
                    int quantity = Convert.ToInt32(selectedRow["quantity"]);
                    int selectedOrderId = GetSelectedOrderId(); // Retrieve the selected order ID

                    try
                    {
                        using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                        {
                            connection.Open();

                            // Update the products table to return the quantity
                            string updateProductQuery = "UPDATE products SET quantity_in_stock = quantity_in_stock + @quantity WHERE product_id = @productId";
                            using (var updateProductCommand = new SQLiteCommand(updateProductQuery, connection))
                            {
                                updateProductCommand.Parameters.AddWithValue("@quantity", quantity);
                                updateProductCommand.Parameters.AddWithValue("@productId", productId);
                                updateProductCommand.ExecuteNonQuery();
                            }

                            // Delete the selected item from the order_details table
                            string deleteOrderDetailQuery = "DELETE FROM order_details WHERE product_id = @productId AND order_id = @orderId";
                            using (var deleteOrderDetailCommand = new SQLiteCommand(deleteOrderDetailQuery, connection))
                            {
                                deleteOrderDetailCommand.Parameters.AddWithValue("@productId", productId);
                                deleteOrderDetailCommand.Parameters.AddWithValue("@orderId", selectedOrderId);
                                deleteOrderDetailCommand.ExecuteNonQuery();
                            }

                            MessageBox.Show("Item deleted from the order and returned to the warehouse successfully.");
                            LoadOrderDetails(selectedOrderId); // Refresh the order items grid
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Problem while deleting item from the order: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select an item from the order items grid.");
            }
        }

        private int GetSelectedOrderId()
        {
            if (OrdersGrid.SelectedItem == null)
            {
                return -1;
            }

            var selectedRow = OrdersGrid.SelectedItem as DataRowView;
            if (selectedRow == null)
            {
                return -1;
            }

            return Convert.ToInt32(selectedRow["order_id"]);
        }

        private void AddItemIntoOrderDetailsTable(object sender, RoutedEventArgs e)
        {
            if (productsGrid.SelectedItem != null && OrdersGrid.SelectedItem != null)
            {
                var selectedProductRow = productsGrid.SelectedItem as DataRowView;
                var selectedOrderRow = OrdersGrid.SelectedItem as DataRowView;

                if (selectedProductRow != null && selectedOrderRow != null)
                {
                    string productId = selectedProductRow["product_id"].ToString();
                    string productName = selectedProductRow["product_name"].ToString();
                    string quantityInStock = selectedProductRow["quantity_in_stock"].ToString();
                    string unitPrice = selectedProductRow["unit_price"].ToString();
                    int orderId = Convert.ToInt32(selectedOrderRow["order_id"]);

                    int orderedQuantity;
                    if (int.TryParse(quantityTextBox.Text, out orderedQuantity))
                    {
                        if (orderedQuantity <= Convert.ToInt32(quantityInStock))
                        {
                            try
                            {
                                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                                {
                                    connection.Open();

                                    // Update the products table
                                    string updateQuery = "UPDATE products SET quantity_in_stock = @newQuantity WHERE product_id = @productId";
                                    using (var updateCommand = new SQLiteCommand(updateQuery, connection))
                                    {
                                        updateCommand.Parameters.AddWithValue("@newQuantity", (Convert.ToInt32(quantityInStock) - orderedQuantity));
                                        updateCommand.Parameters.AddWithValue("@productId", productId);
                                        updateCommand.ExecuteNonQuery();
                                    }

                                    // Insert the order details
                                    string insertQuery = "INSERT INTO order_details (order_id, product_id, quantity, unit_price) " +
                                                         "VALUES (@orderId, @productId, @quantity, @unitPrice)";
                                    using (var insertCommand = new SQLiteCommand(insertQuery, connection))
                                    {
                                        insertCommand.Parameters.AddWithValue("@orderId", orderId);
                                        insertCommand.Parameters.AddWithValue("@productId", productId);
                                        insertCommand.Parameters.AddWithValue("@quantity", orderedQuantity);
                                        insertCommand.Parameters.AddWithValue("@unitPrice", unitPrice);
                                        insertCommand.ExecuteNonQuery();
                                    }
                                }

                                MessageBox.Show("Product added to the selected order successfully.");
                                LoadOrderDetails(selectedOrderId);
                                LoadData(); // Refresh the products grid
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Problem while adding product to the order: {ex.Message}");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Insufficient quantity in stock.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid quantity entered.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a product and an order from the grids.");
            }
        }


        string GenerateRandomOrderNumber()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";

            StringBuilder sb = new StringBuilder();

            Random random = new Random();

            // Generate 3 random uppercase letters
            for (int i = 0; i < 3; i++)
            {
                int index = random.Next(chars.Length);
                sb.Append(chars[index]);
            }

            // Generate 4 random digits
            for (int i = 0; i < 4; i++)
            {
                int index = random.Next(digits.Length);
                sb.Append(digits[index]);
            }

            // Generate 1 random uppercase letter
            int lastIndex = random.Next(chars.Length);
            sb.Append(chars[lastIndex]);

            return sb.ToString();
        }
        private string CreateDefaultOrder()
        {
            string orderNumber;
            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();

                    // Generate a unique order number (e.g., using the current timestamp)
                    orderNumber = GenerateRandomOrderNumber();

                    // Set default values for the order
                    DateTime orderDate = DateTime.Now;
                    string vehicle = "Default Vehicle";
                    string totalPrice = "0";

                    // SQL query to insert the new order
                    string query = "INSERT INTO orders (order_date, order_number, vehicle, total_price) VALUES (@orderDate, @orderNumber, @vehicle, @totalPrice)";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        // Add parameters to the query
                        command.Parameters.AddWithValue("@orderDate", orderDate);
                        command.Parameters.AddWithValue("@orderNumber", orderNumber);
                        command.Parameters.AddWithValue("@vehicle", vehicle);
                        command.Parameters.AddWithValue("@totalPrice", totalPrice);

                        // Execute the query
                        command.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Default order created successfully.");
                return orderNumber;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Problem while creating default order: {ex.Message}");
                return null;
            }
        }

        private void LoadOrderDetails(int? orderId = null)
        {
            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();

                    if (!orderId.HasValue)
                    {
                        // Get the order ID based on the order_number
                        string selectOrderIdQuery = "SELECT order_id FROM orders WHERE order_number = @orderNumber";
                        using (var selectOrderIdCommand = new SQLiteCommand(selectOrderIdQuery, connection))
                        {
                            selectOrderIdCommand.Parameters.AddWithValue("@orderNumber", order_number);
                            object result = selectOrderIdCommand.ExecuteScalar();
                            if (result != null && int.TryParse(result.ToString(), out int id))
                            {
                                orderId = id;
                            }
                            else
                            {
                                // Clear the order items grid if no order is found
                                orderItemsGrid.ItemsSource = null;
                                return;
                            }
                        }
                    }

                    // Retrieve the order details based on the order ID
                    string selectOrderDetailsQuery = @"
                SELECT od.product_id, p.product_name, od.quantity, od.unit_price
                FROM order_details od
                JOIN products p ON od.product_id = p.product_id
                WHERE od.order_id = @orderId
            ";

                    using (var selectOrderDetailsCommand = new SQLiteCommand(selectOrderDetailsQuery, connection))
                    {
                        selectOrderDetailsCommand.Parameters.AddWithValue("@orderId", orderId.Value);

                        using (var adapter = new SQLiteDataAdapter(selectOrderDetailsCommand))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            orderItemsGrid.ItemsSource = dataTable.DefaultView;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Problem while loading order details: {ex.Message}");
            }
        }


        private void ChangeOrderInfoClick(object sender, EventArgs e)
        {
            int selectedOrderId = GetSelectedOrderId();
            if (selectedOrderId == -1)
            {
                MessageBox.Show("Please select an order from the grid.");
                return;
            }

            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();

                    // First, retrieve the current order information
                    string selectQuery = "SELECT order_date, vehicle, order_number FROM orders WHERE order_id = @orderId";
                    DateTime currentDate;
                    string currentVehicle, currentOrderNumber;

                    using (var selectCommand = new SQLiteCommand(selectQuery, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@orderId", selectedOrderId);
                        using (var reader = selectCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currentDate = reader.GetDateTime(0);
                                currentVehicle = reader.GetString(1);
                                currentOrderNumber = reader.GetString(2);
                            }
                            else
                            {
                                MessageBox.Show("Selected order not found.");
                                return;
                            }
                        }
                    }

                    // Use new values if provided, otherwise keep the current values
                    DateTime newDate = OrderDataCalendar.SelectedDate ?? currentDate;
                    string newVehicle = string.IsNullOrWhiteSpace(VechicleTextBox.Text) ? currentVehicle : VechicleTextBox.Text;
                    string newOrderNumber = string.IsNullOrWhiteSpace(OrderNumberTextBox.Text) ? currentOrderNumber : OrderNumberTextBox.Text;

                    decimal totalPrice = CalculateTotalPrice(selectedOrderId);

                    // SQL query to update the order
                    string updateQuery = @"
                UPDATE orders 
                SET order_date = @newDate, 
                    vehicle = @newVehicle, 
                    total_price = @totalPrice, 
                    order_number = @newOrderNumber 
                WHERE order_id = @selectedOrderId";

                    using (var updateCommand = new SQLiteCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@newDate", newDate);
                        updateCommand.Parameters.AddWithValue("@newVehicle", newVehicle);
                        updateCommand.Parameters.AddWithValue("@totalPrice", totalPrice);
                        updateCommand.Parameters.AddWithValue("@newOrderNumber", newOrderNumber);
                        updateCommand.Parameters.AddWithValue("@selectedOrderId", selectedOrderId);

                        int rowsAffected = updateCommand.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Order updated successfully.");
                            LoadOrderDetails(selectedOrderId);
                            LoadData();
                            LoadOrdersData();
                        }
                        else
                        {
                            MessageBox.Show("No changes were made to the order.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Problem while updating order: {ex.Message}");
            }
        }








        private decimal CalculateTotalPrice(int orderId)
        {
            decimal totalPrice = 0;
            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    // SQL query to retrieve the total price of products in the order
                    string query = @"
                SELECT SUM(od.quantity * od.unit_price) AS total_price
                FROM order_details od
                WHERE od.order_id = @orderId
            ";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@orderId", orderId);
                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            totalPrice = Convert.ToDecimal(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Problem while calculating total price: {ex.Message}");
            }
            return totalPrice;
        }

        private void DeleteOrderClick(object sender, EventArgs e)
        {
            if (OrdersGrid.SelectedItem == null)
            {
                MessageBox.Show("Please select an order to delete.");
                return;
            }

            var selectedRow = OrdersGrid.SelectedItem as DataRowView;
            if (selectedRow == null)
            {
                MessageBox.Show("Invalid selected order.");
                return;
            }

            int orderId = Convert.ToInt32(selectedRow["order_id"]);

            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();

                    // Delete the order from orders table
                    string deleteOrderQuery = "DELETE FROM orders WHERE order_id = @orderId";
                    using (var deleteOrderCommand = new SQLiteCommand(deleteOrderQuery, connection))
                    {
                        deleteOrderCommand.Parameters.AddWithValue("@orderId", orderId);
                        deleteOrderCommand.ExecuteNonQuery();
                    }

                    // Return products to warehouses
                    string selectOrderDetailsQuery = @"
                SELECT od.product_id, od.quantity
                FROM order_details od
                WHERE od.order_id = @orderId
            ";

                    using (var selectOrderDetailsCommand = new SQLiteCommand(selectOrderDetailsQuery, connection))
                    {
                        selectOrderDetailsCommand.Parameters.AddWithValue("@orderId", orderId);

                        using (var reader = selectOrderDetailsCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int productId = Convert.ToInt32(reader["product_id"]);
                                int quantity = Convert.ToInt32(reader["quantity"]);

                                // Update products table to return quantity to warehouse
                                string updateProductQuery = "UPDATE products SET quantity_in_stock = quantity_in_stock + @quantity WHERE product_id = @productId";
                                using (var updateProductCommand = new SQLiteCommand(updateProductQuery, connection))
                                {
                                    updateProductCommand.Parameters.AddWithValue("@quantity", quantity);
                                    updateProductCommand.Parameters.AddWithValue("@productId", productId);
                                    updateProductCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    MessageBox.Show("Order deleted successfully. Products returned to warehouses.");
                    LoadOrdersData(); // Refresh orders grid after deletion
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting order: {ex.Message}");
            }
        }


        private void ShowCalendarButton_Click(object sender, RoutedEventArgs e)
        {
            CalendarPopup.IsOpen = true;
        }

        private void OrderDataCalendar_DisplayModeChanged(object sender, CalendarModeChangedEventArgs e)
        {
            Calendar calendar = sender as Calendar;

            // Prevent the user from selecting past dates
            if (calendar.SelectedDate.HasValue && calendar.SelectedDate.Value < DateTime.Today)
            {
                calendar.SelectedDate = DateTime.Today;
            }
        }

        private void LoadOrdersData()
        {
            try
            {
                using (var connection = new SQLiteConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT order_id, order_number, order_date, vehicle, total_price FROM orders";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        OrdersGrid.ItemsSource = dataTable.DefaultView; // Bind to DataGrid
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Problem while loading orders data: {ex.Message}");
            }
        }
        private void CreateNewEmptyOrder(object sender, EventArgs e) {
            order_number = CreateDefaultOrder();
        }
    }
}

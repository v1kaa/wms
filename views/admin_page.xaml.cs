using System;
using System.Collections.Generic;
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
    /// Interaction logic for admin_page.xaml
    /// </summary>
    public partial class admin_page : UserControl
    {
        public admin_page()
        {
            InitializeComponent();
        }
        private void NavigationListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavigationListBox.SelectedItem is ListBoxItem selectedItem)
            {
                switch (selectedItem.Tag)
                {
                    case "Warehouses management":
                        MainContentControl.Content = new warehouses_management();
                        break;
                    case "Workers":
                        MainContentControl.Content = new workers_management();
                        break;
                    case "Suppliers":
                        MainContentControl.Content = new supplier_management();
                        break;
                    case "Products":
                        MainContentControl.Content=new products_management();
                        break;
                    case "OrdersExport":
                        MainContentControl.Content=new create_orders_export();
                        break;
                    case "AllOrdersForExport":
                        MainContentControl.Content = new all_orders_export();
                        break;
                    default:
                        MainContentControl.Content = new workers_management();
                        break;
                }
            }
        }
    }
}

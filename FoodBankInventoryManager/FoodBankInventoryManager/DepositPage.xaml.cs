using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
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

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for DepositPage.xaml
    /// </summary>
    public partial class DepositPage : Page
    {
        private L2S_FoodBankDBDataContext dbContext;
        public DepositPage()
        {
            InitializeComponent();
            dbContext = new L2S_FoodBankDBDataContext();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //useful if you need to get data from the database such as table names
            //var dataModel = new AttributeMappingSource().GetModel(typeof(L2S_FoodBankDBDataContext));
            string[] sortOptions = new string[] { "Item Name (A-Z)", "Item Name (Z-A)", "Quantity (Asc.)","Quantity (Desc.)" };
            foreach (string item in sortOptions)
            {
                comboSort.Items.Add(item);
            }
    
            comboSort.SelectedIndex = 0;
               
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            inputBox.Visibility = Visibility.Visible;
            ScannerEmulator se = new ScannerEmulator();
            se.ShowDialog();
            inputBox.Visibility = Visibility.Collapsed;
        }
    }
}

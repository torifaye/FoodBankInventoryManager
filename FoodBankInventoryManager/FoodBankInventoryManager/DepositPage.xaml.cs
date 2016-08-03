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
using System.Configuration;

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for DepositPage.xaml
    /// </summary>
    public partial class DepositPage : Page
    {
        private L2S_FoodBankDBDataContext dbContext;
        private DateTime dateOpened;
        private User myCurrentUser;

        public DepositPage(User aUser)
        {
            dateOpened = DateTime.Now;
            myCurrentUser = aUser;
            InitializeComponent();
            dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //useful if you need to get data from the database such as table names
            //var dataModel = new AttributeMappingSource().GetModel(typeof(L2S_FoodBankDBDataContext));
        }

        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            inputBox.Visibility = Visibility.Visible;
            ScannerEmulator se = new ScannerEmulator(myCurrentUser);
            se.ShowDialog();
            var inventoryInfo = from items in dbContext.GetTable<InventoryEntry>()
                                where items.DateEntered > dateOpened
                                select new DepositEntry
                                {
                                    FoodName = items.FoodName,
                                    BinId = items.BinId,
                                    ShelfId = items.ShelfId,
                                    BinQuantity = items.BinQty
                                };
            grdItems.ItemsSource = inventoryInfo;
            inputBox.Visibility = Visibility.Collapsed;
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            HomePage h = new HomePage();
            this.NavigationService.Navigate(h);
        }

    }
    public class DepositEntry
    {
        public string FoodName
        {
            get; set;
        }
        public string BinId
        {
            get; set;
        }
        public string ShelfId
        {
            get; set;
        }
        public int BinQuantity
        {
            get; set;
        }
    }
}

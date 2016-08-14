using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

        /// <summary>
        /// Displays items that are entered into database in current session page is opened in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                                    Quantity = items.ItemQty
                                };
            grdItems.ItemsSource = inventoryInfo;
            inputBox.Visibility = Visibility.Collapsed;
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            HomePage h = new HomePage(myCurrentUser);
            this.NavigationService.Navigate(h);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            HomePage h = new HomePage(myCurrentUser);
            NavigationService.Navigate(h);
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
        public int Quantity
        {
            get; set;
        }
    }
}

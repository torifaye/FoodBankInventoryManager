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
        private DateTime dateOpened;
        //These three arrays will capture the food items that the user inputs this session and will delete them after the user clicks finish adding
        private List<string> itemNamesThisSession;
        private List<int> quantitiesThisSession;
        private List<DateTime> datesThisSession;
        public DepositPage()
        {
            InitializeComponent();
            dbContext = new L2S_FoodBankDBDataContext(@"Data Source=.\SQLEXPRESS;AttachDbFilename=C:\Users\YostR\Source\Repos\FoodBankInventoryManager\FoodBankInventoryManager\FoodBankInventoryManager\FoodBankDB.mdf;Integrated Security=True;Connect Timeout=30;User Instance=True");
            dateOpened = DateTime.Now;
            itemNamesThisSession = new List<string>();
            quantitiesThisSession = new List<int>();
            datesThisSession = new List<DateTime>();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //useful if you need to get data from the database such as table names
            //var dataModel = new AttributeMappingSource().GetModel(typeof(L2S_FoodBankDBDataContext));
            string[] sortOptions = new string[] { "Item Name (A-Z)", "Item Name (Z-A)", "Quantity (Asc.)", "Quantity (Desc.)" };
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
            var inventoryInfo = from items in dbContext.GetTable<InvBin>()
                                where items.DateEntered.CompareTo(dateOpened) < 0
                                select new InventoryInfo
                                {
                                    FoodCode = items.FoodCode,
                                    DateEntered = items.DateEntered,
                                    Quantity = items.Quantity
                                };
            grdItems.ItemsSource = inventoryInfo;
            //grdItems.ItemsSource = dbContext.GetTable<InvBin>();
            //itemNamesThisSession.Add(invBinThisSession.FoodCode);
            //quantitiesThisSession.Add(invBinThisSession.Quantity);
            //datesThisSession.Add(invBinThisSession.DateEntered);
            inputBox.Visibility = Visibility.Collapsed;
        }

        private void bttnHome_Click(object sender, RoutedEventArgs e)
        {
            HomePage h = new HomePage(true);
            this.NavigationService.Navigate(h);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}

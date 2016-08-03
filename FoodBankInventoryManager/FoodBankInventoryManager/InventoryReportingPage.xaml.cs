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
using System.Configuration;

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for StatisticsPage.xaml
    /// </summary>
    public partial class InventoryReportingPage : Page
    {
        private L2S_FoodBankDBDataContext dbContext;
        public InventoryReportingPage()
        {
            InitializeComponent();
            dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var inventoryInfo = from items in dbContext.GetTable<InventoryEntry>()
                                where items != null
                                select new InventoryInfo
                                {
                                    FoodId = items.FoodId,
                                    BinId = items.BinId,
                                    ShelfId = items.ShelfId,
                                    BinQuantity = items.BinQty,
                                    DateEntered = items.DateEntered
                                };
            grdItems.ItemsSource = inventoryInfo;
            txtItemCount.Text = inventoryInfo.ToArray<InventoryInfo>().Length.ToString();
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {

        }
    }
    class InventoryInfo
    {
        public string FoodId
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
        public DateTime DateEntered
        {
            get; set;
        }
        public int BinQuantity
        {
            get; set;
        }
    }
}

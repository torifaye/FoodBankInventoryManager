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
using System.Collections;

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
                                    FoodName = items.FoodName,
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

        private void grdItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DataGridRow item = ItemsControl.ContainerFromElement(sender as DataGrid, e.OriginalSource as DependencyObject) as DataGridRow;
                if (item != null)
                {
                    // ListBox item clicked - do some cool things here
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this row?", "Food Bank Manager", MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        InventoryInfo entry = (InventoryInfo)grdItems.SelectedItem;
                        InventoryEntry itemToRemove = (from items in dbContext.GetTable<InventoryEntry>()
                                                       where items.DateEntered == entry.DateEntered
                                                       select items).First<InventoryEntry>();
                        dbContext.InventoryEntries.DeleteOnSubmit(itemToRemove);
                        dbContext.SubmitChanges();
                        grdItems.Items.Remove(item);
                        //lstPreview.Items.Remove(strItem);
                        //barcodes.RemoveAt(barcodeValues.IndexOf(strItem));
                        //barcodeValues.Remove(strItem);
                        //txtNumBarcodes.Text = barcodes.Count.ToString();
                        //txtBarcodedata.Text = "";
                        //imgBarcode.Source = null;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Item unable to be deleted at this time", "Food Bank Manager");
                return;
            }
        }
    }
    class InventoryInfo
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

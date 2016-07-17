using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using System.Windows.Shapes;

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for ScannerEmulator.xaml
    /// </summary>
    public partial class ScannerEmulatorDelete : Page
    {
        private Random rand;
        private L2S_FoodBankDBDataContext dbContext;
        public ScannerEmulatorDelete()
        {
            rand = new Random();
            dbContext = new L2S_FoodBankDBDataContext(@"Data Source=DESKTOP-ABVBM4U\SQLEXPRESS;Initial Catalog=FoodBankDB;Integrated Security=True");
            InitializeComponent();
        }
        private void btnRmvFromInv_Click(object sender, RoutedEventArgs e)
        {
            //string[] foodQuery = (from food in dbContext.GetTable<InvBin>() where food.FoodCode == txtFood.Text orderby food.DateEntered select food).ToArray<string>();
            //Gathers info from Bins for oldest to newest
            //IQueryable < InvBin > dateQuery = from database in foodQuery orderby database.DateEntered select database;
            var oldestItem = (from food in dbContext.GetTable<InventoryEntry>() where food.FoodId == txtFood.Text orderby food.DateEntered select food).First();
            txtBinRemove.Text = oldestItem.BinId;
            txtShelfRemove.Text = oldestItem.ShelfId;

            //delete from InvBin based on BinCode
            dbContext.InventoryEntries.DeleteOnSubmit(oldestItem);
            dbContext.SubmitChanges();
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            HomePage h = new HomePage(true);
            NavigationService.Navigate(h);
        }
    }
}

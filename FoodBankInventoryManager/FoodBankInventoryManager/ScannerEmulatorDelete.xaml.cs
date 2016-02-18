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
            dbContext = new L2S_FoodBankDBDataContext(@"Data Source = SQLEXPRESS; AttachDbFilename = C:\Users\YostR\Source\Repos\FoodBankInventoryManager\FoodBankInventoryManager\FoodBankInventoryManager\FoodBankDB.mdf; Integrated Security = True; User Instance = True");
            InitializeComponent();
        }
        /// <summary>
        /// Adds the generated codes to the database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void btnRemoveFromInv_Click(object sender, RoutedEventArgs e)
        //{
        //    Instances of each table
        //    Food foodItem = new Food();
        //    Bin binItem = new Bin();
        //    Shelf shelfItem = new Shelf();
        //    Gets the text fields and sets the item's instance's code to it
        //    foodItem.FoodCode = txtFood.Text;
        //    Sets the changes ready to insert when changes are submitted
        //    dbContext.Foods.DeleteOnSubmit(foodItem);
        //    Submits the changes to the database
        //    dbContext.SubmitChanges();
        //    Closes the window
        //    Close();
        //}

        private void btnRmvFromInv_Click(object sender, RoutedEventArgs e)
        {
            //string[] foodQuery = (from food in dbContext.GetTable<InvBin>() where food.FoodCode == txtFood.Text orderby food.DateEntered select food).ToArray<string>();
            //Gathers info from Bins for oldest to newest
            //IQueryable < InvBin > dateQuery = from database in foodQuery orderby database.DateEntered select database;
            var item = (from food in dbContext.GetTable<InvBin>() where food.FoodCode == txtFood.Text orderby food.DateEntered select food).First();

            txtBinRemove.Text = Convert.ToString(item.BinCode);
            txtShelfRemove.Text = Convert.ToString(item.ShelfCode);

            //delete from InvBin based on BinCode
            dbContext.InvBins.DeleteOnSubmit(item);
            dbContext.SubmitChanges();
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            HomePage h = new HomePage(true);
            this.NavigationService.Navigate(h);
        }
    }
}

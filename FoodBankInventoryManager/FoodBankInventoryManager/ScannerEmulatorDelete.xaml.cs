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
    public partial class ScannerEmulatorDelete : Window
    {
        private Random rand;
        private L2S_FoodBankDBDataContext dbContext;
        public ScannerEmulatorDelete()
        {
            rand = new Random();
            dbContext = new L2S_FoodBankDBDataContext();
            InitializeComponent();
        }

        private void btnFood_Click(object sender, RoutedEventArgs e)
        {
            //Gathers all of the food codes currently in the database
            int[] foodCodes = (from items in dbContext.GetTable<Food>() select items.FoodCode).ToArray<int>();
            //if the table isn't empty, selects a random existing food code, otherwise just generates a new food code
            if (foodCodes.Length > 0)
            {
                txtFood.Text = foodCodes[rand.Next(foodCodes.Length)].ToString();
            }
            else
            {
                txtFood.Text = rand.Next().ToString();
            }
        }

        //private void btnBin_Click(object sender, RoutedEventArgs e)
        //{
        //    //Gathers all of the bin codes currently in the database
        //    int[] binCodes = (from items in dbContext.GetTable<Bin>() select items.BinCode).ToArray<int>();
        //    //if the table isn't empty, selects a random existing bin code, otherwise just generates a new bin code
        //    if (binCodes.Length > 0)
        //    {
        //        txtBin.Text = binCodes[rand.Next(binCodes.Length)].ToString();
        //    }
        //    else
        //    {
        //        txtBin.Text = rand.Next().ToString();
        //    }
        //}

        //private void btnShelf_Click(object sender, RoutedEventArgs e)
        //{
        //    //Gathers all of the shelf codes currently in the database
        //    int[] shelfCodes = (from items in dbContext.GetTable<Shelf>() select items.ShelfCode).ToArray<int>();
        //    //if the table isn't empty, selects a random existing shelf code, otherwise just generates a new shelf code
        //    if (shelfCodes.Length > 0)
        //    {
        //        txtShelf.Text = shelfCodes[rand.Next(shelfCodes.Length)].ToString();
        //    }
        //    else
        //    {
        //        txtShelf.Text = rand.Next(51).ToString();
        //    }
        //}
        /// <summary>
        /// Randomizes codes for all fields
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRandomizeAll_Click(object sender, RoutedEventArgs e)
        {
            btnFood_Click(sender, e);
            //btnBin_Click(sender, e);
            //btnShelf_Click(sender, e);
        }
        /// <summary>
        /// Adds the generated codes to the database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRemoveFromInv_Click(object sender, RoutedEventArgs e)
        {
            //Instances of each table
            Food foodItem = new Food();
            //Bin binItem = new Bin();
            //Shelf shelfItem = new Shelf();
            //Gets the text fields and sets the item's instance's code to it
            foodItem.FoodCode = Convert.ToInt32(txtFood.Text);
            //Sets the changes ready to insert when changes are submitted
            dbContext.Foods.DeleteOnSubmit(foodItem);
            //Submits the changes to the database
            dbContext.SubmitChanges();
            //Closes the window
            Close();
        }
    }
}

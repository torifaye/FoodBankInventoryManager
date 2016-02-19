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
    public partial class ScannerEmulator : Window
    {
        private Random rand;
        private L2S_FoodBankDBDataContext dbContext;
        private string foodCode;
        private int binCode;
        private int shelfCode;
        private int quantity;
        public ScannerEmulator()
        {
            rand = new Random();
            foodCode = "";
            binCode = 0;
            shelfCode = 0;
            dbContext = new L2S_FoodBankDBDataContext(@"C:\Users\YostR\Source\Repos\FoodBankInventoryManager\FoodBankInventoryManager\FoodBankInventoryManager\FoodBankDB.mdf");
            InitializeComponent();
        }

        private void btnFood_Click(object sender, RoutedEventArgs e)
        {
            //Gathers all of the food codes currently in the database
            string[] foodCodes = (from items in dbContext.GetTable<InvBin>() select items.FoodCode).ToArray<string>();
            //a selection of possible chars that a random string can contain
            string possibleChars = "abcdefghijklmnopqrstuvwxyz0123456789";
            //if the table isn't empty, selects a random existing food code, otherwise just generates a new food code
            if (foodCodes.Length > 10)
            {
                foodCode = foodCodes[rand.Next(foodCodes.Length)].ToString();
            }
            else
            {
                foodCode = "";
                //generates a random 10 character code
                for (int i = 0; i < 10; i++)
                {
                    foodCode += possibleChars[rand.Next(possibleChars.Length)];
                }
            }
            txtFood.Text = foodCode;
        }

        private void btnBin_Click(object sender, RoutedEventArgs e)
        {
            //Gathers all of the bin codes currently in the database
            int[] binCodes = (from items in dbContext.GetTable<Bin>() select items.BinCode).ToArray<int>();
            //if the table isn't empty, selects a random existing bin code, otherwise just generates a new bin code
            if (binCodes.Length > 10)
            {
                binCode = binCodes[rand.Next(binCodes.Length)];
            }
            else
            {
                binCode = rand.Next(9999);
            }
            txtBin.Text = binCode.ToString();
        }

        private void btnShelf_Click(object sender, RoutedEventArgs e)
        {
            //Gathers all of the shelf codes currently in the database
            int[] shelfCodes = (from items in dbContext.GetTable<Shelf>() select items.ShelfCode).ToArray<int>();
            //if the table isn't empty, selects a random existing shelf code, otherwise just generates a new shelf code
            if (shelfCodes.Length > 10)
            {
                shelfCode = shelfCodes[rand.Next(shelfCodes.Length)];
            }
            else
            {
                shelfCode = rand.Next(51);
            }
            txtShelf.Text = shelfCode.ToString();
        }
        /// <summary>
        /// Randomizes codes for all fields
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRandomizeAll_Click(object sender, RoutedEventArgs e)
        {
            btnFood_Click(sender, e);
            btnBin_Click(sender, e);
            btnShelf_Click(sender, e);
            quantity = rand.Next(50);
            txtQuantity.Text = quantity.ToString();
        }
        /// <summary>
        /// Adds the generated codes to the database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddToInv_Click(object sender, RoutedEventArgs e)
        {
            foodCode = txtFood.Text;
            binCode = Convert.ToInt32(txtBin.Text);
            shelfCode = Convert.ToInt32(txtShelf.Text);
            quantity = Convert.ToInt32(txtQuantity.Text);
            //An instance object to be added to the database
            InvBin invBin = new InvBin();
            Food food = new Food();
            Bin bin = new Bin();
            Shelf shelf = new Shelf();
            //Sets a volue for all of the columns in the invBin table
            food.FoodCode = foodCode;
            invBin.FoodCode = foodCode;
            bin.BinCode = binCode;
            invBin.BinCode = binCode;
            shelf.ShelfCode = shelfCode;
            invBin.ShelfCode = shelfCode;
            invBin.DateEntered = DateTime.Now;
            invBin.Quantity = quantity;
            //Sets the changes ready to insert when changes are submitted
            dbContext.Foods.InsertOnSubmit(food);
            dbContext.Bins.InsertOnSubmit(bin);
            dbContext.Shelfs.InsertOnSubmit(shelf);
            dbContext.InvBins.InsertOnSubmit(invBin);
            //Submits the changes to the database
            dbContext.SubmitChanges();
            //Closes the window
            Close();
        }
    }
}

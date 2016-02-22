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
        private InvBin invBin;
        private Food food;
        private Bin bin;
        private Shelf shelf;
        public ScannerEmulator()
        {
            rand = new Random();
            foodCode = "";
            binCode = 0;
            shelfCode = 0;
            dbContext = new L2S_FoodBankDBDataContext(@"C:\Users\YostR\Source\Repos\FoodBankInventoryManager\FoodBankInventoryManager\FoodBankInventoryManager\FoodBankDB.mdf");
            invBin = new InvBin();
            food = new Food();
            bin = new Bin();
            shelf = new Shelf();
            InitializeComponent();
        }

        private void btnFood_Click(object sender, RoutedEventArgs e)
        {
            //Gathers all of the food codes currently in the database
            string[] foodCodes = (from items in dbContext.GetTable<InvBin>() select items.FoodCode).ToArray<string>();
            //a selection of possible chars that a random string can contain
            string possibleChars = "abcdefghijklmnopqrstuvwxyz0123456789";
            //if the table isn't empty, selects a random existing food code, otherwise just generates a new food code
            foodCode = "";
            //generates a random 10 character code
            for (int i = 0; i < 10; i++)
            {
                foodCode += possibleChars[rand.Next(possibleChars.Length)];
            }
            txtFood.Text = foodCode;
        }

        private void btnBin_Click(object sender, RoutedEventArgs e)
        {
            //if the table isn't empty, selects a random existing bin code, otherwise just generates a new bin code
            binCode = rand.Next(9999);
            txtBin.Text = binCode.ToString();
        }

        private void btnShelf_Click(object sender, RoutedEventArgs e)
        {
            shelfCode = rand.Next(51);
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
            if ((from items in dbContext.GetTable<Food>() where items.FoodCode == food.FoodCode select items.FoodCode).ToArray<string>().Length == 0)
            {
                dbContext.Foods.InsertOnSubmit(food); 
            }
            if ((from items in dbContext.GetTable<Bin>() where items.BinCode == bin.BinCode select items.BinCode).ToArray<int>().Length == 0)
            {
                dbContext.Bins.InsertOnSubmit(bin);
            }
            if ((from items in dbContext.GetTable<Shelf>() where items.ShelfCode == shelf.ShelfCode select items.ShelfCode).ToArray<int>().Length == 0)
            {
                dbContext.Shelfs.InsertOnSubmit(shelf);
            }
            dbContext.InvBins.InsertOnSubmit(invBin);
            //Submits the changes to the database
            dbContext.SubmitChanges();
            //Closes the window
            Close();
        }
        public InvBin getCapturedData()
        {
            return invBin;
        }
    }
}

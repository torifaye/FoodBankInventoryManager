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
        private string binId;
        private string shelfId;
        private int quantity;
        private DateTime dateEntered;
        private InventoryEntry invEntry;
        private Food food;
        private Bin bin;
        private Shelf shelf;
        public ScannerEmulator()
        {
            rand = new Random();
            foodCode = "";
            binId = "";
            shelfId = "";
            dateEntered = DateTime.Now;
            dbContext = new L2S_FoodBankDBDataContext(@"Data Source=DESKTOP-ABVBM4U\SQLEXPRESS;Initial Catalog=FoodBankDB;Integrated Security=True");
            invEntry = new InventoryEntry();
            food = new Food();
            bin = new Bin();
            shelf = new Shelf();
            InitializeComponent();
        }

        private void btnFood_Click(object sender, RoutedEventArgs e)
        {
            //Gathers all of the food codes currently in the database
            string[] foodCodes = (from items in dbContext.GetTable<InventoryEntry>() select items.FoodId).ToArray<string>();
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
            binId = rand.Next(9999).ToString();
            txtBin.Text = binId.ToString();
        }

        private void btnShelf_Click(object sender, RoutedEventArgs e)
        {
            shelfId = rand.Next(51).ToString();
            txtShelf.Text = shelfId.ToString();
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
            binId = txtBin.Text;
            shelfId = txtShelf.Text;
            quantity = Convert.ToInt32(txtQuantity.Text);
            //An instance object to be added to the database
            //Sets a volue for all of the columns in the invBin table
            food.FoodId = foodCode;
            invEntry.FoodId = foodCode;
            bin.BinId = binId;
            invEntry.BinId = binId;
            shelf.ShelfId = shelfId;
            invEntry.ShelfId = shelfId;
            invEntry.DateEntered = DateTime.Now; 
            invEntry.BinQty = quantity;
            //Sets the changes ready to insert when changes are submitted
            if ((from items in dbContext.GetTable<Food>() where items.FoodId == food.FoodId select items.FoodId).ToArray<string>().Length == 0)
            {
                dbContext.Foods.InsertOnSubmit(food); 
            }
            if ((from items in dbContext.GetTable<Bin>() where items.BinId == bin.BinId select items.BinId).ToArray<string>().Length == 0)
            {
                dbContext.Bins.InsertOnSubmit(bin);
            }
            if ((from items in dbContext.GetTable<Shelf>() where items.ShelfId == shelf.ShelfId select items.ShelfId).ToArray<string>().Length == 0)
            {
                dbContext.Shelfs.InsertOnSubmit(shelf);
            }
            dbContext.InventoryEntries.InsertOnSubmit(invEntry);
            //Submits the changes to the database
            dbContext.SubmitChanges();
            //Closes the window
            Close();
        }
    }
}

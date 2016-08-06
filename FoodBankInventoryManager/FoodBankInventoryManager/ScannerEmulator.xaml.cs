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
using System.Net.Mail;
using System.Configuration;
using System.Text.RegularExpressions;

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for ScannerEmulator.xaml
    /// </summary>
    public partial class ScannerEmulator : Window
    {
        private Random rand;
        private L2S_FoodBankDBDataContext dbContext;
        private DateTime dateEntered;
        private InventoryEntry invEntry;
        private User myCurrentUser;
        private Regex binRegex;
        private Regex shelfRegex;
        private Regex foodRegex;

        private const string APPLICATION_NAME = "SCANNER";

        public ScannerEmulator(User aUser)
        {
            binRegex = new Regex("^[B][0-9]*$");
            shelfRegex = new Regex("^[S][0-9]*$");
            foodRegex = new Regex("^[a-zA-Z ]*$");
            rand = new Random();
            dateEntered = DateTime.Now;
            dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);
            invEntry = new InventoryEntry();
            myCurrentUser = aUser;
            InitializeComponent();
        }

        /// <summary>
        /// Adds the generated codes to the database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddToInv_Click(object sender, RoutedEventArgs e)
        {
            //An instance object to be added to the database
            //Sets a value for all of the columns in the invBin table
            invEntry.FoodName = cbFood.SelectedValue.ToString();
            invEntry.BinId = cbBin.SelectedValue.ToString();
            invEntry.ShelfId = cbShelf.SelectedValue.ToString();
            invEntry.DateEntered = DateTime.Now;
            invEntry.UserId = myCurrentUser.UserId;
            invEntry.BinQty = Convert.ToInt32(txtQuantity.Text);
            invEntry.ApplicationName = APPLICATION_NAME;

            AuditTrail auditRecord = new AuditTrail();
            auditRecord.FoodName = invEntry.FoodName;
            auditRecord.Binid = invEntry.BinId;
            auditRecord.ShelfId = invEntry.ShelfId;
            auditRecord.BinQty = invEntry.BinQty;
            auditRecord.Date_Action_Occured = DateTime.Now;
            auditRecord.UserName = myCurrentUser.LastName + ", " + myCurrentUser.FirstName;
            auditRecord.ApplicationName = APPLICATION_NAME;
            auditRecord.Action = "INSERTION";
            switch (myCurrentUser.AccessLevel)
            {
                case 0: auditRecord.AccessLevel = "Administrator";
                    break;
                case 1: auditRecord.AccessLevel = "Standard User";
                    break;
                default:
                    break;
            }
            ////Sets the changes ready to insert when changes are submitted
            dbContext.InventoryEntries.InsertOnSubmit(invEntry);
            dbContext.AuditTrails.InsertOnSubmit(auditRecord);
            //Submits the changes to the database
            dbContext.SubmitChanges();
            //Closes the window
            Close();
        }

        //private void txtTempStorage_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    if (binRegex.IsMatch(txtTempStorage.Text))
        //    {
        //        System.Threading.Thread.Sleep(10000);
        //        txtBin.Text = txtTempStorage.Text;
        //    }
        //    else if(shelfRegex.IsMatch(txtTempStorage.Text))
        //    {
        //        System.Threading.Thread.Sleep(10000);
        //        txtShelf.Text = txtTempStorage.Text;
        //    }
        //    else if(foodRegex.IsMatch(txtTempStorage.Text))
        //    {
        //        System.Threading.Thread.Sleep(10000);
        //        txtFood.Text = txtTempStorage.Text;
        //    }
        //    txtTempStorage.Text = "";
        //}

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            cbFood.ItemsSource = (from foods in dbContext.GetTable<Food>()
                                  select foods.FoodName).ToList<String>();
            cbBin.ItemsSource = (from bins in dbContext.GetTable<Bin>()
                                 select bins.BinId).ToList<String>();
            cbShelf.ItemsSource = (from shelves in dbContext.GetTable<Shelf>()
                                   select shelves.ShelfId).ToList<String>();
//#if DEBUG
//            txtTempStorage.Visibility = Visibility.Visible;
//            txtTempStorage.Width = 100;
//            txtTempStorage.HorizontalAlignment = HorizontalAlignment.Left;
//            txtTempStorage.Margin = new Thickness(75, 0, 0, 0);
//            btnAddToInv.HorizontalAlignment = HorizontalAlignment.Right;
//            btnAddToInv.Margin = new Thickness(0, 0, 75, 0);
//#endif
//            txtTempStorage.Focus();
        }
    }
}

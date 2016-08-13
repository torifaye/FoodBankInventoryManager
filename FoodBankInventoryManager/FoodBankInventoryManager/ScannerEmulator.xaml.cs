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

        private DateTime lastKeyPress;

        private List<String> textStream;

        private const string APPLICATION_NAME = "SCANNER";

        public ScannerEmulator(User aUser)
        {
            dateEntered = DateTime.Now;
            dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);
            invEntry = new InventoryEntry();
            myCurrentUser = aUser;

            lastKeyPress = new DateTime(0);
            textStream = new List<String>();
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
            invEntry.ItemQty = Convert.ToInt32(txtQuantity.Text);
            Food associatedFood = (from foods in dbContext.GetTable<Food>()
                                   where foods.FoodName == invEntry.FoodName
                                   select foods).First();
            associatedFood.Quantity += invEntry.ItemQty;
            invEntry.ApplicationName = APPLICATION_NAME;

            AuditEntry auditRecord = new AuditEntry();
            auditRecord.FoodName = invEntry.FoodName;
            auditRecord.BinId = invEntry.BinId;
            auditRecord.ShelfId = invEntry.ShelfId;
            auditRecord.ItemQty = invEntry.ItemQty;
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
            dbContext.AuditEntries.InsertOnSubmit(auditRecord);
            //Submits the changes to the database
            dbContext.SubmitChanges();
            //Closes the window
            Close();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> entries = (from items in dbContext.GetTable<InventoryEntry>()
                                    select items.BinId).ToList();
            cbFood.ItemsSource = (from foods in dbContext.GetTable<Food>()
                                  select foods.FoodName).ToList<String>();
            cbBin.ItemsSource = (from bins in dbContext.GetTable<Bin>()
                                 where !entries.Contains(bins.BinId)
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

        private void txtTempStorage_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            TimeSpan elasped = DateTime.Now - lastKeyPress;
            if (elasped.TotalMilliseconds > 1000)
            {
                textStream.Clear();
            }
            if ((e.Key >= Key.D0) && (e.Key <= Key.D9))
            {
                textStream.Add(e.Key.ToString()[1].ToString()); 
            }
            else
            {
                textStream.Add(e.Key.ToString());
            }
            lastKeyPress = DateTime.Now;
            if (e.Key == Key.Return && textStream.Count > 1)
            {
                String barcodeData = String.Join("", textStream);
                barcodeData = barcodeData.Substring(0, barcodeData.IndexOf("Return"));

                String nums = "0123456789";
                if (barcodeData[0] == 'B' && nums.Contains(barcodeData[1]))
                {
                    txtBin.Text = barcodeData.ToString();
                }
                else if (barcodeData[0] == 'S' && nums.Contains(barcodeData[1]))
                {
                    txtShelf.Text = barcodeData.ToString();
                }
                else
                {
                    txtFood.Text = UppercaseFirst(barcodeData.ToString().ToLower());
                }
            }
        }
        static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for ScannerEmulator.xaml
    /// </summary>
    public partial class ScannerEmulator : Window
    {
        private readonly L2S_FoodBankDBDataContext dbContext;
        private DateTime dateEntered;
        private readonly InventoryEntry invEntry;
        private readonly User myCurrentUser;
        private bool readyToSubmit;

        private DateTime lastKeyPress;

        private readonly List<string> textStream;

        private const string APPLICATION_NAME = "SCANNER";

        public ScannerEmulator(User aUser)
        {
            dateEntered = DateTime.Now;
            dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);
            invEntry = new InventoryEntry();
            myCurrentUser = aUser;

            lastKeyPress = new DateTime(0);
            textStream = new List<String>();
            readyToSubmit = true;
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
            if (Validate(cbBin.Text))
            {
                invEntry.BinId = cbBin.Text; 
            }
            else
            {
                readyToSubmit = false;
            }
            if (Validate(cbShelf.Text))
            {
                invEntry.ShelfId = cbShelf.Text; 
            }
            else
            {
                readyToSubmit = false;
            }
            invEntry.DateEntered = DateTime.Now;
            invEntry.UserId = myCurrentUser.UserId;
            if (Validate(txtQuantity.Text))
            {
                invEntry.ItemQty = Convert.ToInt32(txtQuantity.Text);
                
            }
            else
            {
                readyToSubmit = false;
            }
            if (Validate(cbFood.Text))
            {
                invEntry.FoodName = cbFood.Text;

                Food associatedFood = (from foods in dbContext.GetTable<Food>()
                                       where foods.FoodName == invEntry.FoodName
                                       select foods).First();
                associatedFood.Quantity += invEntry.ItemQty;
            }
            else
            {
                readyToSubmit = false;
            }
            invEntry.ApplicationName = APPLICATION_NAME;

            AuditEntry auditRecord = new AuditEntry
            {
                FoodName = invEntry.FoodName,
                BinId = invEntry.BinId,
                ShelfId = invEntry.ShelfId,
                ItemQty = invEntry.ItemQty,
                Date_Action_Occured = DateTime.Now,
                UserName = myCurrentUser.LastName + ", " + myCurrentUser.FirstName,
                ApplicationName = APPLICATION_NAME,
                Action = "INSERTION"
            };
            switch (myCurrentUser.AccessLevel)
            {
                case 0: auditRecord.AccessLevel = "Administrator";
                    break;
                case 1: auditRecord.AccessLevel = "Standard User";
                    break;
                default:
                    break;
            }
            if (!readyToSubmit)
            {
                MessageBox.Show("Please don't leave any field blank.", "Inventory Manager Error System");
                return;
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
            //Populates comboboxes with possible entries based on database content
            List<string> entries = (from items in dbContext.GetTable<InventoryEntry>()
                                    select items.BinId).ToList();
            cbFood.ItemsSource = (from foods in dbContext.GetTable<Food>()
                                  select foods.FoodName).ToList();
            cbBin.ItemsSource = (from bins in dbContext.GetTable<Bin>()
                                 where !entries.Contains(bins.BinId)
                                 select bins.BinId).ToList();
            cbShelf.ItemsSource = (from shelves in dbContext.GetTable<Shelf>()
                                   select shelves.ShelfId).ToList();
        }
        /// <summary>
        /// Fills out textboxes based on keyboard input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                barcodeData = barcodeData.Substring(0, barcodeData.IndexOf("Return", StringComparison.Ordinal));

                String nums = "0123456789";
                if (barcodeData[0] == 'B' && nums.Contains(barcodeData[1]))
                {
                    cbBin.Text = barcodeData;
                }
                else if (barcodeData[0] == 'S' && nums.Contains(barcodeData[1]))
                {
                    cbShelf.Text = barcodeData;
                }
                else
                {
                    cbFood.Text = UppercaseFirst(barcodeData.ToLower());
                }
            }
        }
        /// <summary>
        /// Capitalizes first letter of a word
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
        private static bool Validate(string content)
        {
            return !(string.IsNullOrWhiteSpace(content) || string.IsNullOrEmpty(content));
        }
    }
}

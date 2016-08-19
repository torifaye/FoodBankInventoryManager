using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Configuration;
using System.Windows.Input;

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for DeletionManagementWindow.xaml
    /// </summary>
    public partial class DeletionManagementWindow : Window
    {
        private readonly User myCurrentUser;
        private readonly InventoryInfo selectedEntry;

        private List<InventoryInfo> individualEntries;
        private bool isScanned;

        private DateTime lastKeyPress;
        private List<string> textStream;

        private readonly L2S_FoodBankDBDataContext dbContext;

        private const string APPLICATION_NAME = "INVENTORY_TRACKER";

        /// <summary>
        /// Constructs a deletion management window object
        /// </summary>
        /// <param name="aUser">User that is currently logged in</param>
        /// <param name="anEntry">Entry item that will be looked at (a food that is in multiple bins)</param>
        /// <param name="isScanned">Whether the user got to this page through scanning a bin code</param>
        public DeletionManagementWindow(User aUser, InventoryInfo anEntry, bool isScanned)
        {
            myCurrentUser = aUser;
            selectedEntry = anEntry;
            this.isScanned = isScanned;
            individualEntries = new List<InventoryInfo>();
            lastKeyPress = new DateTime(0);
            dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /*Gets all of the inventory entries that are condensed in the InventoryInfo object that was passed
             * to the constructor of this page
             */ 
            individualEntries = (from entries in dbContext.GetTable<InventoryEntry>()
                                 where entries.FoodName == selectedEntry.FoodName
                                 select new InventoryInfo
                                 {
                                     FoodName = entries.FoodName,
                                     BinId = entries.BinId,
                                     ShelfId = entries.ShelfId,
                                     DateEntered = entries.DateEntered,
                                     Quantity = entries.ItemQty
                                 }).ToList();
            //Sets the data source of the datagrid to the previous linq query
            gridItems.ItemsSource = individualEntries;
        }
        /// <summary>
        /// Event processor for when a user clicks the delete context menu item (which can be shown by 
        /// right clicking a datagrid row)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RowContMenuDel_Click(object sender, RoutedEventArgs e)
        {
            //Grabs the InventoryInfo item stored in the datagrid row that was right-clicked
            InventoryInfo selectedItem = (InventoryInfo)gridItems.SelectedValue;

            //Make sure that the user isn't clicking on a null item (shouldn't ever happen)
            if (selectedItem != null)
            {
                //Grabs the inventory entry to delete based on the foodname and date item was entered
                InventoryEntry entryToDelete = (from entries in dbContext.GetTable<InventoryEntry>()
                                                where entries.FoodName == selectedItem.FoodName
                                                && entries.DateEntered == selectedItem.DateEntered
                                                select entries).First();
                //Gets the food item associated with the inventory entry that the user clicks
                Food associatedFoodItem = (from items in dbContext.GetTable<Food>()
                                           where items.FoodName == entryToDelete.FoodName
                                           select items).First();
                //Removes the quantity associated with that inventory entry from the total quantity of the food
                associatedFoodItem.Quantity -= entryToDelete.ItemQty;
                
                //Removes item from list that the datagrid is bounded to and updates datagrid
                individualEntries.Remove(selectedItem);
                gridItems.ItemsSource = individualEntries;
                gridItems.Items.Refresh();

                //Creates a record in the audit trail
                AuditEntry auditRecord = new AuditEntry
                {
                    Action = "DELETION",
                    ApplicationName = APPLICATION_NAME,
                    BinId = entryToDelete.BinId,
                    ItemQty = entryToDelete.ItemQty,
                    Date_Action_Occured = DateTime.Now,
                    FoodName = entryToDelete.FoodName,
                    ShelfId = entryToDelete.ShelfId,
                    UserName = myCurrentUser.LastName + ", " + myCurrentUser.FirstName
                };
                switch (myCurrentUser.AccessLevel)
                {
                    case 0:
                        auditRecord.AccessLevel = "Administrator";
                        break;
                    case 1:
                        auditRecord.AccessLevel = "Standard User";
                        break;
                    default:
                        break;
                }
                //Removes inventory entry from database and adds audit record
                dbContext.InventoryEntries.DeleteOnSubmit(entryToDelete);
                dbContext.AuditEntries.InsertOnSubmit(auditRecord);
                dbContext.SubmitChanges();
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TimeSpan elasped = DateTime.Now - lastKeyPress;
            if (elasped.TotalMilliseconds > 100)
            {
                textStream.Clear();
            }
            if ((e.Key >= Key.D0) && (e.Key <= Key.D9))
            {
                textStream.Add(e.Key.ToString()[1].ToString());
            }
            else if (e.Key == Key.LeftShift)
            {
                return;
            }
            else if (e.Key == Key.Space)
            {
                textStream.Add(" ");
            }
            else
            {
                textStream.Add(e.Key.ToString());
            }
            lastKeyPress = DateTime.Now;
            if (e.Key == Key.Tab && textStream.Count > 1)
            {
                string barcodeData = string.Join("", textStream).TrimEnd();
                barcodeData = barcodeData.Substring(0, barcodeData.IndexOf("Tab", StringComparison.Ordinal));

                string nums = "0123456789";

                if (barcodeData[0] == 'B' && nums.Contains(barcodeData[1]))
                {
                    List<InventoryInfo> scannedEntry = (from entries in dbContext.GetTable<InventoryEntry>()
                                                        where entries.FoodName == (from items in dbContext.GetTable<InventoryEntry>()
                                                                                   where items.BinId == barcodeData
                                                                                   select items.FoodName).First()
                                                        select new InventoryInfo
                                                        {
                                                            FoodName = entries.FoodName,
                                                            DateEntered = entries.DateEntered,
                                                            BinId = entries.BinId,
                                                            ShelfId = entries.ShelfId,
                                                            Quantity = entries.ItemQty
                                                        }).ToList();
                        if (myCurrentUser.AccessLevel == 0)
                        {
                            try
                            {
                                if (sender != null)
                                {
                                    InventoryInfo selectedItem = scannedEntry.First();

                                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this row?", "Food Bank Manager", MessageBoxButton.YesNo);

                                    if (result == MessageBoxResult.Yes)
                                    {
                                        if (!string.IsNullOrEmpty(selectedItem.FoodName))
                                        {
                                            InventoryEntry entryToDelete = (from items in dbContext.GetTable<InventoryEntry>()
                                                                            where items.FoodName == selectedItem.FoodName
                                                                            select items).First<InventoryEntry>();

                                            Food associatedFoodItem = (from items in dbContext.GetTable<Food>()
                                                                       where items.FoodName == entryToDelete.FoodName
                                                                       select items).First();

                                            associatedFoodItem.Quantity -= entryToDelete.ItemQty;

                                            AuditEntry auditRecord = new AuditEntry();
                                            auditRecord.Action = "DELETION";
                                            auditRecord.ApplicationName = APPLICATION_NAME;
                                            auditRecord.BinId = entryToDelete.BinId;
                                            auditRecord.ItemQty = -entryToDelete.ItemQty;
                                            auditRecord.Date_Action_Occured = DateTime.Now;
                                            auditRecord.FoodName = entryToDelete.FoodName;
                                            auditRecord.ShelfId = entryToDelete.ShelfId;
                                            auditRecord.UserName = myCurrentUser.LastName + ", " + myCurrentUser.FirstName;
                                            switch (myCurrentUser.AccessLevel)
                                            {
                                                case 0:
                                                    auditRecord.AccessLevel = "Administrator";
                                                    break;
                                                case 1:
                                                    auditRecord.AccessLevel = "Standard User";
                                                    break;
                                                default:
                                                    break;
                                            }
                                            dbContext.InventoryEntries.DeleteOnSubmit(entryToDelete);
                                            dbContext.AuditEntries.InsertOnSubmit(auditRecord);
                                            dbContext.SubmitChanges();
                                        }
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.ToString());
                                MessageBox.Show("Item unable to be deleted at this time", "Food Bank Manager Error System");
                                return;

                            }
                        }
                        else
                        {
                            MessageBox.Show("Sorry, you do not have the right permission level to remove this item.");
                            return;
                        }
                }
            }
        }
    }
}

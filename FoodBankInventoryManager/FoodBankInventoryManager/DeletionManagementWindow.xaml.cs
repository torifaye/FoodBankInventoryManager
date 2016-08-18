using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Configuration;

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

        
    }
}

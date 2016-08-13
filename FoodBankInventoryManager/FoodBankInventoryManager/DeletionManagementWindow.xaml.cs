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
using System.Configuration;

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for DeletionManagementWindow.xaml
    /// </summary>
    public partial class DeletionManagementWindow : Window
    {
        private User myCurrentUser;
        private InventoryInfo selectedEntry;

        private List<InventoryInfo> individualEntries;

        private L2S_FoodBankDBDataContext dbContext;

        private const string APPLICATION_NAME = "INVENTORY_TRACKER";

        public DeletionManagementWindow(User aUser, InventoryInfo anEntry)
        {
            myCurrentUser = aUser;
            selectedEntry = anEntry;
            individualEntries = new List<InventoryInfo>();
            dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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
            gridItems.ItemsSource = individualEntries;
        }
        private void RowContMenuDel_Click(object sender, RoutedEventArgs e)
        {
            InventoryInfo selectedItem = (InventoryInfo)gridItems.SelectedValue;
            if (selectedItem != null)
            {
                InventoryEntry entryToDelete = (from entries in dbContext.GetTable<InventoryEntry>()
                                                where entries.FoodName == selectedItem.FoodName
                                                select entries).First();
                Food associatedFoodItem = (from items in dbContext.GetTable<Food>()
                                           where items.FoodName == entryToDelete.FoodName
                                           select items).First();
                associatedFoodItem.Quantity -= entryToDelete.ItemQty;
                                           
                individualEntries.Remove(selectedItem);
                gridItems.ItemsSource = individualEntries;
                gridItems.Items.Refresh();

                AuditEntry auditRecord = new AuditEntry();
                auditRecord.Action = "DELETION";
                auditRecord.ApplicationName = APPLICATION_NAME;
                auditRecord.BinId = entryToDelete.BinId;
                auditRecord.ItemQty = entryToDelete.ItemQty;
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

        
    }
}

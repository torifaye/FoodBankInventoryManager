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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Collections;

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for StatisticsPage.xaml
    /// </summary>
    public partial class InventoryReportingPage : Page
    {
        private L2S_FoodBankDBDataContext dbContext;
        private User myCurrentUser;
        List<InventoryInfo> currentInventory;
        List<MinWatchInfo> watchList;

        private const string APPLICATION_NAME = "INVENTORY TRACKER";
        public InventoryReportingPage(User aUser)
        {
            InitializeComponent();
            myCurrentUser = aUser;
            currentInventory = new List<InventoryInfo>();
            watchList = new List<MinWatchInfo>();
            dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            currentInventory = (from items in dbContext.GetTable<InventoryEntry>()
                                select new InventoryInfo
                                {
                                    FoodName = items.FoodName,
                                    DateEntered = items.DateEntered,
                                    BinId = String.Join(", ", (from bins in dbContext.GetTable<InventoryEntry>()
                                            where bins.FoodName == items.FoodName
                                            select bins.BinId).ToList()),
                                    ShelfId = items.ShelfId,
                                    Quantity = (from foods in dbContext.GetTable<Food>()
                                                where foods.FoodName == items.FoodName
                                                select foods.Quantity).First()
                                }).ToList();
            grdItems.ItemsSource = currentInventory.GroupBy(i => i.FoodName).Select(g => g.First()).ToList();
            txtItemCount.Text = currentInventory.ToArray<InventoryInfo>().Length.ToString();

            List<InventoryEntry> entireInv = (from items in dbContext.GetTable<InventoryEntry>()
                                        select items).ToList();
            List<Food> allFoods = (from foods in dbContext.GetTable<Food>()
                                   select foods).ToList();
            //watchList = (from items in dbContext.GetTable<Food>()
            //                                where items.AverageQty * ((from entries in dbContext.GetTable<InventoryEntry>()
            //                                                           where entries.FoodName == items.FoodName
            //                                                           select entries.ItemQty).ToList<int>().Sum())
            //                                      < items.MinimumQty
            //                                select new MinWatchInfo
            //                                {
            //                                    FoodName = items.FoodName,
            //                                    CurrentQuantity = items.AverageQty * ((from entries in dbContext.GetTable<InventoryEntry>()
            //                                                                           where entries.FoodName == items.FoodName
            //                                                                           select entries.ItemQty).ToList().Sum()),
            //                                    MinThreshold = items.MinimumQty
            //                                }).ToList();
            //gridMinWatch.ItemsSource = watchList;

        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            HomePage h = new HomePage(myCurrentUser);
            NavigationService.Navigate(h);
        }

        private void grdItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //0 = Admin, 1 = Standard
            if (myCurrentUser.AccessLevel == 0)
            {
                try
                {
                    if (sender != null)
                    {
                        //TODO: Handle the user selecting multiple items
                        InventoryInfo selectedItem = ((InventoryInfo)grdItems.SelectedValue);

                        MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this row?", "Food Bank Manager", MessageBoxButton.YesNo);

                        if (result == MessageBoxResult.Yes)
                        {
                            if (!String.IsNullOrEmpty(selectedItem.FoodName))
                            {
                                InventoryEntry entryToDelete = (from items in dbContext.GetTable<InventoryEntry>()
                                                                where items.DateEntered == selectedItem.DateEntered
                                                                && items.FoodName == selectedItem.FoodName
                                                                && items.ShelfId == selectedItem.ShelfId
                                                                && items.BinId == selectedItem.BinId
                                                                && items.ItemQty == selectedItem.Quantity
                                                                select items).First<InventoryEntry>();
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
                            currentInventory.Remove((InventoryInfo)selectedItem);
                            grdItems.Items.Refresh();
                            //TODO: Get the minwatch list to update when a user removes an item immediately
                            gridMinWatch.Items.Refresh();
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
                    MessageBox.Show("Item unable to be deleted at this time", "Food Bank Manager");
                    return;
                } 
            }
            else
            {
                return;
            }
        }
    }
    public class InventoryInfo
    {
        public string FoodName
        {
            get; set;
        }
        public DateTime DateEntered
        {
            get; set;
        }
        public string BinId
        {
            get; set;
        }
        public string ShelfId
        {
            get; set;
        }
        public int Quantity
        {
            get; set;
        }
    }
    public class MinWatchInfo
    {
        public string FoodName
        {
            get; set;
        }
        public int CurrentQuantity
        {
            get; set;
        }
        public int MinThreshold
        {
            get; set;
        }
    }
    public class DistinctEntryComparer : IEqualityComparer<InventoryInfo>
    {
        public bool Equals(InventoryInfo item1, InventoryInfo item2)
        {
            return item1.BinId == item2.BinId
                && item1.FoodName == item2.FoodName;
        }
        public int GetHashCode(InventoryInfo item)
        {
            return item.BinId.GetHashCode()
                ^ item.DateEntered.GetHashCode()
                ^ item.FoodName.GetHashCode()
                ^ item.Quantity.GetHashCode()
                ^ item.ShelfId.GetHashCode();
        }
    }
}

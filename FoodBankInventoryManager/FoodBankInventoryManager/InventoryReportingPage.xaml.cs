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

        private const string APPLICATION_NAME = "INVENTORY_TRACKER";
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
            updateDataGrids();

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

        private List<InventoryInfo> getCurrentInventory()
        {
            return (from items in dbContext.GetTable<InventoryEntry>()
                    select new InventoryInfo
                    {
                        FoodName = items.FoodName,
                        DateEntered = items.DateEntered,
                        BinId = String.Join(", ", (from bins in dbContext.GetTable<InventoryEntry>()
                                                   where bins.FoodName == items.FoodName
                                                   select bins.BinId).ToList()),
                        ShelfId = String.Join(", ", (from shelves in dbContext.GetTable<InventoryEntry>()
                                                     where shelves.FoodName == items.FoodName
                                                     select shelves.ShelfId).ToList().Distinct()),
                        Quantity = (from foods in dbContext.GetTable<Food>()
                                    where foods.FoodName == items.FoodName
                                    select foods.Quantity).First()
                    }).GroupBy(i => i.FoodName).Select(g => g.First()).ToList();
        }
        private List<MinWatchInfo> getCurrentMinWatchList()
        {
            List<Food> belowThreshold = (from foods in dbContext.GetTable<Food>()
                                         where foods.Quantity < foods.MinimumQty
                                         select foods).ToList();
            return (from items in dbContext.GetTable<InventoryEntry>()
                    where belowThreshold.Contains((from foods in dbContext.GetTable<Food>()
                                                   where foods.FoodName == items.FoodName
                                                   select foods).First())
                    select new MinWatchInfo
                    {
                        FoodName = items.FoodName,
                        CurrentQuantity = (from foods in dbContext.GetTable<Food>()
                                           where foods.FoodName == items.FoodName
                                           select foods.Quantity).First(),
                        MinThreshold = (from foods in dbContext.GetTable<Food>()
                                        where foods.FoodName == items.FoodName
                                        select foods.MinimumQty).First()
                    }).ToList();
        }
        private void RowContMenuDel_Click(object sender, RoutedEventArgs e)
        {
            //TODO Allow user to decide what bins they want to delete
            if ((from entries in dbContext.GetTable<InventoryEntry>()
                 where entries.FoodName == ((InventoryInfo)gridItems.SelectedValue).FoodName
                 select entries)
                 .ToList().Count > 1)
            {
                DeletionManagementWindow d = new DeletionManagementWindow(myCurrentUser, (InventoryInfo)gridItems.SelectedValue);
                d.ShowInTaskbar = false;
                d.Owner = Application.Current.MainWindow;
                d.ShowDialog();
                updateDataGrids();
            }
            else
            {
                if (myCurrentUser.AccessLevel == 0)
                {
                    try
                    {
                        if (sender != null)
                        {
                            //TODO: Handle the user selecting multiple items
                            InventoryInfo selectedItem = ((InventoryInfo)gridItems.SelectedValue);

                            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this row?", "Food Bank Manager", MessageBoxButton.YesNo);

                            if (result == MessageBoxResult.Yes)
                            {
                                if (!String.IsNullOrEmpty(selectedItem.FoodName))
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
                                currentInventory.Remove((InventoryInfo)selectedItem);
                                updateDataGrids();
                                
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
                    return;
                }
            }
        }
        private void updateDataGrids()
        {
            currentInventory = getCurrentInventory();
            gridItems.ItemsSource = currentInventory;
            txtItemCount.Text = currentInventory.Count.ToString();
            gridItems.Items.Refresh();
            gridMinWatch.ItemsSource = getCurrentMinWatchList();
            gridMinWatch.Items.Refresh();
        }
        private void gridItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGrid grid = sender as DataGrid;

                InventoryInfo selectedRow = (InventoryInfo)grid.SelectedItem;
                MessageBox.Show(String.Format("Item Name: {0}\nBins item is in: {1}\nShelves item is on: {2}\nQuantity: {3}\nDate Entered: {4}",
                    selectedRow.FoodName, 
                    selectedRow.BinId, 
                    selectedRow.ShelfId, 
                    selectedRow.Quantity, 
                    selectedRow.DateEntered), "Entry Info");
            }
            else
            {
                throw new NullReferenceException();
            }
        }

        private void btnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            ExcelExporter<ReportEntry, ReportEntries> exporter =
                new ExcelExporter<ReportEntry, ReportEntries>();
            exporter.dataToPrint = (from entries in dbContext.GetTable<InventoryEntry>()
                                    select new ReportEntry
                                    {
                                        Shelf = entries.ShelfId,
                                        Bin = entries.BinId,
                                        Recorded_Food = entries.FoodName,
                                        Actual_Food = "",
                                        Recorded_Quantity = entries.ItemQty,
                                        Actual_Quantity = ""
                                    }).ToList();
            exporter.GenerateReport();
        }
    }

    public class ReportEntries : List<ReportEntry> { }

    public class ReportEntry
    {
        public string Shelf
        {
            get; set;
        }
        public string Bin
        {
            get; set;
        }
        public string Recorded_Food
        {
            get; set;
        }
        public string Actual_Food
        {
            get; set;
        }
        public int Recorded_Quantity
        {
            get; set;
        }
        public string Actual_Quantity
        {
            get; set;
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

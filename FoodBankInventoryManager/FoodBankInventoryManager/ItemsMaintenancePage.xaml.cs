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

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for ItemMaintenance.xaml
    /// </summary>
    public partial class ItemsMaintenancePage : Page
    {
        private User myCurrentUser;
        private L2S_FoodBankDBDataContext dbContext;

        private List<FoodInfo> allFoods;
        private List<BinInfo> allBins;
        private List<ShelfInfo> allShelves;

        public ItemsMaintenancePage(User aUser)
        {
            myCurrentUser = aUser;
            dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);

            InitializeComponent();
        }

        private void RowContMenuMod_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                FoodInfo selectedItem = (FoodInfo)dgridFood.SelectedValue;
                ModifyFoodWindow m = new ModifyFoodWindow(selectedItem.FoodName, selectedItem.MinimumQty);
                m.Owner = Application.Current.MainWindow;
                m.ShowDialog();
            }
        }

        private void RowContMenuDel_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                FoodInfo selectedItem = (FoodInfo)dgridFood.SelectedValue;
                List<InventoryEntry> matchingFoods = (from foods in dbContext.GetTable<InventoryEntry>()
                                            where foods.FoodName == selectedItem.FoodName
                                            select foods).ToList();
                if (matchingFoods.Count != 0)
                {
                    MessageBox.Show("There are inventory entries containing " + selectedItem.FoodName + ". To prevent " +
                        "data loss please delete those entries before deleting this item.", "Unable to Delete",MessageBoxButton.OK);
                }
                else
                {
                    allFoods.Remove(selectedItem);
                    dgridFood.Items.Refresh();
                    Food foodToBeRemoved = (from foods in dbContext.GetTable<Food>()
                                            where foods.FoodName == selectedItem.FoodName
                                            select foods).First();
                    dbContext.Foods.DeleteOnSubmit(foodToBeRemoved);
                    dbContext.SubmitChanges();
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            allFoods = (from foods in dbContext.GetTable<Food>()
                        select new FoodInfo
                        {
                            FoodName = foods.FoodName,
                            MinimumQty = foods.MinimumQty
                        }).ToList();
            dgridFood.ItemsSource = allFoods;

            allBins = (from bins in dbContext.GetTable<Bin>()
                       select new BinInfo
                       {
                           BinId = bins.BinId
                       }).ToList();
            dgridBin.ItemsSource = allBins;

            allShelves = (from shelves in dbContext.GetTable<Shelf>()
                          select new ShelfInfo
                          {
                              ShelfId = shelves.ShelfId
                          }).ToList();
            dgridShelf.ItemsSource = allShelves;
        }

        //private void RowContMenuModShelf_Click(object sender, RoutedEventArgs e)
        //{
            
        //}

        private void RowContMenuDelShelf_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                ShelfInfo selectedItem = (ShelfInfo)dgridShelf.SelectedValue;
                List<InventoryEntry> matchingShelves = (from shelves in dbContext.GetTable<InventoryEntry>()
                                                     where shelves.ShelfId == selectedItem.ShelfId
                                                     select shelves).ToList();
                if (matchingShelves.Count != 0)
                {
                    MessageBox.Show("There are inventory entries containing " + selectedItem.ShelfId + ". To prevent " +
                        "data loss please delete those entries before deleting this item.", "Unable to Delete", MessageBoxButton.OK);
                }
                else
                {
                    allShelves.Remove(selectedItem);
                    dgridShelf.Items.Refresh();
                    Shelf shelfToBeRemoved = (from shelves in dbContext.GetTable<Shelf>()
                                          where shelves.ShelfId == selectedItem.ShelfId
                                          select shelves).First();
                    dbContext.Shelfs.DeleteOnSubmit(shelfToBeRemoved);
                    dbContext.SubmitChanges();
                }
            }
        }

        //private void RowContMenuModBin_Click(object sender, RoutedEventArgs e)
        //{
        //    if (sender != null)
        //    {
        //        BinInfo selectedItem = (BinInfo)dgridBin.SelectedValue;
        //        ModifyBinWindow m = new ModifyBinWindow(Convert.ToInt32(selectedItem.BinId.Substring(1)));
        //        m.Owner = Application.Current.MainWindow;
        //        m.ShowDialog();
        //    }
        //}

        private void RowContMenuDelBin_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                BinInfo selectedItem = (BinInfo)dgridBin.SelectedValue;
                List<InventoryEntry> matchingBins = (from bins in dbContext.GetTable<InventoryEntry>()
                                                      where bins.BinId == selectedItem.BinId
                                                      select bins).ToList();
                if (matchingBins.Count != 0)
                {
                    MessageBox.Show("There are inventory entries containing " + selectedItem.BinId + ". To prevent " +
                        "data loss please delete those entries before deleting this item.", "Unable to Delete", MessageBoxButton.OK);
                }
                else
                {
                    allBins.Remove(selectedItem);
                    dgridBin.Items.Refresh();
                    Bin binToBeRemoved = (from bins in dbContext.GetTable<Bin>()
                                            where bins.BinId == selectedItem.BinId
                                            select bins).First();
                    dbContext.Bins.DeleteOnSubmit(binToBeRemoved);
                    dbContext.SubmitChanges();
                }
            }
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            HomePage h = new HomePage(myCurrentUser);
            NavigationService.Navigate(h);
        }
    }

    public class FoodInfo
    {
        public string FoodName
        {
            get; set;
        }
        public int MinimumQty
        {
            get; set;
        }
    }
    public class BinInfo
    {
        public string BinId
        {
            get; set;
        }
    }
    public class ShelfInfo
    {
        public string ShelfId
        {
            get; set;
        }
    }
}

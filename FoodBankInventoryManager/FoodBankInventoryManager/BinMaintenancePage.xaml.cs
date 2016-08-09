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
    /// Interaction logic for BinMaitenance.xaml
    /// </summary>
    public partial class BinMaintenance : Window
    {
        private User myCurrentUser;
        private L2S_FoodBankDBDataContext dbContext;
        private List<String> binList;
        private InventoryEntry currentInvEntry;

        private const string APPLICATION_NAME = "BIN_MAINTENANCE";
        private bool isChanged;        

        public BinMaintenance(User aUser)
        {
            InitializeComponent();
            myCurrentUser = aUser;
            binList = new List<string>();
            isChanged = false;
            dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                object item = cbBinSearch.SelectedItem;
                if (item != null)
                {
                    if (currentInvEntry != null)
                    {
                        if (currentInvEntry.FoodName != cbFoodSearch.SelectedValue.ToString())
                        {
                            isChanged = true;
                            currentInvEntry.FoodName = cbFoodSearch.SelectedValue.ToString();
                        }
                        if (currentInvEntry.ShelfId != cbShelfSearch.SelectedValue.ToString())
                        {
                            isChanged = true;
                            currentInvEntry.ShelfId = cbShelfSearch.SelectedValue.ToString();
                        }
                        if (currentInvEntry.ItemQty != Convert.ToInt32(txtQty.Text))
                        {
                            isChanged = true;
                            currentInvEntry.ItemQty = Convert.ToInt32(txtQty.Text);
                        }
                        if (isChanged)
                        {
                            AuditEntry auditRecord = new AuditEntry();
                            auditRecord.FoodName = currentInvEntry.FoodName;
                            auditRecord.BinId = currentInvEntry.BinId;
                            auditRecord.ShelfId = currentInvEntry.ShelfId;
                            auditRecord.ItemQty = currentInvEntry.ItemQty;
                            auditRecord.Date_Action_Occured = DateTime.Now;
                            auditRecord.UserName = myCurrentUser.LastName + ", " + myCurrentUser.FirstName;
                            auditRecord.ApplicationName = APPLICATION_NAME;
                            auditRecord.Action = "UPDATE";
                            switch (myCurrentUser.AccessLevel)
                            {
                                case 0: auditRecord.AccessLevel = "Administrator";
                                    break;
                                case 1: auditRecord.AccessLevel = "Standard User";
                                    break;
                                default:
                                    break;
                            }
                            dbContext.AuditEntries.InsertOnSubmit(auditRecord);
                            dbContext.SubmitChanges();
                        }
                    }
                    int currentIndex = cbBinSearch.SelectedIndex;
                    //TODO: Somehow make this only go to the next inventory entry with actual stuff at it's location
                    object nextItem = cbBinSearch.Items[(currentIndex + 1) % cbBinSearch.Items.Count];
                    cbBinSearch.SelectedItem = nextItem;
                    binList.Remove(item.ToString());
                    cbBinSearch.Items.Refresh();

                }
            }
            catch (NullReferenceException)
            {
                cbFoodSearch.SelectedValue = "";
                cbShelfSearch.SelectedValue = "";
                txtQty.Text = "";
                MessageBox.Show("There are no more items in the list to check.");
                Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            binList = (from bins in dbContext.GetTable<InventoryEntry>() select bins.BinId).ToList();
            cbBinSearch.ItemsSource = binList;

            cbShelfSearch.ItemsSource = (from shelves in dbContext.GetTable<Shelf>() select shelves.ShelfId).ToList<String>();

            cbFoodSearch.ItemsSource = (from foods in dbContext.GetTable<Food>() select foods.FoodName).ToList<String>();
        }

        private void cbBinSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (binList.Contains(cbBinSearch.SelectedValue.ToString()))
            {
                String binId = cbBinSearch.SelectedItem.ToString();
                currentInvEntry = (from items in dbContext.GetTable<InventoryEntry>()
                                   where binId == items.BinId
                                   select items).First();
                String foodName = currentInvEntry.FoodName;
                String shelfId = currentInvEntry.ShelfId;
                int binQuantity = currentInvEntry.ItemQty;
                cbFoodSearch.SelectedValue = foodName;
                cbShelfSearch.SelectedValue = shelfId;
                txtQty.Text = binQuantity.ToString();
                btnSubmit.IsEnabled = true; 
            }
        }
    }
}

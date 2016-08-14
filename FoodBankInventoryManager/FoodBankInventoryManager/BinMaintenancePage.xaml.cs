using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;
namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for BinMaitenance.xaml
    /// </summary>
    public partial class BinMaintenance : Window
    {
        private readonly User myCurrentUser;
        private readonly L2S_FoodBankDBDataContext dbContext;
        private List<string> binList;
        private InventoryEntry currentInvEntry;

        private const string APPLICATION_NAME = "BIN_MAINTENANCE";
        private bool isChanged;
        private bool allFieldsAreFilled;  

        /// <summary>
        /// Constructs a bin maintenance window object
        /// </summary>
        /// <param name="aUser">User currently logged in</param>
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
            allFieldsAreFilled = true;
            try
            {
                //Get item currently being displayed in combobox
                object item = cbBinSearch.SelectedItem;
                if (item != null)
                {
                    if (currentInvEntry != null)
                    {
                        //Checks to see if any of the fields associated with the current bin are changed
                        if (Validate(cbFoodSearch.Text))
                        {
                            if (currentInvEntry.FoodName != cbFoodSearch.SelectedValue.ToString())
                            {
                                isChanged = true;
                                currentInvEntry.FoodName = cbFoodSearch.SelectedValue.ToString();
                            } 
                        }
                        else
                        {
                            allFieldsAreFilled = false;
                        }
                        if (Validate(cbShelfSearch.Text))
                        {
                            if (currentInvEntry.ShelfId != cbShelfSearch.SelectedValue.ToString())
                            {
                                isChanged = true;
                                currentInvEntry.ShelfId = cbShelfSearch.SelectedValue.ToString();
                            } 
                        }
                        else
                        {
                            allFieldsAreFilled = false;
                        }
                        if (Validate(txtQty.Text))
                        {
                            if (currentInvEntry.ItemQty != Convert.ToInt32(txtQty.Text))
                            {
                                isChanged = true;
                                currentInvEntry.ItemQty = Convert.ToInt32(txtQty.Text);
                            } 
                        }
                        else
                        {
                            allFieldsAreFilled = false;
                        }
                        if (!allFieldsAreFilled)
                        {
                            MessageBox.Show("Please fill out all fields before submitting.");
                            return;
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
                    /*Iterates to the next inventory entry containing food in it and removes the 
                     * item that has just been checked from the combobox list
                     */ 
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
            //Grabs all of the bins containing items and sets the combobox's datasource to it
            binList = (from bins in dbContext.GetTable<InventoryEntry>() select bins.BinId).ToList();
            cbBinSearch.ItemsSource = binList;

            //Gets all possible shelves that the user could change to
            cbShelfSearch.ItemsSource = (from shelves in dbContext.GetTable<Shelf>() select shelves.ShelfId).ToList<String>();

            //Gets all possible foods that the user could change to
            cbFoodSearch.ItemsSource = (from foods in dbContext.GetTable<Food>() select foods.FoodName).ToList<String>();
        }
        /// <summary>
        /// If the user changes what bin they are looking at, it changes the data of the comboboxes to the
        /// associated data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        private static bool Validate(string content)
        {
            return !(string.IsNullOrWhiteSpace(content) || string.IsNullOrEmpty(content));
        }
    }
}

using System.Windows;
using System.Windows.Controls;

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private User myCurrentUser;

        public HomePage(User currentUser)
        {
            InitializeComponent();

            myCurrentUser = currentUser;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //If the user isn't an admin, disables access to more indepth data modification
            if (myCurrentUser.AccessLevel == 1)
            {
                btnModify_Items.IsEnabled = false;
                btnCreateBarcode.IsEnabled = false;
                btnAuditTrail.IsEnabled = false;
            }
        }

        /// <summary>
        /// Navigates to the item modification page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnModify_Items_Click(object sender, RoutedEventArgs e)
        {
            ItemsMaintenancePage i = new ItemsMaintenancePage(myCurrentUser);
            NavigationService.Navigate(i);
        }
        /// <summary>
        /// Navigates to the barcode generator page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCreateBarcode_Click(object sender, RoutedEventArgs e)
        {
            BarcodeCreatorPage b = new BarcodeCreatorPage(myCurrentUser);
            this.NavigationService.Navigate(b);
        }
        /// <summary>
        /// Navigates to the deposit items page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Items_Click(object sender, RoutedEventArgs e)
        {
            DepositPage d = new DepositPage(myCurrentUser);
            this.NavigationService.Navigate(d);
        }
        /// <summary>
        /// Navigates to the inventory report page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStats_Click(object sender, RoutedEventArgs e)
        {
            InventoryReportingPage i = new InventoryReportingPage(myCurrentUser);
            this.NavigationService.Navigate(i);
        }
        /// <summary>
        /// Navigates to the bin maintenance page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBinMaintenance_Click(object sender, RoutedEventArgs e)
        {
            BinMaintenance b = new BinMaintenance(myCurrentUser);
            b.Owner = Application.Current.MainWindow;
            b.ShowDialog();
        }

        /// <summary>
        /// Navigates to the audit trail page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAuditTrail_Click(object sender, RoutedEventArgs e)
        {
            AuditPage a = new AuditPage(myCurrentUser);
            this.NavigationService.Navigate(a);
        }

        private void btnAdd_Items_ToolTipOpening(object sender, ToolTipEventArgs e)
        {

        }

        /// <summary>
        /// Navigates to the window where a user can change their password
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mItemPassword_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Probably move this to the login page and ask for email
            PasswordManagementWindow p = new PasswordManagementWindow(myCurrentUser);
            p.ShowInTaskbar = false;
            p.Owner = Application.Current.MainWindow;
            p.Show();
            Application.Current.MainWindow.Show();
        }
        /// <summary>
        /// Logs current user out
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mItemLogout_Click(object sender, RoutedEventArgs e)
        {
            myCurrentUser = null;
            LoginPage l = new LoginPage();
            this.NavigationService.Navigate(l);
        }

    }
}


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

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        bool admin;

        public HomePage(bool isAdmin)
        {
            InitializeComponent();

            admin = isAdmin;

            if (isAdmin)
            {
                btnAdd_Items.IsEnabled = true;
                btnRemove_Items.IsEnabled = true;
                btnCreateBarcode.IsEnabled = true;
                btnCreateBarcode.Visibility = Visibility.Visible;
            }
            else
            {
                btnAdd_Items.IsEnabled = false;
                btnRemove_Items.IsEnabled = false;
                btnCreateBarcode.IsEnabled = false;
                btnCreateBarcode.Visibility = Visibility.Hidden;
            }
        }

        private void btnRemove_Items_Click(object sender, RoutedEventArgs e)
        {
            ScannerEmulatorDelete w = new ScannerEmulatorDelete();
            this.NavigationService.Navigate(w);
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            if (helpLabels.Visibility == Visibility.Hidden)
            {
                if (admin)
                {
                    helpAdd_Items.Visibility = Visibility.Visible;
                    helpRemove_Items.Visibility = Visibility.Visible;
                    //helpBarcode.Visibility = Visibility.Visible;
                }
                helpStats.Visibility = Visibility.Visible;
                helpMap.Visibility = Visibility.Visible;
                helpLabels.Visibility = Visibility.Visible;
            }
            else
            {
                if (admin)
                {
                    helpAdd_Items.Visibility = Visibility.Hidden;
                    helpRemove_Items.Visibility = Visibility.Hidden;
                    //helpBarcode.Visibility = Visibility.Hidden;
                }
                helpStats.Visibility = Visibility.Hidden;
                helpMap.Visibility = Visibility.Hidden;
                helpLabels.Visibility = Visibility.Hidden;
            }
        }

        private void btnCreateBarcode_Click(object sender, RoutedEventArgs e)
        {
            BarcodeCreatorPage b = new BarcodeCreatorPage();
            this.NavigationService.Navigate(b);
        }

        private void btnAdd_Items_Click(object sender, RoutedEventArgs e)
        {
            DepositPage d = new DepositPage();
            this.NavigationService.Navigate(d);
        }

        private void btnStats_Click(object sender, RoutedEventArgs e)
        {
            StatisticsPage s = new StatisticsPage();
            this.NavigationService.Navigate(s);
        }

        private void bttnHome_Click(object sender, RoutedEventArgs e)
        {
            LoginPage l = new LoginPage();
            this.NavigationService.Navigate(l);
        }
    }
}


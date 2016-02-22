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
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public static bool isAdministrator;
        private const string PASSWORD = "12345";

        public LoginPage()
        {
            InitializeComponent();
        }

        private void btnLoginGuest_Click(object sender, RoutedEventArgs e)
        {
            HomePage h = new HomePage(false);
            isAdministrator = false;
            this.NavigationService.Navigate(h);
        }

        private void pwBoxAdmin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                isCorrectPasswordandAdvance(pwBoxAdmin.Password);
            }
        }

        private void btnLoginAdmin_Click(object sender, RoutedEventArgs e)
        {
            isCorrectPasswordandAdvance(pwBoxAdmin.Password);
        }
        private void isCorrectPasswordandAdvance(string password)
        {
            if (password == PASSWORD)
            {
                HomePage h = new HomePage(true);
                isAdministrator = true;
                this.NavigationService.Navigate(h);
            }
            else
            {
                MessageBox.Show("Password does not match. Please try again.", "Food Bank Manager");
            }
        }
    }
}

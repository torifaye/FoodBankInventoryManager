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
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public static bool isAdministrator;
        private L2S_FoodBankDBDataContext dbContext;

        public LoginPage()
        {
            InitializeComponent();
            dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);
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
                //isCorrectPasswordandAdvance(pwBoxAdmin.Password);
            }
        }

        private void btnLoginAdmin_Click(object sender, RoutedEventArgs e)
        {
            //isCorrectPasswordandAdvance(pwBoxAdmin.Password);
        }
        //private void isCorrectPasswordandAdvance(string password)
        //{
        //    if (password == PASSWORD)
        //    {
        //        HomePage h = new HomePage(true);
        //        isAdministrator = true;
        //        this.NavigationService.Navigate(h);
        //    }
        //    else
        //    {
        //        MessageBox.Show("Password does not match. Please try again.", "Food Bank Manager");
        //    }
        //}

        private void mItemNewAccount_Click(object sender, RoutedEventArgs e)
        {
            CreateAccountWindow c = new CreateAccountWindow();
            this.NavigationService.Navigate(c);
        }

        private void mItemPassword_Click(object sender, RoutedEventArgs e)
        {
            PasswordManagementWindow p = new PasswordManagementWindow();
            this.NavigationService.Navigate(p);
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Create a LINQ query that searches the database for all of the emails
            //TODO: Create a LINQ query that finds the password associated with the email, assuming the email is valid
            //TODO: Research if LINQ queries are secure and if it would be better to make a stored procedure instead
            if (Validate(txtEmail.Text)
                && Validate(pwBoxAdmin.Password)
                )
            {

            }
        }

        private bool Validate(string content)
        {
            return !(String.IsNullOrWhiteSpace(content) || String.IsNullOrEmpty(content));
        }
    }
}

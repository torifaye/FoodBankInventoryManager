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
            //HomePage h = new HomePage(false);
            //isAdministrator = false;
            //this.NavigationService.Navigate(h);
        }

        private void pwBoxAdmin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
            }
        }

        private void btnLoginAdmin_Click(object sender, RoutedEventArgs e)
        {
        }

        private void mItemNewAccount_Click(object sender, RoutedEventArgs e)
        {
        }

        private void mItemPassword_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            loginUser(txtEmail.Text, pwBoxAdmin.Password);
        }
        private void loginUser(string email, string aPassword)
        {
            if (Validate(email)
                && Validate(aPassword)
                )
            {
                var emails = (from items in dbContext.GetTable<User>() where items.Email == txtEmail.Text select items.Email).ToArray<string>();
                if (emails.Length != 0)
                {
                    var password = (from items in dbContext.GetTable<User>() where items.Email == txtEmail.Text select items.Password).ToArray<string>();
                    if (BCrypt.CheckPassword(pwBoxAdmin.Password, password[0]))
                    {
                        User userToBeLoggedIn = (from users in dbContext.GetTable<User>() where users.Email == txtEmail.Text && users.Password == pwBoxAdmin.Password select users).ToArray<User>()[0];
                        HomePage h = new HomePage(userToBeLoggedIn);
                        this.NavigationService.Navigate(h);
                    }
                }
            }
        }
        private bool Validate(string content)
        {
            return !(String.IsNullOrWhiteSpace(content) || String.IsNullOrEmpty(content));
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            CreateAccountWindow c = new CreateAccountWindow();
            this.NavigationService.Navigate(c);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            PasswordManagementWindow p = new PasswordManagementWindow();
            this.NavigationService.Navigate(p);
        }
    }
}

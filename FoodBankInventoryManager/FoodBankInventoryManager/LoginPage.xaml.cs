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
            CreateAccountWindow c = new CreateAccountWindow();
            c.ShowInTaskbar = false;
            c.Owner = Application.Current.MainWindow;
            c.Show();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (!loginUser(txtEmail.Text, pwBoxAdmin.Password))
            {
                MessageBox.Show("The email address or password you provided is incorrect");
            }

            //HomePage h = new HomePage();
            //this.NavigationService.Navigate(h);
        }
        /// <summary>
        /// Attempts to login the user with the provided credentials
        /// </summary>
        /// <param name = "email" > The email the user provides</param>
        /// <param name = "aPassword" > The password the user provides</param>
        /// <returns>Whether or not the user is successfully able to sign in with the provided credentials</returns>
        private bool loginUser(string email, string aPassword)
        {
            if (Validate(email)
                && Validate(aPassword)
                )
            {
                var emails = (from items in dbContext.GetTable<User>() where items.Email == txtEmail.Text select items.Email).ToArray<string>();
                if (emails.Length != 0)
                {
                    var password = (from items in dbContext.GetTable<User>() where items.Email == txtEmail.Text select items.Password).ToArray<string>();
                    bool validPassword = BCrypt.CheckPassword(pwBoxAdmin.Password, password[0]);
                    if (validPassword)
                    {
                        User userToBeLoggedIn = (from users in dbContext.GetTable<User>() where users.Email == txtEmail.Text && validPassword select users).ToArray<User>()[0];
                        HomePage h = new HomePage(userToBeLoggedIn);
                        this.NavigationService.Navigate(h);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            return false;
        }
        private bool Validate(string content)
        {
            return !(String.IsNullOrWhiteSpace(content) || String.IsNullOrEmpty(content));
        }
    }
}

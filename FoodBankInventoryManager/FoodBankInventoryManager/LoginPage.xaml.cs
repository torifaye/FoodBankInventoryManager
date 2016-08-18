using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            User guestUser = new User
            {
                AccessLevel = 2,
                FirstName = "Guest",
                LastName = "Guest"
            };
            HomePage h = new HomePage(guestUser);
            this.NavigationService.Navigate(h);
        }

        private void pwBoxAdmin_KeyDown(object sender, KeyEventArgs e)
        {
            //If user presses enter after entering password, click the login button
            if (e.Key == Key.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }

        private void mItemNewAccount_Click(object sender, RoutedEventArgs e)
        {
            CreateAccountWindow c = new CreateAccountWindow
            {
                ShowInTaskbar = false,
                Owner = Application.Current.MainWindow
            };
            c.ShowDialog();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (!LoginUser(txtEmail.Text, pwBoxAdmin.Password) && (from users in dbContext.GetTable<User>()
                                                                   where users.Email == txtEmail.Text
                                                                   select users).ToList().Count > 0)
            {
                MessageBox.Show("The email address or password you provided is incorrect.", "Inventory Manager Error System");
                pwBoxAdmin.Password = "";
            }   
        }
        /// <summary>
        /// Attempts to login the user with the provided credentials
        /// </summary>
        /// <param name = "email" > The email the user provides</param>
        /// <param name = "aPassword" > The password the user provides</param>
        /// <returns>Whether or not the user is successfully able to sign in with the provided credentials</returns>
        private bool LoginUser(string email, string aPassword)
        {
            if (Validate(email) //Makes sure both fields aren't empty, null, or whitespace
                && Validate(aPassword)
                )
            {
                var emails = (from items in dbContext.GetTable<User>() //grabs emails that match inputted email (should only be one)
                              where items.Email == txtEmail.Text
                              select items.Email).ToArray();
                if (emails.Length != 0)
                {
                    var password = (from items in dbContext.GetTable<User>() //grabs password associated with account that has the provided email
                                    where items.Email == txtEmail.Text
                                    select items.Password).ToArray();
                    bool validPassword = BCrypt.CheckPassword(pwBoxAdmin.Password, password[0]); //checks to see if provided password matches actual password
                    if (validPassword)
                    {
                        User userToBeLoggedIn = (from users in dbContext.GetTable<User>() //Gets user with credentials matching the provided ones
                                                 where users.Email == txtEmail.Text 
                                                 && validPassword
                                                 select users).ToArray()[0];
                        HomePage h = new HomePage(userToBeLoggedIn); //Navigates to application homepage
                        this.NavigationService.Navigate(h);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show("There are no accounts associated with the email you provided.", "Inventory Manager Error System");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Makes sure content isn't null, empty, or just whitespace
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private bool Validate(string content)
        {
            return !(String.IsNullOrWhiteSpace(content) || String.IsNullOrEmpty(content));
        }

        private void mItemPassword_OnClick(object sender, RoutedEventArgs e)
        {
            PasswordManagementWindow p = new PasswordManagementWindow();
            p.ShowInTaskbar = false;
            p.Owner = Application.Current.MainWindow;
            p.Show();
            Application.Current.MainWindow.Show();
        }
    }
}

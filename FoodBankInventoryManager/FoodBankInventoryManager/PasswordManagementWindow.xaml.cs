using System;
using System.Linq;
using System.Windows;
using System.Configuration;

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for PasswordManagementWindow.xaml
    /// </summary>
    public partial class PasswordManagementWindow : Window
    {
        private User myCurrentUser;
        private L2S_FoodBankDBDataContext dbContext;
        public PasswordManagementWindow(User currentUser)
        {
            InitializeComponent();
            myCurrentUser = currentUser;
            dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);
        }
        //Loads name of user wanting to change their password
        private void txtUserName_Loaded(object sender, RoutedEventArgs e)
        {
            txtUserName.Text = myCurrentUser.FirstName + " " + myCurrentUser.LastName;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            //If user enters a password that matches the database, user's password is changed to desired new password
            if (BCrypt.CheckPassword(pwBoxCurrent.Password, myCurrentUser.Password))
            {
                string myHash = "";
                if (Validate(pwBoxNew.Password))
                {
                    string mySalt = BCrypt.GenerateSalt();
                    myHash = BCrypt.HashPassword(pwBoxNew.Password, mySalt);
                }
                else
                {
                    MessageBox.Show("Please enter a password");
                }
                if (Validate(pwBoxConfirm.Password) && BCrypt.CheckPassword(pwBoxConfirm.Password, myHash))
                {
                    //For some reason you have to query for the user instead of being able to use
                    //the one passed to you (i.e. myCurrentUser), or at least that's only what works for me
                    User user = (from users in dbContext.GetTable<User>()
                                 where users.Email == myCurrentUser.Email
                                 select users).First<User>();
                    user.Password = myHash;
                    dbContext.SubmitChanges();
                    MessageBox.Show("Password successfully changed!");
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Incorrect password provided");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Makes sure desired content isn't null, empty, or just whitespace
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private bool Validate(string content)
        {
            return !(String.IsNullOrWhiteSpace(content) || String.IsNullOrEmpty(content));
        }
    }
}

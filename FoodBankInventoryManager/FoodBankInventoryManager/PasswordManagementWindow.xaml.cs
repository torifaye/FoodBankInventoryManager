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

        private void txtUserName_Loaded(object sender, RoutedEventArgs e)
        {
            txtUserName.Text = myCurrentUser.FirstName + " " + myCurrentUser.LastName;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
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
                    myCurrentUser.Password = myHash;
                    MessageBox.Show("Password successfully changed!");
                    string sqlCommand = "UPDATE [dbo].[User] SET [User].[Password] = " + myHash + " WHERE [UserId] = " + myCurrentUser.UserId;
                    dbContext.ExecuteCommand(sqlCommand);
                    dbContext.SubmitChanges();
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

        private bool Validate(string content)
        {
            return !(String.IsNullOrWhiteSpace(content) || String.IsNullOrEmpty(content));
        }
    }
}

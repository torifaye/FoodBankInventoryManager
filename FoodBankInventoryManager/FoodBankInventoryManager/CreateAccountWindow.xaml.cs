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
using System.Net;
using System.Net.Mail;
using NUnit.Framework;

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for CreateAccountWindow.xaml
    /// </summary>
    public partial class CreateAccountWindow : Window
    {
        private bool[] emptyFields;
        private L2S_FoodBankDBDataContext dbContext;
        private User myAccount;


        public CreateAccountWindow()
        {
            InitializeComponent();
            emptyFields = new bool[7];
            dbContext = new L2S_FoodBankDBDataContext();
            myAccount = new User();
        }

        private void cBoxAccessLevel_Loaded(object sender, RoutedEventArgs e)
        {
            List<String> accessLevels = new List<string>();
            accessLevels.Add("Administrator");
            accessLevels.Add("Standard User");
            //Assign the combobox's item source to the list
            cBoxAccessLevel.ItemsSource = accessLevels;
        }

        private void cBoxAccessLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TODO: Based on what the user decides, change their access level in the database
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            myAccount.FirstName = txtFirstName.Text;
            myAccount.LastName = txtLastName.Text;
            //TODO: Confirm that the email that they give is valid
            if (txtEmail.Text == txtConfirmEmail.Text)
            {
                myAccount.Email = txtEmail.Text;
            }
            string mySalt = BCrypt.GenerateSalt();
            string myHash = BCrypt.HashPassword(pwBoxPassword.Password, mySalt);
            if (BCrypt.CheckPassword(pwBoxConfirmPassword.Password, myHash))
            {
                myAccount.Password = myHash;
            }
            //TODO: Set the user's access level based on what they've chosen from the combobox
            //TODO: Insert the user item created during this window's lifespan into the database

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

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

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for CreateAccountWindow.xaml
    /// </summary>
    public partial class CreateAccountWindow : Window
    {
        private Account newUser;
        private bool[] emptyFields;

        public CreateAccountWindow()
        {
            InitializeComponent();
            emptyFields = new bool[7];
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
            if (txtFirstName.Text.ToString().Trim() != null)
            {
                newUser.FirstName = txtFirstName.Text;
                emptyFields[0] = false;
            }
            if (txtLastName.Text.ToString().Trim() != null)
            {
                newUser.LastName = txtLastName.Text;
                emptyFields[1] = false;
            }
            if (txtEmail.Text.ToString().Trim() != null)
            {
                emptyFields[2] = false;
            }
            if (txtConfirmEmail.Text.ToString().Trim() != null)
            {
                emptyFields[3] = false;
            }
            if (pwBoxPassword.Password.ToString().Trim() != null)
            {
                emptyFields[4] = false;
            }
            if (pwBoxConfirmPassword.Password.ToString().Trim() != null)
            {
                emptyFields[5] = false;
            }
            if (cBoxAccessLevel.SelectedIndex > -1)
            {
                emptyFields[6] = false;
            }
            for (int i = 0; i < emptyFields.Length; i++)
            {
                //TODO: MessageBox.Show() the user what fields they have not filled out
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

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

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for CreateAccountWindow.xaml
    /// </summary>
    public partial class CreateAccountWindow : Window
    {
        public CreateAccountWindow()
        {
            InitializeComponent();
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

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

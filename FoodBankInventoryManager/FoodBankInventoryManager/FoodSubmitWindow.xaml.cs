using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for FoodSubmitWindow.xaml
    /// </summary>
    public partial class FoodSubmitWindow : Window
    {
        private string myFoodName;
        private L2S_FoodBankDBDataContext dbContext;

        public FoodSubmitWindow(string foodName)
        {
            myFoodName = foodName;
            dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtFoodName.Text = myFoodName;
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (Validate(txtMinQty.Text) && Validate(txtFoodName.Text))
            {
                Food toBeAdded = new Food();
                toBeAdded.FoodName = myFoodName;
                toBeAdded.MinimumQty = Convert.ToInt32(txtMinQty.Text);
                dbContext.Foods.InsertOnSubmit(toBeAdded);
                dbContext.SubmitChanges();
            }
            else
            {
                MessageBox.Show("Please fill out all fields");
            }
            Close();
        }

        private bool Validate(string content)
        {
            return !(String.IsNullOrWhiteSpace(content) || String.IsNullOrEmpty(content));
        }

        private void txtQty_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !isTextAllowed(e.Text);
        }

        private static bool isTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        /// <summary>
        /// Allows user to cancel adding an item to the database
        /// </summary>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
           MessageBoxResult answer = MessageBox.Show("Are you sure you want to CANCEL adding the current Food item?", "Food Bank Manager", MessageBoxButton.YesNo);

            if (answer.Equals(MessageBoxResult.Yes))
            {
                Close();
                throw new Exception("ExceptionNoFoodToday");
            }
           
        }
    }
}

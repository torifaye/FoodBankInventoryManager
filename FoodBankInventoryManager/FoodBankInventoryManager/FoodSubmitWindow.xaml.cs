using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Configuration;

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for FoodSubmitWindow.xaml
    /// </summary>
    public partial class FoodSubmitWindow : Window
    {
        private readonly string myFoodName;
        private readonly L2S_FoodBankDBDataContext dbContext;

        /// <summary>
        /// Constructs a food submit window object
        /// </summary>
        /// <param name="foodName">food name that is being added to the food table of the database</param>
        public FoodSubmitWindow(string foodName)
        {
            myFoodName = foodName;
            dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);
            InitializeComponent();
        }
        /// <summary>
        /// Open loading of the window, shows the food that the user is about to submit. Is grabbed from 
        /// barcode generator page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtFoodName.Text = myFoodName;
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            //If all of the fields are filled out, submits food item to the database
            if (Validate(txtMinQty.Text) && Validate(txtFoodName.Text))
            {
                Food toBeAdded = new Food
                {
                    FoodName = myFoodName,
                    MinimumQty = Convert.ToInt32(txtMinQty.Text)
                };
                toBeAdded.Quantity += 0;
                dbContext.Foods.InsertOnSubmit(toBeAdded);
                dbContext.SubmitChanges();
            }
            else
            {
                MessageBox.Show("Please fill out all fields.", "Inventory Manager Error System");
            }
            Close();
        }

        /// <summary>
        /// Makes sure any text field isn't null, empty, or whitespace
        /// </summary>
        /// <param name="content">content to be checked against</param>
        /// <returns></returns>
        private static bool Validate(string content)
        {
            return !(string.IsNullOrWhiteSpace(content) || string.IsNullOrEmpty(content));
        }
        /// <summary>
        /// If user is creating a shelf or bin barcode, restricts text input to only numeric
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtQty_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
        /// <summary>
        /// Looks for numeric input only
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        /// <summary>
        /// Allows user to cancel adding an item to the database
        /// </summary>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
           MessageBoxResult answer = MessageBox.Show("Are you sure you want to CANCEL adding the current food item?", "Food Bank Manager", MessageBoxButton.YesNo);

            if (answer.Equals(MessageBoxResult.Yes))
            {
                Close();
                throw new Exception("NoFoodTodayException");
            }
           
        }
    }
}

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
using System.Text.RegularExpressions;
using System.Configuration;

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for ModifyFoodWindow.xaml
    /// </summary>
    public partial class ModifyFoodWindow : Window
    {
        private readonly string _myFoodName;
        private int oldMinQty;

        private readonly L2S_FoodBankDBDataContext _dbContext;

        public ModifyFoodWindow(string aFoodName, int aMinQty)
        {
            _myFoodName = aFoodName;
            oldMinQty = aMinQty;
            _dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);
            InitializeComponent();
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (Validate(txtFoodName.Text))
            {
                if (_myFoodName != txtFoodName.Text)
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to change " + _myFoodName + " to " + txtFoodName.Text + "?", 
                        "Confirm Change", 
                        MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        string newFoodName = txtFoodName.Text;
                        Food changedFood = (from foods in _dbContext.GetTable<Food>() //Queries for old food name, and
                                             where foods.FoodName == _myFoodName      //then sets it to new food name 
                                             select foods).First();
                        changedFood.FoodName = newFoodName;
                                                
                        _dbContext.SubmitChanges();
                        MessageBox.Show(_myFoodName + " successfully changed to " + newFoodName + ".");
                    }
                    else
                    {
                        return;
                    }
                }
                if (Validate(txtMinQty.Text))
                {
                    //if food with matching foodname exists and the desired min quantity is different from current, change the current min quantity to desired
                    if ((from foods in _dbContext.GetTable<Food>()
                         where foods.FoodName == _myFoodName
                         select foods).ToList().Count != 0)
                    {
                        if ((from foods in _dbContext.GetTable<Food>()
                             where foods.FoodName == _myFoodName
                             select foods.MinimumQty).First() != Convert.ToInt32(txtMinQty.Text))
                        {
                            int oldQty = (from foods in _dbContext.GetTable<Food>()
                                          where foods.FoodName == _myFoodName
                                          select foods.MinimumQty).First();
                            MessageBoxResult result = MessageBox.Show("Are you sure you want to change the minimum quantity of " + _myFoodName + "?", 
                                "Confirm Change", 
                                MessageBoxButton.YesNo);
                            if (result == MessageBoxResult.Yes)
                            {
                                string newFoodName = txtFoodName.Text;
                                int newQty = Convert.ToInt32(txtMinQty.Text);
                                Food changedFood = (from foods in _dbContext.GetTable<Food>()
                                                    where foods.FoodName == newFoodName
                                                    select foods).First();
                                changedFood.MinimumQty = newQty;

                                _dbContext.SubmitChanges();
                                MessageBox.Show("Minimum threshold of " + newFoodName + " changed from " + oldQty + " to " + newQty + " successfully!");
                            }
                        } 
                    } 
                }
            }
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        /// <summary>
        /// Makes sure content isn't null, empty, or whitespace
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static bool Validate(string content)
        {
            return !(string.IsNullOrWhiteSpace(content) || string.IsNullOrEmpty(content));
        }
        //Looks for content that is only numeric
        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }
        //Restricts min quantity textbox to numeric
        private void txtMinQty_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtFoodName.Text = _myFoodName;
            txtMinQty.Text = oldMinQty.ToString();
        }
    }
}

﻿using System;
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
        private string myFoodName;
        private int oldMinQty;

        private L2S_FoodBankDBDataContext dbContext;

        public ModifyFoodWindow(string aFoodName, int aMinQty)
        {
            myFoodName = aFoodName;
            oldMinQty = aMinQty;
            dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);
            InitializeComponent();
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (Validate(txtFoodName.Text))
            {
                if (myFoodName != txtFoodName.Text)
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to change " + myFoodName + " to " + txtFoodName.Text + "?", 
                        "Confirm Change", 
                        MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        string newFoodName = txtFoodName.Text;
                        Food changedFood = (from foods in dbContext.GetTable<Food>()
                                             where foods.FoodName == myFoodName
                                             select foods).First();
                        changedFood.FoodName = newFoodName;

                        //List<InventoryEntry> changedEntries = (from entries in dbContext.GetTable<InventoryEntry>()
                        //                                       where entries.FoodName == myFoodName
                        //                                       select entries).ToList();
                        //foreach (InventoryEntry entry in changedEntries)
                        //{
                        //    entry.FoodName = newFoodName;
                        //}
                        
                        dbContext.SubmitChanges();
                        MessageBox.Show(myFoodName + " successfully changed to " + newFoodName + ".");
                    }
                    else
                    {
                        return;
                    }
                }
                if (Validate(txtMinQty.Text))
                {
                    if ((from foods in dbContext.GetTable<Food>() where foods.FoodName == myFoodName select foods).ToList().Count != 0)
                    {
                        if ((from foods in dbContext.GetTable<Food>() where foods.FoodName == myFoodName select foods.MinimumQty).First() != Convert.ToInt32(txtMinQty.Text))
                        {
                            int oldQty = (from foods in dbContext.GetTable<Food>() where foods.FoodName == myFoodName select foods.MinimumQty).First();
                            MessageBoxResult result = MessageBox.Show("Are you sure you want to change the minimum quantity of " + myFoodName + "?", "Confirm Change", MessageBoxButton.YesNo);
                            if (result == MessageBoxResult.Yes)
                            {
                                string newFoodName = txtFoodName.Text;
                                int newQty = Convert.ToInt32(txtMinQty.Text);
                                Food changedFood = (from foods in dbContext.GetTable<Food>()
                                                    where foods.FoodName == newFoodName
                                                    select foods).First();
                                changedFood.MinimumQty = newQty;

                                dbContext.SubmitChanges();
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
        private bool Validate(string content)
        {
            return !(String.IsNullOrWhiteSpace(content) || String.IsNullOrEmpty(content));
        }

        private static bool isTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void txtMinQty_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !isTextAllowed(e.Text);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtFoodName.Text = myFoodName;
            txtMinQty.Text = oldMinQty.ToString();
        }
    }
}
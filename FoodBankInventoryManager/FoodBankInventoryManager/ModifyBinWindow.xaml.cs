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
    /// Interaction logic for ModifyBinWindow.xaml
    /// </summary>
    public partial class ModifyBinWindow : Window
    {
        private int myBinId;
        private L2S_FoodBankDBDataContext dbContext;
        public ModifyBinWindow(int binId)
        {
            myBinId = binId;
            dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtBinNo.Text = myBinId.ToString();
        }

        private void txtBinNo_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !isInputAllowed(e.Text);
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (Validate(txtBinNo.Text))
            {
                if (Convert.ToInt32(txtBinNo.Text) != myBinId)
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to change Bin B" + myBinId + " to B" + txtBinNo.Text + "?", 
                        "Confirm Change", 
                        MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        string newBinId = "B" + txtBinNo.Text;
                        Bin binToBeChanged = (from bins in dbContext.GetTable<Bin>()
                                              where bins.BinId == "B" + myBinId
                                              select bins).First();
                        binToBeChanged.BinId = newBinId;

                        dbContext.SubmitChanges();

                        MessageBox.Show("B" + myBinId + " successfully changed to " + newBinId + ".");
                    }
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private bool isInputAllowed(string input)
        {
            return !(new Regex("[^0-9]+").IsMatch(input));
        }
        private bool Validate(string content)
        {
            return !(String.IsNullOrWhiteSpace(content) || String.IsNullOrEmpty(content));
        }
    }
}

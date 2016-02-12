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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for WithdrawalPage.xaml
    /// </summary>
    public partial class WithdrawalPage : Page
    {
        public WithdrawalPage()
        {
            InitializeComponent();
        }

        private void btnRemoveAll_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Are you sure you want to remove ALL items in the current bin?", "Food Bank Manager", MessageBoxButton.YesNo);
        }
    }
}

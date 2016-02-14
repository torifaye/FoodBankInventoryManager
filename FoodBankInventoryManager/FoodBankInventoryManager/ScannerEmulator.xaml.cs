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
    /// Interaction logic for ScannerEmulator.xaml
    /// </summary>
    public partial class ScannerEmulator : Window
    {
        private Random rand;
        public ScannerEmulator()
        {
            rand = new Random();
            InitializeComponent();
        }

        private void btnFood_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Gather array of current values in database for food and choose a random food code out of that
        }

        private void btnBin_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnShelf_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRandomizeAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAddToInv_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

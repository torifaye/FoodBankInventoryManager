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
using System.Configuration;
namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for BinMaitenance.xaml
    /// </summary>
    public partial class BinMaitenance : Window
    {
        private User myCurrentUser;
        private L2S_FoodBankDBDataContext dbContext;
        private List<String> binList;

        public BinMaitenance(User aUser)
        {
            InitializeComponent();
            myCurrentUser = aUser;
            binList = new List<string>();
            dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            binList = (from bins in dbContext.GetTable<Bin>() select bins.BinId).ToList();
            cbBinSearch.ItemsSource = binList;
        }
    }
}

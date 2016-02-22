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
    /// Interaction logic for StatisticsPage.xaml
    /// </summary>
    public partial class StatisticsPage : Page
    {
        private L2S_FoodBankDBDataContext dbContext;
        public StatisticsPage()
        {
            InitializeComponent();
            dbContext = new L2S_FoodBankDBDataContext(@"Data Source=.\SQLEXPRESS;AttachDbFilename=C:\Users\YostR\Source\Repos\FoodBankInventoryManager\FoodBankInventoryManager\FoodBankInventoryManager\FoodBankDB.mdf;Integrated Security=True;Connect Timeout=30;User Instance=True");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (LoginPage.isAdministrator==true)
            {
                HomePage h = new HomePage(true); //NEEDS TO BE FIXED LATER
                LoginPage.isAdministrator = true;
                this.NavigationService.Navigate(h);
            }
            else
            {
                HomePage h = new HomePage(false); //NEEDS TO BE FIXED LATER
                LoginPage.isAdministrator = false;
                this.NavigationService.Navigate(h);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var inventoryInfo = from items in dbContext.GetTable<InvBin>()
                                where items.FoodCode != null
                                select new InventoryInfo
                                {
                                    FoodCode = items.FoodCode,
                                    DateEntered = items.DateEntered,
                                    Quantity = items.Quantity
                                };
            grdItems.ItemsSource = inventoryInfo;
            txtItemCount.Text = inventoryInfo.ToArray<InventoryInfo>().Length.ToString();
        }
    }
}

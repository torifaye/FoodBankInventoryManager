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
using System.Configuration;

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for AuditPage.xaml
    /// </summary>
    public partial class AuditPage : Page
    {
        private User myCurrentUser;
        private L2S_FoodBankDBDataContext dbContext;

        public AuditPage(User aUser)
        {
            InitializeComponent();
            myCurrentUser = aUser;

            dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            List<AuditInfo> auditEntries = (from entries in dbContext.GetTable<AuditEntry>()
                                                select new AuditInfo
                                                {
                                                    EntryId = entries.AuditEntryId,
                                                    FoodName = entries.FoodName,
                                                    BinId = entries.BinId,
                                                    ShelfId = entries.ShelfId,
                                                    BinQty = entries.ItemQty,
                                                    Date_Action_Occured = entries.Date_Action_Occured,
                                                    UserName = entries.UserName,
                                                    ApplicationName = entries.ApplicationName,
                                                    Action = entries.Action
                                                }).ToList();

            gridAudit.ItemsSource = auditEntries;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            HomePage h = new HomePage(myCurrentUser);
            NavigationService.Navigate(h);
        }

    }
    public class AuditInfo
    {
        public int EntryId
        {
            get; set;
        }
        public string FoodName
        {
            get; set;
        }
        public string BinId
        {
            get; set;
        }
        public string ShelfId
        {
            get; set;
        }
        public int BinQty
        {
            get; set;
        }
        public DateTime Date_Action_Occured
        {
            get; set;
        }
        public string UserName
        {
            get; set;
        }
        public string ApplicationName
        {
            get; set;
        }
        public string Action
        {
            get; set;
        }
    }
}

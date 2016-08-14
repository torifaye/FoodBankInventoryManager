using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

        List<AuditInfo> auditEntries;

        /// <summary>
        /// Constructs an Audit Page
        /// </summary>
        /// <param name="aUser">User that is currenty using the page</param>
        public AuditPage(User aUser)
        {
            InitializeComponent();
            myCurrentUser = aUser;
            //linq2sql database data context object: allows you to modify application's database with linq
            dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            /*
             * Loads every audit record from database and loads it into a object displaying specific 
             * properties of audit record (AuditInfo)
             */
            auditEntries = (from entries in dbContext.GetTable<AuditEntry>()
                                select new AuditInfo
                                {
                                    EntryId = entries.AuditEntryId,
                                    FoodName = entries.FoodName,
                                    BinId = entries.BinId,
                                    ShelfId = entries.ShelfId,
                                    Qty = entries.ItemQty,
                                    Date_Action_Occured = entries.Date_Action_Occured,
                                    UserName = entries.UserName,
                                    ApplicationName = entries.ApplicationName,
                                    Action = entries.Action
                                }).ToList();
            //Set's the datagrid's data source to the previous linq query
            gridAudit.ItemsSource = auditEntries;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            //Navigates back to homepage, with the user using the page as an argument
            HomePage h = new HomePage(myCurrentUser);
            NavigationService.Navigate(h);
        }
        /// <summary>
        /// Upon button press, load data that's currently in the datagrid into an excel spreadsheet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            ExcelExporter<AuditInfo, AuditInfos> auditTrail = new ExcelExporter<AuditInfo, AuditInfos>();
            auditTrail.dataToPrint = auditEntries;
            auditTrail.GenerateReport();
        }
    }
    /// <summary>
    /// Let's you declare a list containing AuditInfo objects with normal object declaration syntax
    /// </summary>
    public class AuditInfos : List<AuditInfo> { }

    /// <summary>
    /// Class that is used to store data from specific (but not all) columns from a linq query
    /// </summary>
    public class AuditInfo
    {
        /// <summary>
        /// Unique identifier for each entry recorded in audit trail
        /// </summary>
        public int EntryId
        {
            get; set;
        }
        /// <summary>
        /// Name of food that is associated with the database modification
        /// </summary>
        public string FoodName
        {
            get; set;
        }
        /// <summary>
        /// Bin ID code that is associated with the database modification
        /// </summary>
        public string BinId
        {
            get; set;
        }
        /// <summary>
        /// Shelf ID code that is associated with the database modification
        /// </summary>
        public string ShelfId
        {
            get; set;
        }
        /// <summary>
        /// Change in quantity of food item associated with database modification
        /// </summary>
        public int Qty
        {
            get; set;
        }
        /// <summary>
        /// Date that the audit entry was recorded in the database
        /// </summary>
        public DateTime Date_Action_Occured
        {
            get; set;
        }
        /// <summary>
        /// User that was logged in and made the database modification
        /// </summary>
        public string UserName
        {
            get; set;
        }
        /// <summary>
        /// Name of the application that the user used to modify the database
        /// </summary>
        public string ApplicationName
        {
            get; set;
        }
        /// <summary>
        /// A string representing how the database was modified (insertion, deletion, modification, etc.)
        /// </summary>
        public string Action
        {
            get; set;
        }
    }
}

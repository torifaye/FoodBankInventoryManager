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
using System.Net;
using System.Net.Mail;

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for CreateAccountWindow.xaml
    /// </summary>
    public partial class CreateAccountWindow : Window
    {
        private bool[] emptyFields;
        private L2S_FoodBankDBDataContext dbContext;
        private User myAccount;
        private int comboBoxChoice;


        public CreateAccountWindow()
        {
            InitializeComponent();
            emptyFields = new bool[7];
            dbContext = new L2S_FoodBankDBDataContext();
            myAccount = new User();
        }

        private void cBoxAccessLevel_Loaded(object sender, RoutedEventArgs e)
        {
            List<String> accessLevels = new List<string>();
            accessLevels.Add("Administrator");
            accessLevels.Add("Standard User");
            //Assign the combobox's item source to the list
            cBoxAccessLevel.ItemsSource = accessLevels;
        }

        private void cBoxAccessLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            comboBoxChoice = cBoxAccessLevel.SelectedIndex;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            //TODO: If the user forgets to feel out a field, don't allow database insert
            //and then alert them
            myAccount.FirstName = txtFirstName.Text;
            myAccount.LastName = txtLastName.Text;
            //TODO: Confirm that the email that they give is valid
            if (txtEmail.Text == txtConfirmEmail.Text)
            {
                myAccount.Email = txtEmail.Text;
            }
            string mySalt = BCrypt.GenerateSalt();
            string myHash = BCrypt.HashPassword(pwBoxPassword.Password, mySalt);
            if (BCrypt.CheckPassword(pwBoxConfirmPassword.Password, myHash))
            {
                myAccount.Password = myHash;
            }
            if (cBoxAccessLevel.SelectedIndex != -1)
            {
                myAccount.AccessLevel = cBoxAccessLevel.SelectedIndex;
            }
            dbContext.Users.InsertOnSubmit(myAccount);
            dbContext.SubmitChanges();

            //TODO: Figure out if Reach Out Lakota uses outlook or gmail
            try
            {
                MailMessage mail = new MailMessage();
                //Gmail
                SmtpClient gmailServer = new SmtpClient("smtp.gmail.com");
                //Outlook
                SmtpClient outlookServer = new SmtpClient("smtp-mail.outlook.com");
                mail.From = new MailAddress("ROLFoodBankInventoryManager@gmail.com");
                mail.To.Add(myAccount.Email);
                mail.Subject = "Account Created for " + myAccount.LastName + ", " + myAccount.LastName;
                mail.Body = "You have successfully created an account! Below is a summary of your account information: \n" +
                    "Name: " + String.Format("{0} {1}\n", myAccount.FirstName, myAccount.LastName) +
                    "Access Level: " + "Standard\n " + //Determine access level based on access level value
                    "Time of Account Creation: " + DateTime.Now;
                //Useful to know for later when sending exported spreadsheet for quarterly inventory reports
                //Attachment attachment = new Attachment("filename"); 

                //For outlook, this could also be 25
                gmailServer.Port = 587;
                outlookServer.Port = 587;

                //Account information for gmail account that emails will be sent from
                gmailServer.Credentials = new NetworkCredential("ROLFoodBankInventoryManager@gmail.com", "5LWP5MhOetfhFVlZv1bg");
                //Ensures that the client uses Secure Socket Layer (SSL) to encrypt the connection
                gmailServer.EnableSsl = true;

                gmailServer.Send(mail);
                MessageBox.Show("Mail successfully sent to " + myAccount.Email);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

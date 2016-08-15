using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Configuration;

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for CreateAccountWindow.xaml
    /// </summary>
    public partial class CreateAccountWindow : Window
    {
        private L2S_FoodBankDBDataContext dbContext;
        private User myAccount;
        private int comboBoxChoice;
        private bool emailIsInvalid;

        /*Data structure used to store whether a user has successfully filled out all the fields necessary to
        * create a user
        */
        private Dictionary<String, bool> nonEmptyFields;
        private bool readyToSubmit;

        /// <summary>
        /// Constructs a create account window object
        /// </summary>
        public CreateAccountWindow()
        {
            InitializeComponent();
            nonEmptyFields = new Dictionary<string, bool>();
            dbContext = new L2S_FoodBankDBDataContext(ConfigurationManager.ConnectionStrings["FoodBankInventoryManager.Properties.Settings.FoodBankDBConnectionString"].ConnectionString);
            myAccount = new User();
            emailIsInvalid = false;
            readyToSubmit = true;
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
            //Set first name and last name of user if fields are filled out
            if (Validate("First Name", txtFirstName.Text))
            {
                myAccount.FirstName = txtFirstName.Text; 
            }
            if (Validate("Last Name", txtLastName.Text))
            {
                myAccount.LastName = txtLastName.Text; 
            }
            /*
             * Makes sure that the text in both fields match and that the text in both
             * fields is in a valid format before setting the account's email as the
             * user's input
             */
            if (Validate("Email", txtEmail.Text)
                && Validate("Confirm Email", txtConfirmEmail.Text)
                && txtEmail.Text == txtConfirmEmail.Text 
                && isValidEmailFormat(txtEmail.Text) 
                && isValidEmailFormat(txtConfirmEmail.Text))
            {
                myAccount.Email = txtEmail.Text;
            }
            else
            {
                MessageBox.Show("Emails do not match.", "Inventory Manager Error System");
                return;
            }
            /*
             * Encrypts and salts a user's password using BCrypt. Might eventually want
             * to change to PBKDF2 since that's the only NIST-approved encryption algorithm
             * for C#
             */
            string mySalt = BCrypt.GenerateSalt();
            string myHash = "";
            if (Validate("Password", pwBoxPassword.Password))
            {
                myHash = BCrypt.HashPassword(pwBoxPassword.Password, mySalt); 
            }
            if (Validate("Confirm Password", pwBoxConfirmPassword.Password)
                && BCrypt.CheckPassword(pwBoxConfirmPassword.Password, myHash)) //CheckPassword() checks a plaintext string to a hashed password
            {
                myAccount.Password = myHash;
            }
            else
            {
                MessageBox.Show("Passwords do not match.", "Inventory Manager Error System");
                return;
            }
            //Sets user's access level if an option is chosen
            if (cBoxAccessLevel.SelectedIndex != -1)
            {
                if (nonEmptyFields.ContainsKey("Access Level"))
                {
                    nonEmptyFields.Remove("Access Level");
                }
                nonEmptyFields.Add("Access Level", true);
                myAccount.AccessLevel = cBoxAccessLevel.SelectedIndex;
            }
            else
            {
                nonEmptyFields.Add("Access Level", false);
            }
            //String that will be used in a messagebox to show the user what fields are not yet filled out
            string strEmptyReporter = "";
            foreach (KeyValuePair<String, bool> entry in nonEmptyFields)
            {
                if (!entry.Value)
                {
                    readyToSubmit = false;
                    strEmptyReporter += entry.Key + ", ";
                }
            }
            //If everything is filled out successfully, a user object is created
            if (readyToSubmit)
            {
                dbContext.Users.InsertOnSubmit(myAccount);
                dbContext.SubmitChanges(); 
            }
            else
            {
                strEmptyReporter = strEmptyReporter.Substring(0, strEmptyReporter.Length - 2);
                MessageBox.Show(this, "The following fields have not been filled out yet: " + strEmptyReporter);
            }

            /*TODO: Eventually use google's api for securing an oauth token in order 
             * to login into email in most updated secure way (only an "issue" with gmail, outlook
             * doesn't care)
             */ 
            if (readyToSubmit)
            {
                try
                {
                    MailMessage mail = new MailMessage();
                    //Gmail
                    SmtpClient gmailServer = new SmtpClient("smtp.gmail.com");
                    //Outlook
                    SmtpClient outlookServer = new SmtpClient("smtp-mail.outlook.com");
                    mail.From = new MailAddress("ROLFoodBankInventoryManager@gmail.com");
                    mail.To.Add(myAccount.Email);
                    mail.Subject = "Account Created for " + myAccount.LastName + ", " + myAccount.FirstName;
                    mail.Body = "You have successfully created an account! Below is a summary of your account information: \n" +
                        "Name: " + String.Format("{0} {1}\n", myAccount.FirstName, myAccount.LastName) +
                        "Access Level: " + cBoxAccessLevel.Items[myAccount.AccessLevel].ToString() + "\n" +//Determines access level based on access level value
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

                    //Sends an email to the user's email with their account creation details
                    gmailServer.Send(mail);
                    MessageBox.Show("Mail successfully sent to " + myAccount.Email);
                    Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                } 
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        /// <summary>
        /// Makes sure that the user inputs an email in the current format (xxx@xxx.xxx)
        /// </summary>
        /// <param name="anEmail"></param>
        /// <returns></returns>
        private bool isValidEmailFormat(string anEmail)
        {
            if (String.IsNullOrEmpty(anEmail))
            {
                return false;
            }
            try
            {
                anEmail = Regex.Replace(anEmail, @"(@)(.+)$", this.DomainMapper,
                                RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            if (emailIsInvalid)
            {
                return false;
            }
            try
            {
                return Regex.IsMatch(anEmail,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                emailIsInvalid = true;
            }
            return match.Groups[1].Value + domainName;
        }

        /// <summary>
        /// Makes sure that the field is correctly filled out
        /// </summary>
        /// <param name="fieldName">Field that is being checked</param>
        /// <param name="content">Content of the text field to be checked</param>
        /// <returns></returns>
        private bool Validate(string fieldName, string content)
        {
            if (nonEmptyFields.ContainsKey(fieldName))
            {
                nonEmptyFields.Remove(fieldName);
            }
            nonEmptyFields.Add(fieldName, !(String.IsNullOrWhiteSpace(content) || String.IsNullOrEmpty(content))); 
            return nonEmptyFields[fieldName];
        }
    }
}

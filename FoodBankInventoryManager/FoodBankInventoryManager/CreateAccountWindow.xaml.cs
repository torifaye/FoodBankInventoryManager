﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Globalization;

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
        private bool emailIsInvalid;

        public CreateAccountWindow()
        {
            InitializeComponent();
            emptyFields = new bool[7];
            dbContext = new L2S_FoodBankDBDataContext();
            myAccount = new User();
            emailIsInvalid = false;
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
            /*
             * Makes sure that the text in both fields match and that the text in both
             * fields is in a valid format before setting the account's email as the
             * user's input
             */
            if (txtEmail.Text == txtConfirmEmail.Text 
                && isValidEmailFormat(txtEmail.Text) 
                && isValidEmailFormat(txtConfirmEmail.Text))
            {
                myAccount.Email = txtEmail.Text;
            }
            /*
             * Encrypts and salts a user's password using BCrypt. Might eventually want
             * to change to PBKDF2 since that's the only NIST-approved encryption algorithm
             * for C#
             */
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
                    "Access Level: " + cBoxAccessLevel.Items[myAccount.AccessLevel].ToString() + //Determines access level based on access level value
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace FoodBankInventoryManager
{
    class Account
    {
        private static uint myUserId;
        private string myFirstName;
        private string myLastName;
        private SecureString myPassword;
        private string myEmail;
        private bool myAccessLevel;

        public Account()
        {
            myUserId++;
        }
        public static uint UserID
        {
            get { return myUserId; }
        }
        public string FirstName
        {
            get { return myFirstName; }
            set { myFirstName = value; }
        }
        public string LastName
        {
            get { return myLastName; }
            set { myLastName = value; }
        }
        public SecureString Password
        {
            get { return Password; }
            set { myPassword = value; }
        }
        public string Email
        {
            get { return myEmail; }
            set { myEmail = value; }
        }
        public bool IsAdmin
        {
            get { return myAccessLevel; }
            set { myAccessLevel = value; }
        }

    }
}

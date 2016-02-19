using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodBankInventoryManager
{
    class InventoryInfo
    {
        public string FoodCode
        {
            get; set;
        }
        public DateTime DateEntered
        {
            get; set;
        }
        public int Quantity
        {
            get; set;
        }
    }
}

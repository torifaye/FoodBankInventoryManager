using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodBankInventoryManager
{
    class InventoryInfo
    {
        public string FoodId
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
        public User User
        {
            get; set;
        }
        public int BinQuantity
        {
            get; set;
        }
    }
}

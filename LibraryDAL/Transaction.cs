using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryDAL
{
    public  class Transaction
    {
        public int TransactionID { get; set; } = -1; // default value
        public int BookId { get; set; }
        public int BorrowerId { get; set; }
        public DateTime Date { get; set; }
        public bool isBorrowed { get; set; }
    }
}

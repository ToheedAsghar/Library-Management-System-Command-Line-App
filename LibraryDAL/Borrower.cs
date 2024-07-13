using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryDAL
{
    public  class Borrower
    {
        private int _borrowerId = -1;
        public int BorrowerId
        {
            get { return _borrowerId; }
            set { _borrowerId = value; }
        }
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryDAL
{
    public  class Book
    {
        public int BookID { get; set; } = -1; // default value
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
    }
}

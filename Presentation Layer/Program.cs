#pragma warning disable CS8602, CS8604, CS8618

using LibraryDAL;
using System.ComponentModel.Design;
using System.Diagnostics;

public class PresentationLayerProgram
{
    // creating an instance of our Library
    internal static Library library = new Library();

    static void Main()
    {
        #region Main
        while (true)
        {
            Console.Clear();
            Console.WriteLine("\t\tLibrary Console Application");
            Console.WriteLine("_____________________________________________________\n");
            displayMenu();

            Console.Write("\nEnter Choice: ");
            var choice = Console.ReadLine().Trim();

            switch (choice)
            {
                case "1":
                    AddBook();
                    break;
                case "2":
                    removeBook();
                    break;
                case "3":
                    UpdateBook();
                    break;
                case "4":
                    RegisterBorrower();
                    break;
                case "5":
                    UpdateBorrower();
                    break;
                case "6":
                    DeleteABorrower();
                    break;
                case "7":
                    BorrowBook();
                    break;
                case "8":
                    returnBook();
                    break;
                case "9":
                    SearchBooks();
                    break;
                case "10":
                    ViewAllBooks();
                    break;
                case "11":
                    BooksByBorrower();
                    break;
                case "12":
                    Console.WriteLine("\nApplication Exited Successfully ....\n");
                    return;
                default:
                    {
                        Console.WriteLine("\nError: Invalid Input Entered!");
                        break;
                    }
            }
        }
        #endregion
    }

    #region Menu Functions
    public static void AddBook()
    {
        Console.Clear();
        Console.WriteLine("Enter Book Details:");
        Console.WriteLine("_____________________________________________________\n");

        string title = GetBookDetails("Title");
        string author = GetBookDetails("Author");
        string genre = GetBookDetails("Genre");

        Book book = new Book { Author = author, Genre = genre, Title = title };

        if (Library.AddBook(book)) { Console.WriteLine("\nBook Added Successfully!"); }
        else { Console.WriteLine("Error: Something Went Wrong!"); }

        Console.WriteLine("Press Enter to continue .... ");
        Console.Read();
    }
    public static void removeBook()
    {
        Console.Clear();
        Console.WriteLine("Removing Book");
        Console.WriteLine("_____________________________________________________\n");

        string input;
        Console.Write("Enter Book Id: \t");
        input = Console.ReadLine().Trim();

        if (ValidateInput(input, "Book"))
        {
            int id = int.Parse(input);
            if (library.RemoveBook(id))
            {
                Console.WriteLine("\nBook Removed Succesfully!");
            }
            else
            {
                Console.WriteLine("\nError: Check if the Book is Registerd or Book is not Borrowed!");
            }
        }

        Console.WriteLine("Press Enter to Continue ....");
        Console.Read();
    }
    public static void UpdateBook()
    {
        Console.Clear();
        Console.WriteLine("Updating Book");
        Console.WriteLine("_____________________________________________________\n");

        string input;
        Console.Write("Book ID: \t");
        input = Console.ReadLine().Trim();

        if (!int.TryParse(input, out _))
        {
            Console.WriteLine("\nError: Invalid Book Id!");
        }
        else if (library.isBookBorrowed(int.Parse(input)))
        {
            Console.WriteLine("\nError: Borrowed Book can't be Modified!");
        }
        else
        {
            Console.WriteLine("\nEnter New Details of the Book: ");
            string title = GetBookDetails("Title");
            string author = GetBookDetails("Author");
            string genre = GetBookDetails("Genre");

            var id = int.Parse(input);
            if (library.UpdateBook(new Book { Title = title, Author = author, Genre = genre, BookID = id }))
            {
                Console.WriteLine("\nBook Updated Succesfully!");
            }
            else
            {
                Console.WriteLine("\nError: Check if the Book is Registerd or the Book is not Issued!");
            }
        }
        Console.WriteLine("Press Enter to Continue ....");
        Console.Read();
    }
    public static void RegisterBorrower()
    {
        Console.Clear();
        Console.WriteLine("Enter Borrower Details:");
        Console.WriteLine("_____________________________________________________\n");

        string name = GetBorrowerName();
        string email = GetBorrowerEmail("");

        if (email != null)
        {
            if (library.RegisterBorrower(new Borrower { Name = name, Email = email }))
            {
                Console.WriteLine("\nBorrower Resgistered Successfully!");
            }
            else
            {
                Console.WriteLine("\nError: Email Already Registered!");
            }
        }

        Console.WriteLine("Press Enter to Continue ....");
        Console.Read();
    }
    public static void DeleteABorrower()
    {
        Console.Clear();
        Console.WriteLine("Enter Details of Borrower to be Deleted: ");
        Console.WriteLine("_____________________________________________________\n");

        Console.Write("Enter Id: \t");
        string input = Console.ReadLine().Trim();
        if (ValidateInput(input, "Borrower"))
        {
            int id = int.Parse(input);
            if (library.DeleteBorrower(id))
            {
                Console.WriteLine("\nBorrower Deleted Successfully!");
            }
            else
            {
                Console.WriteLine("\nError: Something went wrong while Deleting Borrower!");
                Console.WriteLine("Make sure the borrower is Registered or hasn't borrowed any Books");
            }
        }
        Console.WriteLine("Press Enter to Continue ....");
        Console.Read();
    }
    public static void BorrowBook()
    {
        Console.Clear();
        Console.WriteLine("Borrow A Book: ");
        Console.WriteLine("_____________________________________________________\n");

        Console.Write("{0, -30}", "Enter Borrower Id:");
        string input = Console.ReadLine().Trim();
        if (ValidateInput(input, "Borrower"))
        {
            int BorrowerId = int.Parse(input);
            Console.Write("{0, -30}", "Enter Book Id:");
            input = Console.ReadLine().Trim();
            if (ValidateInput(input, "Book"))
            {
                Transaction transaction = new Transaction
                {
                    BookId = int.Parse(input),
                    BorrowerId = BorrowerId,
                    Date = DateTime.Now,
                    isBorrowed = true
                };
                library.RecordTransaction(transaction);
            }
        }

        Console.WriteLine("Press Enter to Continue ....");
        Console.Read();
    }
    public static void ViewAllBooks()
    {
        Console.Clear();
        Console.WriteLine("\t\t\t\t\t Library");
        Console.WriteLine("----------------------------------------------------------------------------------------------------");

        List<Book> books = library.getAllBooks();
        if (books.Count == 0)
        {
            Console.WriteLine("No Books in Library!");
        }
        else
        {
            Console.WriteLine("{0,-10} {1,-30} {2,-30} {3,-15} {4,-10}", "BookId", "Title", "Author", "Genre", "Status");
            Console.WriteLine("----------------------------------------------------------------------------------------------------");
            foreach (var book in books)
            {
                string available = book.IsAvailable ? "Available" : "Issued";
                Console.WriteLine("{0,-10} {1,-30} {2,-30} {3,-15} {4,-10}", book.BookID, book.Title, book.Author, book.Genre, available);
            }
        }

        Console.WriteLine("\nPress Enter to Continue ....");
        Console.ReadLine();
    }
    public static void BooksByBorrower()
    {
        Console.Clear();
        Console.WriteLine("Get Books by A Borrower: ");
        Console.WriteLine("_____________________________________________________\n");

        Console.Write("{0, -30}", "Enter Borrower Id:");
        string input = Console.ReadLine().Trim();

        if (ValidateInput(input, "Borrower"))
        {
            int id = Convert.ToInt32(input);
            List<Transaction> transactions = library.GetBorrowedBooksByBorrower(id);
            if (transactions != null && transactions.Count == 0)
            {
                Console.WriteLine("\nNo Books borrowed by The Current Borrower!");
            }
            else if(transactions != null)
            {
                Console.WriteLine("\n{0, -10} {1, -30} {2, -20}", "Book ID", "Book Name", "Date Borrowed");
                Console.WriteLine("---------------------------------------------------------------------------------------------");
                foreach(var transaction in transactions)
                {
                    Book book = library.GetBookById(transaction.BookId);
                    Console.WriteLine("{0, -10} {1, -30} {2, -20}", transaction.BookId, book.Title, transaction.Date);
                }
            }
            Console.WriteLine("\nPress Enter to Continue ....");
            Console.Read();
        }
    }
    public static void returnBook()
    {
        Console.Clear();
        Console.WriteLine("Return A Book: ");
        Console.WriteLine("_____________________________________________________\n");

        Console.Write("{0, -30}", "Enter Book Id:");
        string input = Console.ReadLine().Trim();
        if (ValidateInput(input, "Book"))
        {
            int BookId = int.Parse(input);
            Console.Write("{0, -30}", "Enter Borrower Id:");
            input = Console.ReadLine().Trim();

            if (ValidateInput(input, "Borrower"))
            {
                int BorrowerId = int.Parse(input);
                Transaction transaction = new Transaction
                {
                    BookId = BookId,
                    BorrowerId = BorrowerId,
                    Date = DateTime.Now,
                    isBorrowed = false
                };
                library.RecordTransaction(transaction);
            }
        }

        Console.WriteLine("\nPress Enter to Continue ....");
        Console.Read();
    }
    public static void UpdateBorrower()
    {
        // if the borrower has borrowed any books then the details of the borrower
        // can't be updated
        Console.Clear();
        Console.WriteLine("Update A Borrower: ");
        Console.WriteLine("_____________________________________________________\n");

        Console.Write("{0, -20}", "Enter Borrower Id: ");
        string input = Console.ReadLine().Trim();

        if (ValidateInput(input, "Borrower"))
        {
            int id = int.Parse(input);

            string currEmail = library.BorrowerEmail(id);
            if (currEmail == null)
            {
                Console.WriteLine("\nError: Borrower isn't Registered to Library!");
            }
            else
            {
                string name = GetBorrowerName();
                string email = GetBorrowerEmail(currEmail);
                if (email != null)
                {
                    Borrower newBorrower = new Borrower
                    {
                        BorrowerId = id,
                        Name = name,
                        Email = email
                    };

                    if (library.UpdateBorrower(newBorrower))
                    {
                        Console.WriteLine("\nBorrower Updated Successfully!");
                    }
                }
            }
        }

        Console.WriteLine("Press Enter to Continue ....");
        Console.Read();
    }
    public static void SearchBooks()
    {
        Console.Clear();
        Console.WriteLine("Search by Title, Author OR Genre");
        Console.WriteLine("_____________________________________________________\n");

        Console.Write("Query: ");
        var query = Console.ReadLine().Trim();

        List<Book> books = library.SearchBooks(query);
        if (books.Count == 0)
        {
            Console.WriteLine("\nNo Books Found!");
        }
        else
        {
            Console.WriteLine("\n{0,-10} {1,-30} {2,-30} {3,-15} {4,-10}", "BookId", "Title", "Author", "Genre", "Status");
            Console.WriteLine("-----------------------------------------------------------------------------------------------");
            foreach (var book in books)
            {
                string available = book.IsAvailable ? "Available" : "Issued";
                Console.WriteLine("{0,-10} {1,-30} {2,-30} {3,-15} {4,-10}", book.BookID, book.Title, book.Author, book.Genre, available);
            }
        }

        Console.WriteLine("\nPress Any Key to Continue ....");
        Console.Read();
    }
    #endregion

    #region Helper Functions
    public static void displayMenu()
    {
        Console.WriteLine("1.  Add a new book\n2.  Remove a book\n3.  Update a book\n4.  Register a new borrower\n5.  Update a borrower\r\n6.  Delete a borrower\n7.  Borrow a book\n8.  Return a book\n9.  Search for books by title, author, or genre\r\n10. View all books\n11. View borrowed books by a specific borrower\n12. Exit the application");
    }
    public static string GetBorrowerName()
    {
        string name = "";
        do
        {
            Console.Write("{0, -20}", "Borrower Name:");
            name = Console.ReadLine().Trim();
            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("Error: Name Can't be Empty!");
            }

        } while (string.IsNullOrEmpty(name));
        return name;
    }
    public static string GetBookDetails(string type)
    {
        string input = "";
        do
        {
            Console.Write("{0, -20}", $"{type} of Book:");
            input = Console.ReadLine().Trim();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine($"Error: {type} of the Book Can't be Empty!");
            }
        } while (string.IsNullOrWhiteSpace(input));
        return input;
    }
    public static string GetBorrowerEmail(string exclude)
    {
        string email;
        Console.Write("{0, -20}", "Enter Email:");
        email = Console.ReadLine().Trim().ToLower();
        if (ValidateEmail(email, exclude))
        {
            return email;
        }
        else
        {
            return null;
        }
    }
    public static bool ValidateFormat(string email)
    {
        HashSet<char> disallowedCharacters = new HashSet<char>
        {
            ' ', ',', ':', ';', '<', '>', '(', ')', '[', ']', '\\', '/', '"', '~'
        };
        bool retVal = true;
        int alphas = 0;
        foreach (var ch in email)
        {
            if (ch == '@')
            {
                alphas++;
            }
            else if (disallowedCharacters.Contains(ch))
            {
                retVal = false;
                break;
            }
        }
        if (alphas != 1)
        {
            retVal = false;
        }
        else
        {
            int i = email.IndexOf('@');
            string domain = email.Substring(i + 1);
            string local = email.Substring(0, i);

            var tokens = domain.Split('.');
            if (string.IsNullOrWhiteSpace(local) || string.IsNullOrWhiteSpace(local))
            {
                retVal = false;
            }
            else if (tokens.Count() > 3)
            {
                retVal = false;
            }
            else if (local.Contains(".."))
            {
                retVal = false;
            }
            else if (local.StartsWith(".") || local.EndsWith("."))
            {
                retVal = false;
            }

            if (retVal)
            {
                foreach (var token in tokens)
                {
                    if (token.Count() < 2)
                    {
                        retVal = false;
                        break;
                    }
                }
            }
        }
        return retVal;
    }
    public static bool ValidateEmail(string email, string exclude)
    {
        bool retVal = true;
        if (string.IsNullOrEmpty(email))
        {
            Console.WriteLine("\nError: Email can't be Empty!");
            retVal = false;
        }
        else if (!ValidateFormat(email)) // if format is incorrect
        {
            Console.WriteLine("\nError: Incorrect Format of Email");
            retVal = false;
        }
        else if (library.DoesEmailExistsinDB(email) && email != exclude)    // if email is already registerd
        {
            Console.WriteLine("\nError: Email Already Registered!");
            retVal = false;
        }
        return retVal;
    }
    public static bool ValidateInput(string input, string type)
    {
        bool retVal = true;
        if (string.IsNullOrEmpty(input))
        {
            Console.WriteLine($"\nError: {type} Id can't be Empty!");
            retVal = false;
        }
        else if (!int.TryParse(input, out _))
        {
            Console.WriteLine($"\nError: Invalid {type} Id Given!");
            retVal = false;
        }
        return retVal;
    }
    #endregion
}
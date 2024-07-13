#pragma warning disable CSO120, CS8603

using Microsoft.Data.SqlClient;

using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace LibraryDAL
{
    public class Library
    {
        internal static string connectionString = "Data Source=(localdb)\\ProjectModels;Initial Catalog=LibraryDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

        #region Library Main Methods
        public static bool AddBook(Book book)
        {
            string title = book.Title;
            string author = book.Author;
            string genre = book.Genre;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "insert into books (title, author, genre)" +
                    "values (@title, @author, @genre)";
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@author", author);
                    cmd.Parameters.AddWithValue("@genre", genre);

                    var AffectedRows = cmd.ExecuteNonQuery();

                    return AffectedRows > 0;
                }

            }
        }
        public bool RemoveBook(int bookId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                //Console.WriteLine("Debug: " + bookId);

                string query = @"delete from books
                                where bookid = @bookid and isavailable = 1 ; ";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.Add("@bookid", SqlDbType.Int).Value = bookId;
                    var AffectedRows = cmd.ExecuteNonQuery();
                    return AffectedRows > 0;
                }
            }
        }
        public List<Book> getAllBooks()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "select * from books";
                using (var cmd = new SqlCommand(query, connection))
                {
                    List<Book> books = new List<Book>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Book book = new Book
                            {
                                BookID = Convert.ToInt32(reader["bookid"]),
                                Title = reader["title"].ToString(),
                                Author = Convert.ToString(reader["author"]),
                                Genre = Convert.ToString(reader["genre"]),
                                IsAvailable = Convert.ToBoolean(reader["isavailable"])
                            };
                            books.Add(book);
                        }
                        return books;
                    }
                }
            }
        }
        public Book GetBookById(int bookId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "select * from books where bookid = @bookid";
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@bookid", bookId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Book
                            {
                                BookID = (int)reader["bookid"],
                                Title = reader["title"].ToString(),
                                Author = reader["author"].ToString(),
                                Genre = reader["genre"].ToString(),
                                IsAvailable = Convert.ToBoolean(reader["isavailable"])
                            };
                        }
                        else
                        {
                            return null;
                        }
                    }
                }

            }
        }
        public bool UpdateBook(Book book)
        {
            string title = book.Title;
            string author = book.Author;
            string genre = book.Genre;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE books " +
                    "SET title = @title, genre = @genre, author = @author " +
                    "WHERE bookid = @bookid and isavailable = 1 ";

                using (var cmd = new SqlCommand(query, connection))
                {

                    cmd.Parameters.AddWithValue("@bookid", book.BookID);
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@author", author);
                    cmd.Parameters.AddWithValue("@genre", genre);

                    var AffectedRows = cmd.ExecuteNonQuery();
                    return AffectedRows > 0;
                }
            }
        }
        public List<Book> SearchBooks(string query)
        {
            query = "%" + query.ToLower().Trim() + "%";
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = @"select * from books 
                                    where lower(title) like @query or lower(author) like @query or lower(genre) like @query;";

                using (var cmd = new SqlCommand(sqlQuery, connection))
                {
                    List<Book> books = new List<Book>();
                    cmd.Parameters.AddWithValue("@query", query);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            books.Add(new Book
                            {
                                BookID = Convert.ToInt32(reader["bookid"]),
                                Title = reader["title"].ToString(),
                                Author = Convert.ToString(reader["author"]),
                                Genre = Convert.ToString(reader["genre"]),
                                IsAvailable = Convert.ToBoolean(reader["isavailable"])
                            });
                        }
                        return books;
                    }
                }
            }
        }
        public bool RegisterBorrower(Borrower borrower)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string Query = "insert into borrowers (name, email) values (@name, @email)";
                using (var cmd = new SqlCommand(Query, connection))
                {
                    cmd.Parameters.AddWithValue("@name", borrower.Name);
                    cmd.Parameters.AddWithValue("@email", borrower.Email);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
        public bool DeleteBorrower(int borrowerId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string Query = @"delete from borrowers 
                                 where borrowerid = @borrowerid 
                                    and @borrowerid not in (select borrowerid from transactions where isborrowed = 1)";

                using (var cmd = new SqlCommand(Query, connection))
                {
                    cmd.Parameters.AddWithValue("@borrowerid", borrowerId);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
        public bool UpdateBorrower(Borrower borrower)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // checking if the borrower has borrowed any books, then it can't be updated
                List<Transaction> transactions = GetBorrowedBooksByBorrower(borrower.BorrowerId);

                if (transactions == null || 0 == transactions.Count)
                {
                    string query = "UPDATE borrowers " +
                    "SET name = @name, email = @email " +
                    "WHERE borrowerid = @borrowerid ";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@borrowerid", borrower.BorrowerId);
                        cmd.Parameters.AddWithValue("@name", borrower.Name);
                        cmd.Parameters.AddWithValue("@email", borrower.Email);

                        int AffectedRows = cmd.ExecuteNonQuery();

                        return AffectedRows > 0;
                    }
                }
                else
                {
                    Console.WriteLine("Error: Borrower has pending Returns, It can't be updated!");
                    return false;
                }
            }
        }
        public void RecordTransaction(Transaction transaction)
        {
            if (!ValidBorrower(transaction.BorrowerId))      // checking the authenticity of borrower
            {
                Console.WriteLine("\nError: Borrower Not Registerd!");
            }
            else if (transaction.isBorrowed == false)        // else the transaction is to borrow book
            {
                ReturnBook(transaction);
            }
            else if (isBookBorrowed(transaction.BookId))
            {
                Console.WriteLine("\nError: Book Already Borrowed!");
            }
            else
            {
                // .. Transaction to Borrow Book
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @" insert into transactions(bookid, borrowerid, isborrowed) 
                                      values (@bookid, @borrowerid, @availability); ";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@bookid", transaction.BookId);
                        cmd.Parameters.AddWithValue("@borrowerid", transaction.BorrowerId);
                        cmd.Parameters.AddWithValue("@availability", transaction.isBorrowed);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        // .. If no rows are affected then transaction can't be processed due to SOME reason, 
                        // .. So returned false
                        if (rowsAffected == 0)
                        {
                            Console.WriteLine("\nSomething Went Wrong on Our End!");
                        }
                        // .. If transaction processed successfully, then udpate the book status to unavailable
                        else
                        {
                            Console.WriteLine("\nBook Borrowed Successfully!");
                            UpdateBookStatus(transaction.BookId, false);
                        }
                    }
                }
            }
        }
        public List<Transaction> GetBorrowedBooksByBorrower(int borrowerId)
        {
            if (!ValidBorrower(borrowerId))
            {
                Console.WriteLine("\nError: Borrower Not Registered!");
                return null;
            }
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "select * from transactions where borrowerid = @borrowerid and isborrowed = 1";
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@borrowerid", borrowerId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        List<Transaction> transactions = new List<Transaction>();
                        while (reader.Read())
                        {
                            transactions.Add(new Transaction
                            {
                                TransactionID = (int)reader["transactionid"],
                                BookId = (int)reader["bookid"],
                                BorrowerId = (int)reader["borrowerid"],
                                Date = Convert.ToDateTime(reader["date"]),
                                isBorrowed = true
                            });
                        }
                        return transactions;
                    }
                }
            }
        }
        #endregion

        #region Helper Functions
        public bool ValidBorrower(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "select 1 from borrowers where borrowerid = @borrowerid";
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@borrowerid", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read();
                    }
                }
            }
        }
        public void UpdateBookStatus(int id, bool availabe)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "update books set isavailable = @availability where bookid = @id";
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    if (availabe) { cmd.Parameters.AddWithValue("@availability", 1); }
                    else { cmd.Parameters.AddWithValue("@availability", 0); }

                    cmd.ExecuteNonQuery();
                }
            }
        }
        public bool isBookBorrowed(int BookId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = " select isavailable from books where bookid = @bookid; ";
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@bookid", BookId);
                    bool retVal = false;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader["isavailable"].ToString() == "False") { retVal = true; }
                        }
                        else
                        {
                            Console.WriteLine("Error: Book not Registered!");
                            retVal = false;
                        }
                        return retVal;
                    }
                }
            }
        }
        public string BorrowerEmail(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "select email from borrowers where borrowerid = @id";
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader[0].ToString();
                        }
                        else { return null; }
                    }
                }
            }
        }

        // function to return Book
        public void ReturnBook(Transaction transaction)
        {
            int transactionid = -1;
            Book book = GetBookById(transaction.BookId);
            if (book == null)               // Validate: if book exists in library
            {
                Console.WriteLine("\nError: Book doesn't exists in Library!");
            }
            else if (true == book.IsAvailable)              // validate: if book is actually borrowed
            {
                Console.WriteLine("\nError: Book not Borrowed!");
            }
            else if (-1 == (transactionid = GetTransaction(transaction)))   // validate: if books is borrowed by the borrower
            {
                Console.WriteLine("\nError: Borrower nevered Borrowed this Book!");
            }
            else
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"update transactions set isborrowed = 0
                                    where transactionid = @transactionid; ";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@transactionid", transactionid);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            // if transaction is successfully added to the database
                            // then update the database and set the availability of the book to true
                            UpdateBookStatus(transaction.BookId, true);
                            Console.WriteLine("\nBook Returned Successfully!");
                        }
                        else
                        {
                            // .. In case the transaction can't be processed due to ANY reason,
                            // .. return false
                            Console.WriteLine("\nSomething Went Wrong on Our End, Please try again!");
                        }
                    }
                }
            }
        }

        // validating return in order to check if the book was borrowed by this borrower or not
        public int GetTransaction(Transaction transaction)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"select transactionid from transactions
                                 where bookid = @bookid and borrowerid = @borrowerid and isborrowed = 1; ";
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@bookid", transaction.BookId);
                    cmd.Parameters.AddWithValue("@borrowerid", transaction.BorrowerId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return (int)reader[0];
                        }
                        else { return -1; }
                    }
                }
            }
        }
        // function to check if email already exists in database
        public bool DoesEmailExistsinDB(string email)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "select 1 from borrowers where email = @email";
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read();
                    }
                }
            }
        }
        #endregion
    }
}
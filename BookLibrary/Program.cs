using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BookLibrary
{
    public interface IInstitution : IDisposable
    {
        int BookId { get; }
        bool IsAccurate();
    }

    public interface IRepository<T> : IDisposable, IEnumerable<T> where T : IInstitution
    {
        IEnumerable<T> Data { get; }
        void Attach(T entity);
        bool Remove(T entity);
        void Modernize(T entity);
        T FindByBookId(int Id);
        IEnumerable<T> Explore(string value);
    }

    public sealed class Book : IInstitution
    {
        public int BookId { get; }
        public string Title { get; set; }
        public string Author { get; set; }
        public Genre Genre { get; set; }
        public int YearPublished { get; set; }

        public Book()
        {
        }

        public Book(int bookId)
        {
            this.BookId = bookId;
        }

        public Book(int bookId, string title, string author, Genre genre, int yearPublished)
        {
            this.BookId = bookId;
            this.Title = title;
            this.Author = author;
            this.Genre = genre;
            this.YearPublished = yearPublished;
        }

        public bool IsAccurate()
        {
            return !string.IsNullOrWhiteSpace(Title) && !string.IsNullOrWhiteSpace(Author) && YearPublished > 0;
        }

        public override string ToString()
        {
            string text = "Book Info\n";
            text = text + $"Book ID: {this.BookId}\n";
            text += $"Title: {this.Title}\n";
            text += $"Author: {this.Author}\n";
            text += $"Genre: {this.Genre}\n";
            text += $"Year Published: {this.YearPublished}\n";
            text += "************************\n";

            return text;
        }

        public void Dispose()
        {
        }
    }

    public sealed class BookRepository : IRepository<Book>
    {
        private static BookRepository _instance;
        public static BookRepository Instance
        {
            get
            {
                _instance = _instance ?? new BookRepository();
                return _instance;
            }
        }

        List<Book> Collection;

        private BookRepository()
        {
            Collection = new List<Book>
            {
                new Book(1, "The Great Gatsby", "F. Scott Fitzgerald", Genre.Fiction, 1925),
                new Book(2, "To Kill a Mockingbird", "Harper Lee", Genre.Fiction, 1960),
                new Book(3, "1984", "George Orwell", Genre.SciFi, 1949),
                new Book(4, "Pride and Prejudice", "Jane Austen", Genre.Romance, 1813),
                new Book(5, "The Hobbit", "J.R.R. Tolkien", Genre.Fantasy, 1937),
                new Book(6, "Steve Jobs", "Walter Isaacson", Genre.Biography, 2011)
            };
        }

        public void Dispose()
        {
            Collection.Clear();
        }

        IEnumerable<Book> IRepository<Book>.Data { get => Collection; }

        public Book this[int index]
        {
            get
            {
                return Collection[index];
            }
        }

        public void Attach(Book entity)
        {
            if (Collection.Any(b => b.BookId == entity.BookId))
            {
                throw new Exception("Duplicate book ID, try another");
            }
            else if (entity.IsAccurate())
            {
                Collection.Add(entity);
            }
            else
            {
                throw new Exception("Book is invalid");
            }
        }

        public bool Remove(Book entity)
        {
            return Collection.Remove(entity);
        }

        public void Modernize(Book entity)
        {
            var existingBook = Collection.FirstOrDefault(b => b.BookId == entity.BookId);
            if (existingBook != null)
            {
                existingBook.Title = entity.Title;
                existingBook.Author = entity.Author;
                existingBook.Genre = entity.Genre;
                existingBook.YearPublished = entity.YearPublished;
            }
            else
            {
                throw new Exception("Book not found");
            }
        }

        public Book FindByBookId(int Id)
        {
            var result = Collection.FirstOrDefault(b => b.BookId == Id);
            return result;
        }

        public IEnumerable<Book> Explore(string value)
        {
            var result = from b in Collection.AsParallel()
                         where
                         b.BookId.ToString().Contains(value) ||
                         b.Title.Contains(value) ||
                         b.Author.Contains(value) ||
                         b.Genre.ToString().Contains(value) ||
                         b.YearPublished.ToString().Contains(value)
                         orderby b.Title ascending
                         select b;
            return result;
        }

        public IEnumerator<Book> GetEnumerator()
        {
            foreach (var b in Collection)
            {
                yield return b;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var b in Collection)
            {
                yield return b;
            }
        }
    }

    public enum Genre
    {
        Fiction,
        NonFiction,
        Mystery,
        SciFi,
        Romance,
        Fantasy,
        Biography,
        SelfHelp,
        History,
        Poetry,
        Other
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (IRepository<Book> books = BookRepository.Instance)
                {
                    #region Add book

                    books.Attach(new Book(7, "The Catcher in the Rye", "J.D. Salinger", Genre.Fiction, 1951));

                    #endregion

                    var b2 = books.FindByBookId(2);
                    b2.Title = "Updated Title";
                    books.Modernize(b2);

                    Console.WriteLine($"Book {b2.BookId} updated successfully");
                    Console.WriteLine(b2.ToString());

                    if (books.Remove(b2))
                        Console.WriteLine($"Book {b2.BookId} deleted successfully");

                    #region Search from repository

                    var data = books.Explore("George");
                    Console.WriteLine();
                    Console.WriteLine($"Total Books: {data.Count()}");
                    Console.WriteLine("----------------------------------");

                    foreach (var b in data)
                    {
                        Console.WriteLine(b.ToString());
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}

using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;

static class DbHelper
{
    static string DbName = "Library";
    static private MongoClient client = new MongoClient("mongodb://localhost:27017");

    static public List<Book> GetAllBooks()
    {
        var db = client.GetDatabase(DbName);
        var booksCollection = db.GetCollection<Book>("Books");
        var filter = Builders<Book>.Filter.Empty;
        var books = booksCollection.Find(filter).ToList();
        return books;
    }

    static public List<Reader> GetAllReaders()
    {
        var db = client.GetDatabase(DbName);
        var readersCollection = db.GetCollection<Reader>("Readers");
        var filter = Builders<Reader>.Filter.Empty;
        var readers = readersCollection.Find(filter).ToList();
        return readers;
    }

    static public Book GetBookFromDB(string _id)
    {
        var db = client.GetDatabase(DbName).GetCollection<BsonDocument>("Books");
        var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(_id));
        var document = db.Find(filter).First();
        Console.WriteLine(document);
        return BsonSerializer.Deserialize<Book>(document);
    }
    static public Reader GetReaderFromDB(string _id)
    {
        var db = client.GetDatabase(DbName).GetCollection<BsonDocument>("Readers");
        var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(_id));
        var document = db.Find(filter).First();
        Console.WriteLine(document);
        return BsonSerializer.Deserialize<Reader>(document);
    }

    static public void AddToDB(Reader _reader)
    {
        var db = client.GetDatabase(DbName);
        var booksCollection = db.GetCollection<Reader>("Readers");
        booksCollection.InsertOne(_reader);
    }
    static public void RemoveReaderFromDB(string _readerId)
    {
        var db = client.GetDatabase(DbName).GetCollection<BsonDocument>("Readers");
        var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(_readerId));
        var document = db.Find(filter).First();
        db.DeleteOne(document);
    }
    static public void UpdateReaderFromDB(string _readerId, string _newName, string _newLastname)
    {
        var db = client.GetDatabase(DbName);
        var readersCollection = db.GetCollection<Reader>("Readers");
        var filter = Builders<Reader>.Filter.Eq("_id", ObjectId.Parse(_readerId));
        var update = Builders<Reader>.Update
            .Set("FirstName", _newName)
            .Set("LastName", _newLastname);

        readersCollection.UpdateOne(filter, update);
    }
    public static void AddBorrowBookToDB(string readerId, Book newBorrowedBook)
    {
        var db = client.GetDatabase(DbName);
        var readersCollection = db.GetCollection<Reader>("Readers");
        var filter = Builders<Reader>.Filter.Eq(r => r.Id, ObjectId.Parse(readerId));
        var update = Builders<Reader>.Update.Push(r => r.BorrowedBooks, newBorrowedBook);
        readersCollection.UpdateOne(filter, update);
    }
    public static void RemoveBorrowBookFromDB(Reader _reader, Book _bookToDelete)
    {
        var db = client.GetDatabase(DbName);
        var readersCollection = db.GetCollection<Reader>("Readers");
        var filter = Builders<Reader>.Filter.Eq(r => r.Id, ObjectId.Parse(_reader.Id.ToString()));
        var update = Builders<Reader>.Update.Set(r => r.BorrowedBooks, _reader.BorrowedBooks);
        readersCollection.UpdateOne(filter, update);
    }
    static public void AddToDB(Book _book)
    {
        var db = client.GetDatabase(DbName);
        var booksCollection = db.GetCollection<Book>("Books");
        booksCollection.InsertOne(_book);
    }
    static public void RemoveBookFromDB(string _bookName)
    {
        var db = client.GetDatabase(DbName).GetCollection<BsonDocument>("Books");
        var filter = Builders<BsonDocument>.Filter.Eq("Title", _bookName);
        var document = db.Find(filter).First();
        db.DeleteOne(document);
    }
    static public void UpdateBookFromDB(string _bookName, string _newTitle, string _newAuthor)
    {
        var db = client.GetDatabase(DbName);
        var booksCollection = db.GetCollection<Book>("Books");
        var filter = Builders<Book>.Filter.Eq("Title", _bookName);
        var update = Builders<Book>.Update
            .Set("Title", _newTitle)
            .Set("Author", _newAuthor);

        booksCollection.UpdateOne(filter, update);
    }
}

public class Reader
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<Book> BorrowedBooks { get; set; } = new List<Book>();
}

static class Library
{
    public static void AddBook(Book book)
    {
        DbHelper.AddToDB(book);
        Console.WriteLine("Książka została dodana do biblioteki");
    }

    public static void AddReader(Reader reader)
    {
        DbHelper.AddToDB(reader);
        Console.WriteLine("Czytelnik został dodany do biblioteki");
    }

    public static void BorrowBook(Reader reader, Book book)
    {
        DbHelper.AddBorrowBookToDB(reader.Id.ToString(), book);
        Console.WriteLine($"Książka '{book.Title}' została wypożyczona przez {reader.FirstName} {reader.LastName}");
    }
    public static void ReturnBook(Reader reader, Book book)
    {
        if (reader != null && book != null) {
            reader.BorrowedBooks.Remove(book);
            DbHelper.RemoveBorrowBookFromDB(reader, book);
            Console.WriteLine($"Książka '{book.Title}' została zwrócona przez czytelnika {reader.FirstName} {reader.LastName}.");
        }
        else {
            Console.WriteLine("Nie znaleziono czytelnika lub książki o podanych danych.");
        }
    }

    public static void RemoveBook(string title)
    {
        List<Book> tempBooks = DbHelper.GetAllBooks();
        Book bookToRemove = tempBooks.FirstOrDefault(b => b.Title == title);
        if (bookToRemove != null)
        {
            DbHelper.RemoveBookFromDB(bookToRemove.Title);
            Console.WriteLine($"Książka '{title}' została usunięta z biblioteki");
        }
        else
        {
            Console.WriteLine($"Nie znaleziono książki o tytule '{title}'");
        }
    }

    public static void RemoveReader(string readerID)
    {
        var tempReaders = DbHelper.GetAllReaders();
        Reader readerToRemove = tempReaders.FirstOrDefault(r => r.Id.ToString() == readerID);
        if (readerToRemove != null)
        {
            DbHelper.RemoveReaderFromDB(readerToRemove.Id.ToString());
            Console.WriteLine($"Czytelnik o ID {readerID} został usunięty z biblioteki");
        }
        else
        {
            Console.WriteLine($"Nie znaleziono czytelnika o ID {readerID}");
        }
    }

    public static void UpdateBook(string title, string newTitle, string newAuthor)
    {
        List<Book> tempBooks = DbHelper.GetAllBooks();
        Book bookToUpdate = tempBooks.FirstOrDefault(b => b.Title == title);
        if (bookToUpdate != null)
        {
            DbHelper.UpdateBookFromDB(title, newTitle, newAuthor);
            Console.WriteLine($"Dane książki '{title}' zostały zaktualizowane");
        }
        else
        {
            Console.WriteLine($"Nie znaleziono książki o tytule '{title}'");
        }
    }

    public static void DisplayBorrowedBooks(Reader reader)
    {
        if (reader.BorrowedBooks.Count == 0)
        {
            Console.WriteLine($"{reader.FirstName} {reader.LastName} nie wypożyczył jeszcze żadnych książek");
        }
        else
        {
            Console.WriteLine($"Lista wypożyczonych książek przez {reader.FirstName} {reader.LastName}:");
            foreach (var book in reader.BorrowedBooks)
            {
                Console.WriteLine($"{book.Title} - {book.Author}");
            }
        }
    }

    public static void UpdateReader(string readerID, string newFirstName, string newLastName)
    {
        var tempReaders = DbHelper.GetAllReaders();
        Reader readerToUpdate = tempReaders.FirstOrDefault(r => r.Id.ToString() == readerID);
        if (readerToUpdate != null)
        {
            DbHelper.UpdateReaderFromDB(readerID, newFirstName, newLastName);
            Console.WriteLine($"Dane czytelnika o ID {readerID} zostały zaktualizowane");
        }
        else
        {
            Console.WriteLine($"Nie znaleziono czytelnika o ID {readerID}");
        }
    }

    public static void DisplayReaders()
    {
        var tempReaders = DbHelper.GetAllReaders();
        if (tempReaders.Count == 0)
        {
            Console.WriteLine("Brak zarejestrowanych czytelników");
        }
        else
        {
            Console.WriteLine("Lista czytelników w bibliotece:");
            foreach (var reader in tempReaders)
            {
                Console.WriteLine($"{reader.FirstName} {reader.LastName} (ID: {reader.Id.ToString()})");
            }
        }
    }

    public static void DisplayBooks()
    {
        List<Book> tempBooks = DbHelper.GetAllBooks();
        if (tempBooks.Count == 0)
        {
            Console.WriteLine("Nie ma książek");
        }
        else
        {
            Console.WriteLine("Lista książek w bibliotece:");
            foreach (var book in tempBooks)
            {
                Console.WriteLine($"{book.Title} - {book.Author} ( ID: {book.Id})");
            }
        }
    }

    public static void DisplayBook(Book _book)
    {
        if (_book == null)
        {
            Console.WriteLine("ID nie poprawny");
        }
        else
        {
            Console.WriteLine($"{_book.Title} - {_book.Author} ( ID: {_book.Id})");
        }
    }

    public static void DisplayReaderByBorrowedBook(string bookTitle)
    {
        List<Reader> readers = DbHelper.GetAllReaders();
        bool hasBorrowedBook = false;

        foreach (Reader reader in readers)
        {
            hasBorrowedBook = reader.BorrowedBooks.Any(book => book.Title == bookTitle);

            if (hasBorrowedBook)
            {
                Console.WriteLine($"ID czytelnika: {reader.Id}");
                Console.WriteLine($"Imię: {reader.FirstName}");
                Console.WriteLine($"Nazwisko: {reader.LastName}");

                Console.WriteLine("Wypożyczone książki:");
                foreach (Book borrowedBook in reader.BorrowedBooks)
                {
                    Console.WriteLine($"- {borrowedBook.Title}");
                }
                Console.WriteLine("----------");
            }
        }
        if (!hasBorrowedBook)
        {
            Console.WriteLine("Nie znaleziono użytkownika");
        }
    }

}

public class Book
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }

    public Book(string title, string author)
    {
        Title = title;
        Author = author;
    }
}

class Program
{
    static void Main()
    {
        while (true)
        {
            Console.WriteLine("\nWybierz działanie:");
            Console.WriteLine("1. Wyświetl dane");
            Console.WriteLine("2. Wprowadź dane");
            Console.WriteLine("3. Modyfikuj dane");
            Console.WriteLine("4. Eksportuj dane");
            Console.WriteLine("5. Wyjdź");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    DisplayDataMenu();
                    break;

                case "2":
                    EnterDataMenu();
                    break;

                case "3":
                    ModifyDataMenu();
                    break;

                case "4":
                    ExportDataMenu();
                    break;

                case "5":
                    Environment.Exit(0);
                    break;

                default:
                    Console.WriteLine("Niepoprawne dane");
                    break;
            }
        }
    }

    static void DisplayDataMenu()
    {
        Console.WriteLine("\nWybierz opcję:");
        Console.WriteLine("1. Wyświetl listę książek");
        Console.WriteLine("2. Wyświetl listę czytelników");
        Console.WriteLine("3. Wyświetl wypożyczone książki czytelnika");
        Console.WriteLine("4. Wyświetl książkę po ID");
        Console.WriteLine("5. Wyświetl studenta na podstawie wypożyczonej książki");

        string displayChoice = Console.ReadLine();

        switch (displayChoice)
        {
            case "1":
                Library.DisplayBooks();
                break;

            case "2":
                Library.DisplayReaders();
                break;

            case "3":
                Console.Write("Podaj ID czytelnika, aby wyświetlić wypożyczone książki: ");
                string readerIDToDisplay = Console.ReadLine();

                Reader readerToDisplay = DbHelper.GetAllReaders().FirstOrDefault(r => r.Id.ToString() == readerIDToDisplay);

                if (readerToDisplay != null)
                {
                    Library.DisplayBorrowedBooks(readerToDisplay);
                }
                else
                {
                    Console.WriteLine("Nie znaleziono czytelnika o podanym ID.");
                }
                break;
            case "4":
                Console.WriteLine("Podaj ID książki: ");
                string bookId = Console.ReadLine();

                Book bookToDisplay = DbHelper.GetAllBooks().FirstOrDefault(b => b.Id.ToString() == bookId);

                Library.DisplayBook(bookToDisplay);
                break;
            case "5":
                Console.Write("Podaj tytuł wypożyczonej książki: ");
                string borrowedBookTitle = Console.ReadLine();
                Library.DisplayReaderByBorrowedBook(borrowedBookTitle);
                break;

            default:
                Console.WriteLine("Niepoprawne dane");
                break;
        }
    }

    static void EnterDataMenu()
    {
        Console.WriteLine("\nWybierz opcję:");
        Console.WriteLine("1. Dodaj książkę");
        Console.WriteLine("2. Dodaj czytelnika");
        Console.WriteLine("3. Wypożycz książkę");

        string enterChoice = Console.ReadLine();

        switch (enterChoice)
        {
            case "1":
                Console.Write("Wprowadź nazwę książki: ");
                string title = Console.ReadLine();

                Console.Write("Wprowadź autora książki: ");
                string author = Console.ReadLine();

                Book newBook = new Book(title, author);
                Library.AddBook(newBook);
                break;

            case "2":
                Console.Write("Podaj imię czytelnika: ");
                string firstName = Console.ReadLine();

                Console.Write("Podaj nazwisko czytelnika: ");
                string lastName = Console.ReadLine();

                Reader newReader = new Reader
                {
                    FirstName = firstName,
                    LastName = lastName
                };

                Library.AddReader(newReader);
                break;
            case "3":
                Console.Write("Podaj ID czytelnika: ");
                string readerID = Console.ReadLine();
                Console.Write("Podaj tytuł książki do wypożyczenia: ");
                string bookTitle = Console.ReadLine();

                Reader selectedReader = DbHelper.GetAllReaders().FirstOrDefault(r => r.Id.ToString() == readerID);
                Book selectedBook = DbHelper.GetAllBooks().FirstOrDefault(b => b.Title == bookTitle);

                if (selectedReader != null && selectedBook != null)
                {
                    Library.BorrowBook(selectedReader, selectedBook);
                }
                else
                {
                    Console.WriteLine("Nie znaleziono czytelnika lub książki o podanych danych.");
                }
                break;

            default:
                Console.WriteLine("Niepoprawne dane");
                break;
        }
    }

    static void ModifyDataMenu()
    {
        Console.WriteLine("\nWybierz opcję:");
        Console.WriteLine("1. Usuń książkę");
        Console.WriteLine("2. Usuń czytelnika");
        Console.WriteLine("3. Aktualizuj książkę");
        Console.WriteLine("4. Aktualizuj czytelnika");
        Console.WriteLine("5. Usuń wypożyczoną książkę");

        string modifyChoice = Console.ReadLine();

        switch (modifyChoice)
        {
            case "1":
                Console.Write("Podaj tytuł książki do usunięcia: ");
                string bookTitleToRemove = Console.ReadLine();
                Library.RemoveBook(bookTitleToRemove);
                break;

            case "2":
                Console.Write("Podaj ID czytelnika do usunięcia: ");
                string readerIDToRemove = Console.ReadLine();
                Library.RemoveReader(readerIDToRemove);
                break;

            case "3":
                Console.Write("Podaj tytuł książki do aktualizacji: ");
                string bookTitleToUpdate = Console.ReadLine();

                Console.Write("Nowy tytuł książki: ");
                string newTitle = Console.ReadLine();

                Console.Write("Nowy autor książki: ");
                string newAuthor = Console.ReadLine();

                Library.UpdateBook(bookTitleToUpdate, newTitle, newAuthor);
                break;

            case "4":
                Console.Write("Podaj ID czytelnika do aktualizacji: ");
                string readerIDToUpdate = Console.ReadLine();

                Console.Write("Nowe imię czytelnika: ");
                string newFirstName = Console.ReadLine();

                Console.Write("Nowe nazwisko czytelnika: ");
                string newLastName = Console.ReadLine();

                Library.UpdateReader(readerIDToUpdate, newFirstName, newLastName);
                break;

            case "5":
                Console.Write("Podaj ID czytelnika: ");
                string readerId = Console.ReadLine();

                Console.Write("Podaj tytuł książki do zwrócenia: ");
                string bookTitle = Console.ReadLine();

                Reader reader = DbHelper.GetAllReaders().FirstOrDefault(r => r.Id.ToString() == readerId);
                Book book = reader.BorrowedBooks.FirstOrDefault(b => b.Title == bookTitle);

                Library.ReturnBook(reader, book);
                break;

            default:
                Console.WriteLine("Niepoprawne dane");
                break;
        }
    }

    static void ExportDataMenu()
    {
        Console.WriteLine("\nWybierz opcję:");
        Console.WriteLine("1. Eksportuj dane użytkownika do pliku JSON");
        Console.WriteLine("2. Eksportuj dane książki do pliku JSON");

        string exportChoice = Console.ReadLine();

        switch (exportChoice)
        {
            case "1":
                Console.Write("Podaj ID użytkownika: ");
                string userId = Console.ReadLine();

                Reader reader = DbHelper.GetAllReaders().FirstOrDefault(r => r.Id.ToString() == userId);

                if (reader != null)
                {
                    string currentDirectory = Directory.GetCurrentDirectory() + "\\JsonFiles";
                    Console.WriteLine(currentDirectory);
                    Directory.CreateDirectory(currentDirectory);
                    string filePath = Path.Combine(currentDirectory, $"{reader.FirstName}_{reader.LastName}_user.json");

                    File.WriteAllText(filePath, reader.ToJson());

                    Console.WriteLine($"Dane użytkownika z ID {userId} zostały eksportowane do pliku JSON.");
                }
                else
                {
                    Console.WriteLine($"Nie znaleziono użytkownika o ID {userId}.");
                }
                break;

            case "2":
                Console.Write("Podaj tytuł książki: ");
                string bookTitle = Console.ReadLine();

                Book book = DbHelper.GetAllBooks().FirstOrDefault(b => b.Title == bookTitle);

                if (book != null)
                {
                    string currentDirectory = Directory.GetCurrentDirectory() + "\\JsonFiles";
                    Console.WriteLine(currentDirectory);
                    Directory.CreateDirectory(currentDirectory);
                    string filePath = Path.Combine(currentDirectory, $"{book.Title}_{book.Author}_book.json");

                    File.WriteAllText(filePath, book.ToJson());

                    Console.WriteLine($"Dane książki z tytułem {bookTitle} zostały eksportowane do pliku JSON.");
                }
                else
                {
                    Console.WriteLine($"Nie znaleziono książki o nazwie {bookTitle}.");
                }
                break;

            default:
                Console.WriteLine("Niepoprawne dane");
                break;
        }
    }
}
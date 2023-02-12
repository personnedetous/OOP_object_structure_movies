using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MoviesDatabase
{
    class Program
    {
        public static void Main(string[] args)
        {
            // Movie movie1 = new Movie("Pelíšky", "drama", "1999/11/17", 0.91);
            // MovieCollection mc = new MovieCollection();
            // mc.addMovie(movie1);
            // mc.addMovie(new Movie("Foo", "scfi", "2020/06/30", 0.21));

            string filename = "moviesDB.yaml";
            // MovieCollection mcDebug = new MovieCollection();
            // mcDebug.addMovie(new Movie("Foo", "scfi", "2020/06/30", 0.21));
            // mcDebug.SerializeToYaml(filename);
            MovieCollection mc = MovieCollection.DeserializeFromYaml(filename);
            while (true)
            {
                Console.WriteLine("===Půjčovna filmů===");
                Console.WriteLine("1. Seznam všech filmů");
                Console.WriteLine("2. Přidat film");
                Console.WriteLine("3. Vypůjčit film");
                Console.WriteLine("4. Vrátit film");
                Console.WriteLine("5. Uložit a ukončit");
                Console.WriteLine("Zadejte číslo volby: ");

                int choice = int.Parse(Console.ReadLine());
                switch (choice)
                {
                    case 1:
                        mc.printAllMovies();
                        break;
                    case 2:
                        AddMovie(mc);
                        break;
                    case 3:
                        BorrowMovie(mc);
                        break;
                    case 4:
                        ReturnMovie(mc);
                        break;
                    case 5:
                        mc.SerializeToYaml(filename);
                        return;
                    default:
                        Console.WriteLine("Nevalidní volba, zkuste znovu");
                        break;
                }
            }
        }
        public static void BorrowMovie(MovieCollection mc)
        {
            Console.WriteLine("Napište jméno filmu k vypůjčení: ");
            string searchedTitle = Console.ReadLine();
            Movie foundMovie;
            try
            {
                foundMovie = mc.getMovie(searchedTitle);
                mc.BorrowMovie(foundMovie);
            }
            catch (MovieNotFound e)
            {
                Console.WriteLine(e.Message);
            }
            catch (MovieIsAlreadyBorrowed e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void ReturnMovie(MovieCollection mc)
        {
            Console.WriteLine("Napište jméno filmu, který chcete vrátit: ");
            string searchedTitle = Console.ReadLine();
            Movie foundMovie;
            try
            {
                foundMovie = mc.getMovie(searchedTitle);
                mc.ReturnMovie(foundMovie);
            }
            catch (MovieNotFound e)
            {
                Console.WriteLine(e.Message);
            }
            catch (MovieIsAlreadyReturned e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void AddMovie(MovieCollection mc)
        {
            Console.WriteLine("Napište jméno filmu, který chcete vložit do půjčovny: ");
            string title = Console.ReadLine();
            Console.WriteLine("Napište žánr filmu: ");
            string genre = Console.ReadLine();
            Console.WriteLine("Napište datum vydání ve formátu RRRR/MM/DD: ");
            string releaseDate = Console.ReadLine();
            Console.WriteLine("Napište hodnocení filmu v %: ");
            string ratingInput = Console.ReadLine();
            double rating = Double.Parse(ratingInput) / 100;
            try
            {
                mc.addMovie(new Movie(title, genre, releaseDate, rating));
            }
            catch (MovieAlreadyExists e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Film byl úspěšně přidán do půjčovny");
        }
    }

    class MovieCollection
    {
        public List<Movie> movieList { get; set; }
        public MovieCollection()
        {
            this.movieList = new List<Movie>();
        }

        public void addMovie(Movie movie)
        {
            try
            {
                this.getMovie(movie.Title);
            }
            catch (MovieNotFound)
            {
                this.movieList.Add(movie);
                return;
            }
            throw new MovieAlreadyExists(string.Format("Film se jménem {0} nelze přidat do půjčovny. Půjčovna již teto titul vlastní", movie.Title));
        }

        public void printAllMovies()
        {
            if (movieList.Count == 0)
            {
                Console.WriteLine("V půjčovně nejsou žádné filmy");
                return;
            }
            foreach (Movie m in this.movieList)
            {
                Console.WriteLine(m);
            }
        }
        public void BorrowMovie(Movie movie)
        {
            //change movie.availability to false
            if (movie.Availability == true)
            {
                movie.Availability = false;
            }
            else
            {
                // movie is already borrowed
                throw new MovieIsAlreadyBorrowed(string.Format("Film se jménem {0} nebyl v půjčovně nalezen.", movie.Title));
            }
        }
        public void ReturnMovie(Movie movie)
        {
            //change movie.availability to true
            if (movie.Availability == false)
            {
                movie.Availability = true;
            }
            else
            {
                throw new MovieIsAlreadyReturned(string.Format("Film se jménem {0} nelze vrátit, již byl vrácen.", movie.Title));
            }
        }

        public Movie getMovie(string searchedTitle)
        /*
            Goes over the whole store and searches for the movie that
            matches the title in the argument
        */
        {
            foreach (Movie movie in movieList)
            {
                if (searchedTitle == movie.Title)
                {
                    return movie;
                }
            }
            throw new MovieNotFound(string.Format("Film se jménem {0} nebyl v půjčovně nalezen.", searchedTitle));
        }

        public void SerializeToYaml(string filename)
        {
            var serializer = new SerializerBuilder().Build();
            using (var writer = new StreamWriter(filename))
            {
                serializer.Serialize(writer, this);
            }
        }

        public static MovieCollection DeserializeFromYaml(string filename)
        {
            var deserializer = new DeserializerBuilder().Build();
            using (var reader = new StreamReader(filename))
            {
                return deserializer.Deserialize<MovieCollection>(reader);
            }
        }
    }
    class Movie
    {
        public string Title { get; set; } = null!;
        public string Genre { get; set; } = null!;
        public DateTime ReleaseDate { get; set; }
        public double Rating { get; set; }
        public bool Availability { get; set; }

        public Movie() { }
        public Movie(string title, string genre, string releaseDate, double rating)
        {
            this.Title = title;
            this.Genre = genre;
            this.ReleaseDate = DateTime.Parse(releaseDate);
            this.Rating = rating;
            this.Availability = true;
        }

        public Movie(string title, string genre, string releaseDate, double rating, bool availability)
            : this(title, genre, releaseDate, rating)
        {
            this.Availability = availability;
        }

        public override string ToString()
        {
            return string.Format("Titul: {0}, žánr: {1}, datum: {2}, hodnocení: {3}, dostupnost: {4}",
                                this.Title, this.Genre, this.ReleaseDate, this.toPercentString(), this.Availability);
        }

        public string toPercentString()
        {
            return this.Rating.ToString("P1", CultureInfo.InvariantCulture);
        }
    }

    public class MovieNotFound : Exception
    {
        public MovieNotFound(string message)
            : base(message)
        {
        }
    }

    public class MovieIsAlreadyBorrowed : Exception
    {
        public MovieIsAlreadyBorrowed(string message)
            : base(message)
        {
        }
    }
    public class MovieIsAlreadyReturned : Exception
    {
        public MovieIsAlreadyReturned(string message)
            : base(message)
        {
        }
    }

    public class MovieAlreadyExists : Exception
    {
        public MovieAlreadyExists(string message)
            : base(message)
        {
        }
    }
}


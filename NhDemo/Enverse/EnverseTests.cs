using System.Diagnostics;
using NhDemo.Configuration;
using System.Linq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace NhDemo.Enverse
{
    public class EnverseTests
    {
        private readonly ITestOutputHelper _output;

        public EnverseTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void SaveWithoutInverse()
        {
            long basketId, appleId;
            var basket = new Basket();
            basket.OwnerName = "grandmother";

            basket.Apples.Add(new Apple {Perimeter = 2.3});
            basket.Apples.Add(new Apple {Perimeter = 0.9});

            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                session.Save(basket);
                basketId = basket.Id;
                appleId = basket.Apples.First().Id;
                session.Flush();
                tran.Commit();
            }

            using (var session = SessionFactory.OpenSession())
            {
                session.Get<Basket>(basketId).ShouldNotBeNull();
                session.Get<Apple>(appleId).ShouldNotBeNull();
            }
        }

        [Fact]
        public void SaveWithInverse_ShouldNotBeSet()
        {
            long libraryId, bookId;
            var library = new Library();
            library.Name = "fantasy";

            library.Books.Add(new Book { Thickness = 2.3 });
            library.Books.Add(new Book { Thickness = 0.9 });

            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                session.Save(library);
                libraryId = library.Id;
                bookId = library.Books.First().Id;
                session.Flush();
                tran.Commit();
            }

            using (var session = SessionFactory.OpenSession())
            {
                var libraryFromDb = session.Get<Library>(libraryId);
                libraryFromDb.ShouldNotBeNull();
                var book = session.Get<Book>(bookId);
                book.ShouldNotBeNull();

                libraryFromDb.Books.ShouldBeEmpty();
            }
        }

        [Fact]
        public void SaveWithInverse_ReferenceToParentIsSet()
        {
            long libraryId, bookId;
            var library = new Library();
            library.Name = "fantasy";

            library.Books.Add(new Book { Thickness = 2.3, Library = library });
            library.Books.Add(new Book { Thickness = 0.9, Library = library });

            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                session.Save(library);
                libraryId = library.Id;
                bookId = library.Books.First().Id;
                session.Flush();
                tran.Commit();
            }

            using (var session = SessionFactory.OpenSession())
            {
                var libraryFromDb = session.Get<Library>(libraryId);
                libraryFromDb.ShouldNotBeNull();
                var book = session.Get<Book>(bookId);
                book.ShouldNotBeNull();

                libraryFromDb.Books.Count.ShouldBe(2);
            }
        }

        [Fact]
        public void WithoutInverse_Time()
        {
            using (SessionFactory.OpenSession())
            {
            }

            var stopwatch = Stopwatch.StartNew();
            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                session.SetBatchSize(100);
                for (int i = 0; i < 100; i++)
                {
                    var basket = new Basket();
                    basket.OwnerName = $"basket {i}";
                    for (int j = 0; j < 10; j++)
                    {
                        basket.Apples.Add(new Apple {Perimeter = j});
                    }
                    session.Save(basket);
                }
                session.Flush();
                tran.Commit();
            }

            stopwatch.Stop();
            var time = stopwatch.Elapsed;
            _output.WriteLine($"Without inverse: {time}");
        }

        [Fact]
        public void Inverse_Time()
        {
            using (SessionFactory.OpenSession())
            {
            }

            var stopwatch = Stopwatch.StartNew();
            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                session.SetBatchSize(100);
                for (int i = 0; i < 100; i++)
                {
                    var library = new Library();
                    library.Name = $"library {i}";
                    for (int j = 0; j < 10; j++)
                    {
                        library.Books.Add(new Book { Thickness = j, Library = library });
                    }
                    session.Save(library);
                }
                session.Flush();
                tran.Commit();
            }

            stopwatch.Stop();
            var time = stopwatch.Elapsed;
            _output.WriteLine($"With inverse: {time}");
        }
    }
}
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace NhDemo.Enverse
{
    public class Library
    {
        public Library()
        {
            Books = new HashSet<Book>();
        }

        public virtual long Id { get; set; }

        public virtual string Name { get; set; }

        public virtual ISet<Book> Books { get; set; }
    }

    public class Book
    {
        public virtual long Id { get; set; }

        public virtual Library Library { get; set; }

        public virtual double Thickness { get; set; }
    }

    public class LibraryMap : ClassMap<Library>
    {
        public LibraryMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("1000");
            Map(x => x.Name);
            HasMany(x => x.Books).Cascade.AllDeleteOrphan().Inverse();
        }
    }

    public class BookMap : ClassMap<Book>
    {
        public BookMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("1000");
            Map(x => x.Thickness);
            References(x => x.Library);
        }
    }
}
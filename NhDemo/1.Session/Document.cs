using FluentNHibernate.Mapping;

namespace NhDemo.Session
{
    public class Document
    {
        public virtual int Id { get; set; }

        public virtual string Number { get; set; }
    }

    public class DocumentMap : ClassMap<Document>
    {
        public DocumentMap()
        {
            Schema("Documents");
            Id(x => x.Id).GeneratedBy.HiLo("0");
            Map(x => x.Number);
        }
    }
}
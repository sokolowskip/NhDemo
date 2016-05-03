using FluentNHibernate.Mapping;
using NhDemo.Entities;

namespace NhDemo.Mappings
{
    public class DocumentMap : ClassMap<Document>
    {
        public DocumentMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("0");
            Map(x => x.Number);
        }
    }
}
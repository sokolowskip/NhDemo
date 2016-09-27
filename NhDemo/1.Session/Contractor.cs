using FluentNHibernate.Mapping;

namespace NhDemo.Session
{
    public class Contractor
    {
        public virtual long Id { get; set; }
        public virtual string Name { get; set; }
    }

    public class ContractorMap : ClassMap<Contractor>
    {
        public ContractorMap()
        {
            Schema("Documents");
            Id(x => x.Id).GeneratedBy.Native();
            Map(x => x.Name);
        }
    }
    
}
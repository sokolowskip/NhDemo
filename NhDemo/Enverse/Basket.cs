using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace NhDemo.Enverse
{
    public class Basket
    {
        public Basket()
        {
            Apples = new HashSet<Apple>();
        }

        public virtual long Id { get; set; }

        public virtual string OwnerName { get; set; }

        public virtual ISet<Apple> Apples { get; set; }   
    }

    public class Apple
    {
        public virtual long Id { get; set; }

        public virtual double Perimeter { get; set; }

    }

    public class BasketMap : ClassMap<Basket>
    {
        public BasketMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("10");
            Map(x => x.OwnerName);
            HasMany(x => x.Apples).Cascade.AllDeleteOrphan();
        }
    }

    public class AppleMap : ClassMap<Apple>
    {
        public AppleMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("10");
            Map(x => x.Perimeter);
        }
    }
}
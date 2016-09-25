using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentNHibernate.Mapping;
using NhDemo.Configuration;
using NHibernate.Criterion;
using Xunit;
using Xunit.Abstractions;

namespace NhDemo.Query
{
    [Collection("DatabaseFixtures")]
    public class QueryTests
    {
        private readonly ITestOutputHelper _output;

        public QueryTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void UsingTwoQueries()
        {
            using (var session = SessionFactory.OpenSession())
            {
                var stopwatch = Stopwatch.StartNew();
                Category categoryAlias = null;
                var productIds = session.QueryOver<Product>()
                        .JoinAlias(x => x.Categories, () => categoryAlias)
                        .WhereRestrictionOn(() => categoryAlias.Name)
                        .IsLike("99", MatchMode.Anywhere)
                        .Select(x => x.Id)
                        .List<Guid>()
                        .ToArray();

                OrderDetail orderDetail = null;
                var orders = session.QueryOver<Order>()
                    .JoinAlias(x => x.OrderDetails, () => orderDetail)
                    .WhereRestrictionOn(() => orderDetail.Product.Id).IsIn(productIds)
                    .List();

                stopwatch.Stop();
                _output.WriteLine($"Using two queries: {stopwatch.Elapsed}");
            }
        }

        [Fact]
        public void UsingSubqueryIn()
        {
            using (var session = SessionFactory.OpenSession())
            {
                var stopwatch = Stopwatch.StartNew();
                Category categoryAlias = null;
                var productsQuery = QueryOver.Of<Product>()
                        .JoinAlias(x => x.Categories, () => categoryAlias)
                        .WhereRestrictionOn(() => categoryAlias.Name)
                        .IsLike("99", MatchMode.Anywhere)
                        .Select(x => x.Id);

                OrderDetail orderDetail = null;
                var orders = session.QueryOver<Order>()
                    .JoinAlias(x => x.OrderDetails, () => orderDetail)
                    .WithSubquery.WhereProperty(() => orderDetail.Product.Id).In(productsQuery)
                    .List();

                stopwatch.Stop();
                _output.WriteLine($"Using subquery IN: {stopwatch.Elapsed}");
            }
        }

        [Fact]
        public void UsingSubqueryExists()
        {
            using (var session = SessionFactory.OpenSession())
            {
                var stopwatch = Stopwatch.StartNew();
                Category categoryAlias = null;
                OrderDetail orderDetail = null;

                var productsQuery = QueryOver.Of<Product>()
                        .JoinAlias(x => x.Categories, () => categoryAlias)
                        .WhereRestrictionOn(() => categoryAlias.Name)
                        .IsLike("99", MatchMode.Anywhere)
                        .Where(x => x.Id == orderDetail.Product.Id)
                        .Select(x => x.Id);

                
                var orders = session.QueryOver<Order>()
                    .JoinAlias(x => x.OrderDetails, () => orderDetail)
                    .WithSubquery.WhereExists(productsQuery)
                    .List();

                stopwatch.Stop();
                _output.WriteLine($"Using subquery EXISTS: {stopwatch.Elapsed}");
            }
        }
    }

    public class Category
    {
        public Category()
        {
            
        }

        public Category(string name)
        {
            Name = name;
        }

        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
    }

    public class Product
    {
        public Product()
        {
            Categories = new HashSet<Category>();
        }

        public Product(string name)
        {
            Name = name;
            Categories = new HashSet<Category>();
        }

        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual ISet<Category> Categories { get; set; }
    }

    public class Order
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public virtual Guid Id { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual string ClientName { get; set; }
        public virtual ISet<OrderDetail> OrderDetails { get; set; }
    }

    public class OrderDetail
    {
        public virtual Guid Id { get; set; }
        public virtual Product Product { get; set; }
        public virtual decimal Price { get; set; }
        public virtual decimal Quantity { get; set; }
        public virtual Order Order { get; set; }
    }

    public class ProductMap : ClassMap<Product>
    {
        public ProductMap()
        {
            Schema("Orders");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.Name);
            HasManyToMany(x => x.Categories).Schema("Orders");
        }
    }

    public class CategoryMap : ClassMap<Category>
    {
        public CategoryMap()
        {
            Schema("Orders");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.Name);
        }
    }

    public class OrderMap : ClassMap<Order>
    {
        public OrderMap()
        {
            Schema("Orders");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.ClientName);
            Map(x => x.Date);
            HasMany(x => x.OrderDetails).Cascade.All();
        }
    }

    public class OrderDetailMap : ClassMap<OrderDetail>
    {
        public OrderDetailMap()
        {
            Schema("Orders");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.Price);
            Map(x => x.Quantity);
            References(x => x.Product);
            References(x => x.Order);
        }
    }
}
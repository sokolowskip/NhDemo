using System;
using System.Collections.Generic;
using NhDemo.Configuration;
using NHibernate.Criterion;
using Shouldly;
using Xunit;

namespace NhDemo.Query
{
    [Collection("DatabaseFixtures")]
    public class HavingCountTest
    {
        class ProductRequst
        {
            public ProductRequst(Guid productId, decimal minPrice)
            {
                ProductId = productId;
                MinPrice = minPrice;
            }

            public Guid ProductId { get; }

            public decimal MinPrice { get;  }
        }

        [Fact]
        public void QueryWorks()
        {
            var request = new List<ProductRequst>
            {
                new ProductRequst(new Guid("7314459F-E48F-47BB-BAA1-A67F01457B07"), 100),
                new ProductRequst(new Guid("3162D706-030A-4C37-895C-A67F01457B3F"), 200)
            };
            FindOrderContainedProducts(request).ShouldNotBeNull();
        }

        private Order FindOrderContainedProducts(List<ProductRequst> products)
        {
            using (var session = SessionFactory.OpenSession())
            {
                OrderDetail orderDetailAlias = null;
                var subquery = QueryOver.Of<Order>()
                    .JoinAlias(x => x.OrderDetails, () => orderDetailAlias);

                var disjunction = new Disjunction();
                foreach (var productRequst in products)
                {
                    disjunction.Add(Restrictions.Where(() =>
                        orderDetailAlias.ProductId == productRequst.ProductId &&
                        orderDetailAlias.Price >= productRequst.MinPrice));
                }
                subquery.Where(disjunction);
                subquery.Where(Restrictions.Eq(Projections.Count<Order>(x => x.Id), products.Count));
                subquery.Select(Projections.GroupProperty(Projections.Property<Order>(x => x.Id)));
                subquery.Take(1);

                var query = session.QueryOver<Order>()
                    .Where(Subqueries.WhereProperty<Order>(x => x.Id).Eq(subquery));

                var result = query.SingleOrDefault();
                return result;
            }
        }
    }
}

using System;
using NhDemo.Configuration;
using NhDemo.Entities;
using NHibernate.Tuple;
using Shouldly;
using Xunit;

namespace NhDemo.Tests
{
    public class SessionTests
    {
        [Fact]
        public void SaveTest()
        {
            int documentId;
            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                var document = new Document();
                document.Number = "1/2016";

                session.Save(document);
                session.Flush();
                tran.Commit();
                documentId = document.Id;
            }

            using (var session = SessionFactory.OpenSession())
            {
                var document = session.Get<Document>(documentId);
                document.Number.ShouldBe("1/2016");
            }
        }
    }
}
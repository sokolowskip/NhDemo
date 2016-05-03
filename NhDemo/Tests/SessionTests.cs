using System;
using NhDemo.Configuration;
using NhDemo.Entities;
using NHibernate;
using NHibernate.Tuple;
using Shouldly;
using Xunit;

namespace NhDemo.Tests
{
    public class SessionTests
    {
        private readonly int _documentId;
        private string initialDocumentNumber = "1/2016";

        public SessionTests()
        {
            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                var document = new Document();
                document.Number = initialDocumentNumber;

                session.Save(document);
                session.Flush();
                tran.Commit();
                _documentId = document.Id;
            }
        }

        [Fact]
        public void SaveTest()
        {
            using (var session = SessionFactory.OpenSession())
            {
                var document = session.Get<Document>(_documentId);
                document.Number.ShouldBe(initialDocumentNumber);
            }
        }

        [Fact]
        public void UpdateTest()
        {
            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                var document = new Document
                {
                    Id = _documentId,
                    Number = "2/2016"
                };

                session.Update(document);
                session.Flush();
                tran.Commit();
            }

            using (var session = SessionFactory.OpenSession())
            {
                var document = session.Get<Document>(_documentId);
                document.Number.ShouldBe("2/2016");
            }
        }

        [Fact]
        public void Update_Should_ThrowExcpetion_WhenEntityWithSameId_IsAlreadyInSession()
        {
            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                session.Get<Document>(_documentId);
                var document = new Document
                {
                    Id = _documentId,
                    Number = "2/2016"
                };
                Assert.Throws<NonUniqueObjectException>(() => session.Update(document));
            }
        }
    }
}
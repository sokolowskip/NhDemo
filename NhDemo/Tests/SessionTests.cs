using NhDemo.Configuration;
using NhDemo.Entities;
using NHibernate;
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
        public void FlushUpdatesEntity_When_ItIsInSession()
        {
            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                var document = session.Get<Document>(_documentId);
                document.Number = "2/2016";

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
        public void WithoutFlush_EntityIsntUpdated()
        {
            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                var document = session.Get<Document>(_documentId);
                document.Number = "2/2016";

                tran.Commit();
            }

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

        [Fact]
        public void Merge_Should_UpdateEntity_WhenAnotherInstanceIsInSession()
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

                session.Merge(document);
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
        public void Merge_Should_UpdateAttributesOfOtherObjectInSessionWithSameId()
        {
            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                var documentFromDatabase = session.Get<Document>(_documentId);
                var document = new Document
                {
                    Id = _documentId,
                    Number = "2/2016"
                };

                session.Merge(document);

                documentFromDatabase.Number.ShouldBe("2/2016");
                document.ShouldNotBe(documentFromDatabase);
            }
        }

        [Fact]
        public void SecondMerge_Should_UpdateOnlyEntityFromDatabase()
        {
            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                var documentFromDatabase = session.Get<Document>(_documentId);
                var document = new Document
                {
                    Id = _documentId,
                    Number = "2/2016"
                };

                session.Merge(document);

                var anotherDocument = new Document
                {
                    Id = _documentId,
                    Number = "3/2016"
                };

                session.Merge(anotherDocument);

                documentFromDatabase.Number.ShouldBe("3/2016");
                document.Number.ShouldBe("2/2016");
                anotherDocument.ShouldNotBe(documentFromDatabase);
                anotherDocument.ShouldNotBe(document);
            }
        }


        [Fact]
        public void MergeNonPersistedObject_ShouldPersistIt()
        {
            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                var documentFromDatabase = session.Get<Document>(_documentId);
                var document = new Document
                {
                    Id = _documentId + 1,
                    Number = "7/2016"
                };

                session.Merge(document);
                session.Flush();
                tran.Commit();
            }

            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                var document = session.Get<Document>(_documentId + 1);
                document.Number.ShouldBe("7/2016");
            }
        }

        [Fact]
        public void UpdateNonPersistedObject_ShouldThrowExcpetion()
        {
            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                var document = new Document
                {
                    Id = _documentId + 1,
                    Number = "7/2016"
                };

                session.Update(document);
                var exception = Assert.Throws<StaleStateException>(() => session.Flush());
                exception.Message.ShouldBe("Batch update returned unexpected row count from update; actual row count: 0; expected: 1");
            }
        }

        [Fact]
        public void SaveOrUpdate_EntityWithNonExistentId_ShouldThrowException()
        {
            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                var document = new Document
                {
                    Id = _documentId + 1,
                    Number = "7/2016"
                };

                session.SaveOrUpdate(document);
                var exception = Assert.Throws<StaleStateException>(() => session.Flush());
                exception.Message.ShouldBe("Batch update returned unexpected row count from update; actual row count: 0; expected: 1");
            }
        }

        [Fact]
        public void SaveOrUpdate_EntityWithNoId_ShouldPersistIt()
        {
            int anotherDocumentd;
            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                var anotherDocument = new Document
                {
                    Number = "7/2016"
                };

                session.SaveOrUpdate(anotherDocument);
                session.Flush();
                anotherDocument.Id.ShouldNotBe(_documentId);
                anotherDocumentd = anotherDocument.Id;
                tran.Commit();
            }

            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                var anotherDocument = session.Get<Document>(anotherDocumentd);
                anotherDocument.Number.ShouldBe("7/2016");
            }
        }

        [Fact]
        public void Evict_ShouldRemoveEntityFromSession()
        {
            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                var document = session.Get<Document>(_documentId);
                session.Contains(document).ShouldBeTrue();

                session.Evict(document);

                session.Contains(document).ShouldBeFalse();
            }
        }

        [Fact]
        public void EvictedEntity_Should_NotBeSaved()
        {
            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                var document = session.Get<Document>(_documentId);
                session.Evict(document);
                document.Number = "2/2016";
                session.Flush();
                tran.Commit();
            }

            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                var document = session.Get<Document>(_documentId);
                document.Number.ShouldBe(initialDocumentNumber);
            }
        }

        [Fact]
        public void ClearShould_RemoveAllEntitiesFromSession()
        {
            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                var document = session.Get<Document>(_documentId);
                session.Statistics.EntityCount.ShouldBe(1);
                session.Clear();
                session.Statistics.EntityCount.ShouldBe(0);
            }
        }

        [Fact]
        public void DeleteTest()
        {
            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                session.Delete(session.Load<Document>(_documentId));
                session.Flush();
                tran.Commit();
            }

            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                var document = session.Get<Document>(_documentId);
                document.ShouldBeNull();
            }
        }
    }
}
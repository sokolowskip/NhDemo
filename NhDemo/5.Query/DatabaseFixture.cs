using System.IO;
using NhDemo.Configuration;
using Xunit;

namespace NhDemo.Query
{
    public class DatabaseFixture
    {
        public DatabaseFixture()
        {
            var current = Directory.GetCurrentDirectory();
            var path = Path.Combine(current, @"..\..\..\nhdemo.bak");
            SessionFactory.RestoreDatabase("NhDemo", path);
        }   
    }

    [CollectionDefinition("DatabaseFixtures")]
    public class DatabaseFixtures : ICollectionFixture<DatabaseFixture>
    {
    }
}
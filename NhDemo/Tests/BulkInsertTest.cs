using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using NhDemo.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace NhDemo.Tests
{
    public class BulkInsertTest
    {
        private readonly ITestOutputHelper _output;

        public BulkInsertTest(ITestOutputHelper  output)
        {
            _output = output;
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public void ElapsedTimeOfInsert(int batchSize)
        {
            using (SessionFactory.OpenSession()) { }
        
            
            const int count = 1000;
            List<Analytic> list = new List<Analytic>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(new Analytic
                {
                    Dimension = "DIM" + i,
                    Value = "VAL" + i
                });
            }

            Stopwatch timer = Stopwatch.StartNew();

            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                session.SetBatchSize(batchSize);
                foreach (var analytic in list)
                {
                    session.Save(analytic);
                }

                session.Flush();
                tran.Commit();
            }

            timer.Stop();
            _output.WriteLine($"{batchSize}: {timer.Elapsed}");
        }
    }

    public class Analytic
    {
        public virtual Guid Id { get; set; }
        
        public virtual string Dimension { get; set; }

        public virtual string Value { get; set; }
    }

    public class AnalyticMap : ClassMap<Analytic>
    {
        public AnalyticMap()
        {
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.Dimension);
            Map(x => x.Value);
        }
    }
}

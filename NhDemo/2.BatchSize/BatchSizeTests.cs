using System;
using System.Collections.Generic;
using System.Diagnostics;
using FluentNHibernate.Mapping;
using NhDemo.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace NhDemo.BatchSize
{
    public class BatchSizeTests
    {
        private readonly ITestOutputHelper _output;

        public BatchSizeTests(ITestOutputHelper  output)
        {
            _output = output;
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(500)]
        [InlineData(1000)]
        public void ElapsedTimeOfInsert(int batchSize)
        {
            using (SessionFactory.OpenSession()) { }
        
            
            const int count = 10000;
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
            Schema("Analytics");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.Dimension);
            Map(x => x.Value);
        }
    }
}

using Shouldly;
using Xunit;

namespace NhDemo
{
    public class XUnitTests
    {
        [Fact]
        public void AddTwoIntegersTest()
        {
            (2+2).ShouldBe(4);
        }
    }
}

using Shouldly;
using Xunit;

namespace NhDemo.Tests
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

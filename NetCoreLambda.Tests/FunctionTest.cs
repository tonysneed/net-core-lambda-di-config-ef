using Xunit;
using Amazon.Lambda.TestUtilities;
using Moq;
using NetCoreLambda.Abstractions;

namespace NetCoreLambda.Tests
{
    public class FunctionTest
    {
        [Fact]
        public async void Function_Should_Return_Product_By_Id()
        {
            // Mock IProductRepository
            var expected = new Product
            {
                Id = 1,
                ProductName = "Chai",
                UnitPrice = 10
            };
            var mockRepo = new Mock<IProductRepository>();
            mockRepo.Setup(m => m.GetProduct(It.IsAny<int>())).ReturnsAsync(expected);

            // Invoke the lambda function and confirm correct value is returned
            var function = new Function(mockRepo.Object);
            var result = await function.FunctionHandler("1", new TestLambdaContext());
            Assert.Equal(expected, result);
        }
    }
}

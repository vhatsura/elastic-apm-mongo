using System;
using System.Diagnostics.CodeAnalysis;
using Moq;
using Xunit;

namespace Elastic.Apm.Mongo.Tests
{
    // in .Net Framework such attribute cannot be used on assembly level
    [ExcludeFromCodeCoverage]
    public class CompositeDisposableTests
    {
        [Fact]
        public void Dispose_ShouldDisposeOnlyOnce()
        {
            // Arrange
            var disposableMock = new Mock<IDisposable>();

            var compositeDisposable = new CompositeDisposable()
                .Add(disposableMock.Object);

            // Act
            compositeDisposable.Dispose();
            compositeDisposable.Dispose();

            // Assert
            disposableMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public void Add_ShouldThrowsException_WhenObjectDisposed()
        {
            // Arrange
            var compositeDisposable = new CompositeDisposable();
            compositeDisposable.Dispose();

            // Act + Assert
            Assert.Throws<ObjectDisposedException>(() => compositeDisposable.Add(Mock.Of<IDisposable>()));
        }
    }
}

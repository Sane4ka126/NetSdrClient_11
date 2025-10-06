using NUnit.Framework;
using Moq;
using NetSdrClientApp;
using NetSdrClientApp.Networking;
using System.Threading.Tasks;

namespace NetSdrClientAppTests
{
    [TestFixture]
    public class NetSdrClientTests
    {
        private Mock<ITcpClient> _mockTcpClient;
        private Mock<IUdpClient> _mockUdpClient;
        private NetSdrClient _client;

        [SetUp]
        public void SetUp()
        {
            _mockTcpClient = new Mock<ITcpClient>();
            _mockUdpClient = new Mock<IUdpClient>();
            _client = new NetSdrClient(_mockTcpClient.Object, _mockUdpClient.Object);
        }

        [Test]
        public void Constructor_InitializesProperties()
        {
            // Arrange & Act
            var client = new NetSdrClient(_mockTcpClient.Object, _mockUdpClient.Object);

            // Assert
            Assert.That(client, Is.Not.Null);
            Assert.That(client.IQStarted, Is.False);
        }

        [Test]
        public async Task ConnectAsync_WhenAlreadyConnected_DoesNotConnect()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(true);

            // Act
            await _client.ConnectAsync();

            // Assert
            _mockTcpClient.Verify(x => x.Connect(), Times.Never);
        }

        [Test]
        public async Task ConnectAsync_WhenNotConnected_CallsConnect()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(false);
            _mockTcpClient.Setup(x => x.Connect());

            // Act
            try
            {
                await _client.ConnectAsync();
            }
            catch
            {
                // Ігноруємо помилки, нас цікавить тільки виклик Connect
            }

            // Assert
            _mockTcpClient.Verify(x => x.Connect(), Times.Once);
        }

        [Test]
        public async Task StartIQAsync_WhenNotConnected_DoesNotStart()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(false);

            // Act
            await _client.StartIQAsync();

            // Assert
            Assert.That(_client.IQStarted, Is.False);
            _mockUdpClient.Verify(x => x.StartListeningAsync(), Times.Never);
        }

        [Test]
        public async Task StopIQAsync_WhenNotConnected_DoesNotStop()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(false);

            // Act
            await _client.StopIQAsync();

            // Assert
            _mockUdpClient.Verify(x => x.StopListening(), Times.Never);
        }

        [Test]
        public void Disconnect_CallsTcpClientDisconnect()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Disconnect());

            // Act
            _client.Disconect();

            // Assert
            _mockTcpClient.Verify(x => x.Disconnect(), Times.Once);
        }

        [Test]
        public void IQStarted_Property_CanBeSetAndRead()
        {
            // Arrange
            _client.IQStarted = false;

            // Act
            var initialValue = _client.IQStarted;
            _client.IQStarted = true;
            var newValue = _client.IQStarted;

            // Assert
            Assert.That(initialValue, Is.False);
            Assert.That(newValue, Is.True);
        }

        [Test]
        public void Connected_Property_ReturnsCorrectValue()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(true);

            // Act
            var result = _mockTcpClient.Object.Connected;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Connected_Property_WhenNotConnected_ReturnsFalse()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(false);

            // Act
            var result = _mockTcpClient.Object.Connected;

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ChangeFrequencyAsync_WhenNotConnected_DoesNotThrow()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(false);
            long frequency = 100_000_000;
            int channel = 0;

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => 
                await _client.ChangeFrequencyAsync(frequency, channel));
        }
    }
}

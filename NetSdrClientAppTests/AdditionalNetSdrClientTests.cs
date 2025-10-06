using NUnit.Framework;
using Moq;
using NetSdrClientApp;
using NetSdrClientApp.Networking;
using NetSdrClientApp.Messages;
using System;
using System.Threading.Tasks;

namespace NetSdrClientAppTests
{
    public class AdditionalNetSdrClientTests
    {
        private Mock<ITcpClient> _mockTcpClient;
        private Mock<IUdpClient> _mockUdpClient;
        private NetSdrClient _client;

        [SetUp]
        public void Setup()
        {
            _mockTcpClient = new Mock<ITcpClient>();
            _mockUdpClient = new Mock<IUdpClient>();
            _client = new NetSdrClient(_mockTcpClient.Object, _mockUdpClient.Object);
        }

        [Test]
        public async Task StartIQAsync_WhenAlreadyStarted_DoesNotStartAgain()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(true);
            _mockTcpClient.Setup(x => x.SendMessageAsync(It.IsAny<byte[]>()))
                .Callback<byte[]>(_ => 
                {
                    _mockTcpClient.Raise(x => x.MessageReceived += null, null, new byte[] { 0x00 });
                })
                .Returns(Task.CompletedTask);
            
            _client.IQStarted = true;

            // Act
            await _client.StartIQAsync();

            // Assert
            Assert.That(_client.IQStarted, Is.True);
            _mockUdpClient.Verify(x => x.StartListeningAsync(), Times.Never);
        }

        [Test]
        public async Task StopIQAsync_WhenNotStarted_DoesNotStop()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(true);
            _client.IQStarted = false;

            // Act
            await _client.StopIQAsync();

            // Assert
            Assert.That(_client.IQStarted, Is.False);
            _mockUdpClient.Verify(x => x.StopListening(), Times.Never);
        }

        [Test]
        public async Task ChangeFrequencyAsync_WhenNotConnected_DoesNotSendMessage()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(false);

            // Act
            await _client.ChangeFrequencyAsync(100000000, 0);

            // Assert
            _mockTcpClient.Verify(x => x.SendMessageAsync(It.IsAny<byte[]>()), Times.Never);
        }

        [Test]
        public async Task ChangeFrequencyAsync_WithZeroFrequency_SendsMessage()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(true);
            _mockTcpClient.Setup(x => x.SendMessageAsync(It.IsAny<byte[]>()))
                .Callback<byte[]>(_ => 
                {
                    _mockTcpClient.Raise(x => x.MessageReceived += null, null, new byte[] { 0x00 });
                })
                .Returns(Task.CompletedTask);

            // Act
            await _client.ChangeFrequencyAsync(0, 0);

            // Assert
            _mockTcpClient.Verify(x => x.SendMessageAsync(It.IsAny<byte[]>()), Times.Once);
        }

        [Test]
        public async Task ChangeFrequencyAsync_WithMaxFrequency_SendsMessage()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(true);
            _mockTcpClient.Setup(x => x.SendMessageAsync(It.IsAny<byte[]>()))
                .Callback<byte[]>(_ => 
                {
                    _mockTcpClient.Raise(x => x.MessageReceived += null, null, new byte[] { 0x00 });
                })
                .Returns(Task.CompletedTask);

            long maxFrequency = 2000000000; // 2 GHz

            // Act
            await _client.ChangeFrequencyAsync(maxFrequency, 0);

            // Assert
            _mockTcpClient.Verify(x => x.SendMessageAsync(It.IsAny<byte[]>()), Times.Once);
        }

        [Test]
        public void Disconnect_WhenNotConnected_StillCallsDisconnect()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(false);
            _mockTcpClient.Setup(x => x.Disconnect());

            // Act
            _client.Disconect();

            // Assert
            _mockTcpClient.Verify(x => x.Disconnect(), Times.Once);
        }

        [Test]
        public void ConnectAsync_WhenConnectionFails_HandlesProperly()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(false);
            _mockTcpClient.Setup(x => x.Connect()).Throws(new InvalidOperationException("Connection failed"));

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _client.ConnectAsync());
        }

        [Test]
        public void IQStarted_Property_CanBeSetAndGet()
        {
            // Arrange
            var initialValue = _client.IQStarted;

            // Act
            _client.IQStarted = true;
            var valueAfterSet = _client.IQStarted;
            
            _client.IQStarted = false;
            var finalValue = _client.IQStarted;

            // Assert
            Assert.That(initialValue, Is.False);
            Assert.That(valueAfterSet, Is.True);
            Assert.That(finalValue, Is.False);
        }
    }
}

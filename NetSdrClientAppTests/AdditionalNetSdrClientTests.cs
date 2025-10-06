using Xunit;
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
        private readonly Mock<ITcpClient> _mockTcpClient;
        private readonly Mock<IUdpClient> _mockUdpClient;
        private readonly NetSdrClient _client;

        public AdditionalNetSdrClientTests()
        {
            _mockTcpClient = new Mock<ITcpClient>();
            _mockUdpClient = new Mock<IUdpClient>();
            _client = new NetSdrClient(_mockTcpClient.Object, _mockUdpClient.Object);
        }

        [Fact]
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
            Assert.True(_client.IQStarted);
            _mockUdpClient.Verify(x => x.StartListeningAsync(), Times.Never);
        }

        [Fact]
        public async Task StopIQAsync_WhenNotStarted_DoesNotStop()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(true);
            _client.IQStarted = false;

            // Act
            await _client.StopIQAsync();

            // Assert
            Assert.False(_client.IQStarted);
            _mockUdpClient.Verify(x => x.StopListening(), Times.Never);
        }

        [Fact]
        public async Task ChangeFrequencyAsync_WhenNotConnected_DoesNotSendMessage()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(false);

            // Act
            await _client.ChangeFrequencyAsync(100000000, 0);

            // Assert
            _mockTcpClient.Verify(x => x.SendMessageAsync(It.IsAny<byte[]>()), Times.Never);
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public async Task ConnectAsync_WhenConnectionFails_HandlesProperly()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(false);
            _mockTcpClient.Setup(x => x.Connect()).Throws(new InvalidOperationException("Connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _client.ConnectAsync());
        }

        [Fact]
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
            Assert.False(initialValue);
            Assert.True(valueAfterSet);
            Assert.False(finalValue);
        }
    }
}

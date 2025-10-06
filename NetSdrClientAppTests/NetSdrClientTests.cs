using NUnit.Framework;
using Moq;
using NetSdrClientApp;
using NetSdrClientApp.Networking;
using NetSdrClientApp.Messages;
using System;
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
        public async Task ConnectAsync_WhenNotConnected_SendsInitializationMessages()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(false);
            _mockTcpClient.Setup(x => x.Connect());

            // Налаштування для SetResult при отриманні повідомлень
            _mockTcpClient.Setup(x => x.SendMessageAsync(It.IsAny<byte[]>()))
                .Callback<byte[]>(_ =>
                {
                    // Симулюємо отримання відповіді
                    _mockTcpClient.Raise(x => x.MessageReceived += null, null, new byte[] { 0x00 });
                })
                .Returns(Task.CompletedTask);

            // Act
            await _client.ConnectAsync();

            // Assert
            _mockTcpClient.Verify(x => x.Connect(), Times.Once);
            _mockTcpClient.Verify(x => x.SendMessageAsync(It.IsAny<byte[]>()), Times.Exactly(3));
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
            _mockTcpClient.Verify(x => x.SendMessageAsync(It.IsAny<byte[]>()), Times.Never);
        }

        [Test]
        public async Task StartIQAsync_WhenConnected_StartsUdpListening()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(true);
            _mockTcpClient.Setup(x => x.SendMessageAsync(It.IsAny<byte[]>()))
                .Callback<byte[]>(_ =>
                {
                    _mockTcpClient.Raise(x => x.MessageReceived += null, null, new byte[] { 0x00 });
                })
                .Returns(Task.CompletedTask);

            _mockUdpClient.Setup(x => x.StartListeningAsync()).Returns(Task.CompletedTask);

            // Act
            await _client.StartIQAsync();

            // Assert
            Assert.That(_client.IQStarted, Is.True);
            _mockUdpClient.Verify(x => x.StartListeningAsync(), Times.Once);
            _mockTcpClient.Verify(x => x.SendMessageAsync(It.IsAny<byte[]>()), Times.Once);
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
        public async Task StopIQAsync_WhenConnected_StopsUdpListening()
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
            await _client.StopIQAsync();

            // Assert
            Assert.That(_client.IQStarted, Is.False);
            _mockUdpClient.Verify(x => x.StopListening(), Times.Once);
            _mockTcpClient.Verify(x => x.SendMessageAsync(It.IsAny<byte[]>()), Times.Once);
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
        public async Task ChangeFrequencyAsync_WhenConnected_SendsFrequencyMessage()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(true);
            _mockTcpClient.Setup(x => x.SendMessageAsync(It.IsAny<byte[]>()))
                .Callback<byte[]>(_ =>
                {
                    _mockTcpClient.Raise(x => x.MessageReceived += null, null, new byte[] { 0x00 });
                })
                .Returns(Task.CompletedTask);

            long frequency = 100000000; // 100 MHz
            int channel = 0;

            // Act
            await _client.ChangeFrequencyAsync(frequency, channel);

            // Assert
            _mockTcpClient.Verify(x => x.SendMessageAsync(It.IsAny<byte[]>()), Times.Once);
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
        public void Constructor_InitializesProperties()
        {
            // Arrange & Act
            var client = new NetSdrClient(_mockTcpClient.Object, _mockUdpClient.Object);

            // Assert
            Assert.That(client, Is.Not.Null);
            Assert.That(client.IQStarted, Is.False);
        }

        [Test]
        public async Task ChangeFrequencyAsync_WithDifferentChannels_SendsCorrectMessage()
        {
            // Arrange
            _mockTcpClient.Setup(x => x.Connected).Returns(true);
            byte[] capturedMessage = null;

            _mockTcpClient.Setup(x => x.SendMessageAsync(It.IsAny<byte[]>()))
                .Callback<byte[]>(msg =>
                {
                    capturedMessage = msg;
                    _mockTcpClient.Raise(x => x.MessageReceived += null, null, new byte[] { 0x00 });
                })
                .Returns(Task.CompletedTask);

            // Act
            await _client.ChangeFrequencyAsync(144000000, 1);

            // Assert
            Assert.That(capturedMessage, Is.Not.Null);
            _mockTcpClient.Verify(x => x.SendMessageAsync(It.IsAny<byte[]>()), Times.Once);
        }
    }
}
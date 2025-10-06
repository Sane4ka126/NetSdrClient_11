using NetSdrClientApp;
using NetSdrClientApp.Networking;

Console.WriteLine(@"Usage:
C - connect
D - disconnet
F - set frequency
S - Start/Stop IQ listener
Q - quit");

var tcpClient = new TcpClientWrapper("127.0.0.1", 5000);
var udpClient = new UdpClientWrapper(60000);

var netSdr = new NetSdrClient(tcpClient, udpClient);

while (true)
{
    var key = Console.ReadKey(intercept: true).Key;
    if (key == ConsoleKey.C)
    {
        await netSdr.ConnectAsync();
    }
    else if (key == ConsoleKey.D)
    {
        netSdr.Disconect();
    }
    else if (key == ConsoleKey.F)
    {
        await netSdr.ChangeFrequencyAsync(20000000, 1);
    }
    else if (key == ConsoleKey.S)
    {
        if (netSdr.IQStarted)
        {
            await netSdr.StopIQAsync();
        }
        else
        {
            await netSdr.StartIQAsync();
        }
    }
    else if (key == ConsoleKey.Q)
    {
        break;
    }
}

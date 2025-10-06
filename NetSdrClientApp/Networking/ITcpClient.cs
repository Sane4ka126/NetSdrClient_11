using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NetSdrClientApp.Networking
{
    public interface ITcpClient
    {
        void Connect();
        void Disconnect();
        Task SendMessageAsync(byte[] data);

        event EventHandler<byte[]> MessageReceived;
        public bool Connected { get; }
    }
}

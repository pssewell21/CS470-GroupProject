using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Server
{
    public partial class Server : Form
    {
        // The default port number to listen for client connections
        private const int PORT = 800;
        private const int BUFFER_SIZE = 8192;

        public Server()
        {
            InitializeComponent(); 
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            outputTextBlock.Text += $"Server Started...{Environment.NewLine}";

            var udpClient = new UdpClient(PORT);

            while (true)
            {
                var responseData = Encoding.ASCII.GetBytes("SomeResponseData");

                var endPoint = new IPEndPoint(IPAddress.Any, 0);
                var clientRequestData = udpClient.Receive(ref endPoint);
                var clientRequest = Encoding.ASCII.GetString(clientRequestData);

                outputTextBlock.Text += $"Received {clientRequest} from {endPoint.Address.ToString()}";
                udpClient.Send(responseData, responseData.Length, endPoint);
            }
        }
    }
}

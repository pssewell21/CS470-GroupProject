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
        private const int TIMEOUT = 5000;

        private const string REQUEST_STRING = "GetData";

        public Server()
        {
            InitializeComponent(); 
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            outputTextBlock.Text += $"{DateTime.Now}: Server Started...{Environment.NewLine}";
            outputTextBlock.Refresh();

            var udpClient = new UdpClient(PORT);
            udpClient.Client.ReceiveTimeout = TIMEOUT;

            while (true)
            {
                var responseData = Encoding.ASCII.GetBytes("Hello World");

                var endPoint = new IPEndPoint(IPAddress.Any, 0);
                var clientRequestData = udpClient.Receive(ref endPoint);
                var clientRequest = Encoding.ASCII.GetString(clientRequestData);

                if (clientRequest.Equals(REQUEST_STRING))
                {
                    outputTextBlock.Text += $"{DateTime.Now}: Received {clientRequest} from {endPoint.Address.ToString()}. Sending response{Environment.NewLine}{Environment.NewLine}";
                    udpClient.Send(responseData, responseData.Length, endPoint);
                }
                else
                {
                    outputTextBlock.Text += $"{DateTime.Now}: Received {clientRequest} from {endPoint.Address.ToString()}. Unrecognized command{Environment.NewLine}{Environment.NewLine}";
                }
                
                outputTextBlock.Refresh();
            }
        }
    }
}

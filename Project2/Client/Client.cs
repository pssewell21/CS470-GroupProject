using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Client
{
    public partial class Client : Form
    {
        private const int PORT = 800;

        public Client()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            outputTextBlock.Text += $"Client Started...{Environment.NewLine}";

            var udpClient = new UdpClient();
            var requestData = Encoding.ASCII.GetBytes("SomeRequestData");
            var endPoint = new IPEndPoint(IPAddress.Any, 0);

            udpClient.EnableBroadcast = true;
            udpClient.Send(requestData, requestData.Length, new IPEndPoint(IPAddress.Broadcast, PORT));

            var serverResponseData = udpClient.Receive(ref endPoint);
            var serverResponse = Encoding.ASCII.GetString(serverResponseData);
            outputTextBlock.Text += $"Recived {serverResponseData} from {serverResponse}";

            udpClient.Close();
        }
    }
}

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
        private const int TIMEOUT = 5000;

        private const string REQUEST_STRING = "GetData";
        private const string REQUEST_BADSTRING = "BadRequest";

        public Client()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            try
            { 
                outputTextBlock.Text += $"{DateTime.Now}: Client Started...{Environment.NewLine}";
                outputTextBlock.Refresh();

                var udpClient = new UdpClient();
                udpClient.Client.ReceiveTimeout = TIMEOUT;

                var requestData = Encoding.ASCII.GetBytes(REQUEST_STRING);
                //var requestData = Encoding.ASCII.GetBytes(REQUEST_BADSTRING);
                var endPoint = new IPEndPoint(IPAddress.Any, 0);

                udpClient.EnableBroadcast = true;
                udpClient.Send(requestData, requestData.Length, new IPEndPoint(IPAddress.Broadcast, PORT));

                try
                {
                    var serverResponseData = udpClient.Receive(ref endPoint);
                    var serverResponse = Encoding.ASCII.GetString(serverResponseData);

                    outputTextBlock.Text += $"{DateTime.Now}: Recived {serverResponse} from {endPoint.Address.ToString()}{Environment.NewLine}{Environment.NewLine}";
                }
                catch (SocketException se)
                {
                    if (se.SocketErrorCode == SocketError.TimedOut)
                    {
                        outputTextBlock.Text += $"{DateTime.Now}: Connection timed out waiting for a response from the server{Environment.NewLine}{Environment.NewLine}";
                    }
                    else
                    {
                        throw;
                    }
                }

                outputTextBlock.Refresh();
                udpClient.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error Occurred");
                Environment.Exit(1);
            }
        }
    }
}

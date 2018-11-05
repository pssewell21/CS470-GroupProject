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

        private const string DISCOVER_MESSAGE = "DiscoverServer";
        private const string GET_DATA_MESSAGE = "GetData";
        private const string INVALID_MESSAGE = "BadRequest";

        private IPEndPoint _endPoint;
        private Socket _socket;
        private IPAddress _serverIpAddress;

        // This lock acts as a mutex to make sure only one action is being performed on the socket at a time
        private readonly object _socketLock = new object();

        public Client()
        {
            InitializeComponent();
        }

        private void discoverServerButton_Click(object sender, EventArgs e)
        {
            try
            { 
                outputTextBlock.Text += $"{DateTime.Now}: Discovering Server...{Environment.NewLine}";
                outputTextBlock.Refresh();

                var udpClient = new UdpClient();
                udpClient.Client.ReceiveTimeout = TIMEOUT;

                var requestData = Encoding.ASCII.GetBytes(DISCOVER_MESSAGE);
                //var requestData = Encoding.ASCII.GetBytes(INVALID_MESSAGE);
                var endPoint = new IPEndPoint(IPAddress.Any, 0);

                udpClient.EnableBroadcast = true;
                udpClient.Send(requestData, requestData.Length, new IPEndPoint(IPAddress.Broadcast, PORT));

                try
                {
                    var serverResponseData = udpClient.Receive(ref endPoint);
                    var serverResponse = Encoding.ASCII.GetString(serverResponseData);

                    outputTextBlock.Text += $"{DateTime.Now}: Received \"{serverResponse}\" from {endPoint.Address.ToString()}. Ready to request data{Environment.NewLine}{Environment.NewLine}";

                    _serverIpAddress = endPoint.Address;
                    getDataButton.Enabled = true;
                    discoverServerButton.Enabled = false;
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

        private void getDataButton_Click(object sender, EventArgs e)
        {
            try
            {
                outputTextBlock.Text += $"{DateTime.Now}: Getting data from server...{Environment.NewLine}";
                outputTextBlock.Refresh();

                // Create a new socket to connect to the server with
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP)
                {
                    ReceiveTimeout = TIMEOUT
                };

                var outboundPort = GetOutboundPort();

                lock (_socketLock)
                {
                    _endPoint = new IPEndPoint(IPAddress.Any, outboundPort);

                    // Bind using the port selected
                    _socket.Bind(_endPoint);

                    // Connect to the server using the IP entered by the user or the fallback address
                    _socket.Connect(_serverIpAddress, PORT);

                    if (_socket.Connected)
                    {
                        outputTextBlock.Text += $"Client is connected using port {outboundPort}{Environment.NewLine}";
                    }
                    else
                    {
                        outputTextBlock.Text += "Client is not connected";
                    }

                    outputTextBlock.Refresh();
                }

                var requestData = Encoding.ASCII.GetBytes(GET_DATA_MESSAGE);

                try
                {
                    lock (_socketLock)
                    {
                        _socket.Send(requestData);
                    }

                    lock (_socketLock)
                    {
                        // Create a byte array to hold the received message
                        var bytes = new byte[_socket.ReceiveBufferSize];

                        // Receive the message
                        var bytesRead = _socket.Receive(bytes);

                        // Convert the message to a string (This application sends only strings between tiers)
                        var serverResponse = Encoding.ASCII.GetString(bytes, 0, bytesRead);

                        // Display received data
                        outputTextBlock.Text += $"{DateTime.Now}: Received \"{serverResponse}\" from {_endPoint.Address.ToString()}. Ready to request data{Environment.NewLine}{Environment.NewLine}";
                        outputTextBlock.Refresh();
                    }

                    //var serverResponseData = tcpClient.Receive(ref _endPoint);
                    //var serverResponse = Encoding.ASCII.GetString(serverResponseData);
                    //
                    //outputTextBlock.Text += $"{DateTime.Now}: Received \"{serverResponse}\" from {endPoint.Address.ToString()}. Ready to request data{Environment.NewLine}{Environment.NewLine}";
                    //
                    //_serverIpAddress = endPoint.Address;
                    //getDataButton.Enabled = true;
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








                //var tcpClient = new TcpClient();
                //tcpClient.Client.ReceiveTimeout = TIMEOUT;

                //var requestData = Encoding.ASCII.GetBytes(GET_DATA_MESSAGE);
                ////var requestData = Encoding.ASCII.GetBytes(INVALID_MESSAGE);
                //var endPoint = new IPEndPoint(IPAddress.Any, 0);

                //tcpClient.Send(requestData, requestData.Length, new IPEndPoint(IPAddress.Broadcast, PORT));

                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error Occurred");
                Environment.Exit(1);
            }
        }

        // Picks a random port to send data from. Must be unique from other clients to prevent exceptions from occurring
        private int GetOutboundPort()
        {
            var randomNumberGenerator = new Random();
            return randomNumberGenerator.Next(51200, 52000);
        }


    }
}

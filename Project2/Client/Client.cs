using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    public partial class Client : Form
    {
        private const int NETWORK_DISCOVERY_PORT = 800;
        private const int CLIENT_CONNECTION_PORT = 801;
        private const int TIMEOUT = 5000;

        private const string DISCOVER_MESSAGE = "DiscoverServer";
        private const string GET_DATA_MESSAGE = "GetData";
        private const string INVALID_MESSAGE = "BadRequest";

        private IPEndPoint _discoveryEndPoint;
        private UdpClient _udpClient;
        private IPEndPoint _dataEndPoint;
        private Socket _socket;
        private IPAddress _serverIpAddress;

        private bool findingServer;
               
        public delegate void delUpdateUiTextBox(string text);

        // This lock acts as a mutex to make sure only one action is being performed on the socket at a time
        private readonly object _socketLock = new object();

        public Client()
        {
            InitializeComponent();
        }

        private void DiscoverServer()
        {
            // Sets up a different thread to send messages to the server
            var thread = new Thread(() => FindServer());
            thread.Start();
        }

        private void FindServer()
        {
            try
            {
                AppendTextBox($"Discovering Server...");

                _serverIpAddress = null;

                _udpClient = new UdpClient();
                _udpClient.Client.ReceiveTimeout = TIMEOUT;

                var requestData = Encoding.ASCII.GetBytes(DISCOVER_MESSAGE);
                //var requestData = Encoding.ASCII.GetBytes(INVALID_MESSAGE);

                _discoveryEndPoint = new IPEndPoint(IPAddress.Broadcast, NETWORK_DISCOVERY_PORT);
                _udpClient.EnableBroadcast = true;
                _udpClient.Send(requestData, requestData.Length, _discoveryEndPoint);

                try
                {
                    var endPoint = new IPEndPoint(IPAddress.Any, 0);
                    var serverResponseData = _udpClient.Receive(ref endPoint);
                    var serverResponse = Encoding.ASCII.GetString(serverResponseData);

                    AppendTextBox($"Received \"{serverResponse}\" from {endPoint.ToString()}. Ready to request data");

                    _serverIpAddress = endPoint.Address;
                }
                catch (SocketException se)
                {
                    if (se.SocketErrorCode == SocketError.TimedOut)
                    {
                        AppendTextBox($"Connection timed out waiting for a response from the server");
                    }
                    else
                    {
                        throw;
                    }
                }

                _udpClient.Close();
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        private void GetData()
        {
            try
            {
                AppendTextBox($"Getting data from server...");

                // Create a new socket to connect to the server with
                if (_socket == null)
                {
                    var outboundPort = GetOutboundPort();

                    _dataEndPoint = new IPEndPoint(IPAddress.Any, outboundPort);

                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP)
                    {
                        ReceiveTimeout = TIMEOUT
                    };

                    lock (_socketLock)
                    {
                        // Bind using the port selected
                        _socket.Bind(_dataEndPoint);

                        // Connect to the server using the IP entered by the user or the fallback address
                        _socket.Connect(_serverIpAddress, CLIENT_CONNECTION_PORT);

                        if (_socket.Connected)
                        {
                            AppendTextBox($"Client is connected using port {outboundPort}");
                        }
                        else
                        {
                            AppendTextBox($"Client is not connected");
                        }
                    }
                }

                // Sets up a different thread to send messages to the server
                var thread = new Thread(() => GetDataFromServer());
                thread.Start();
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        private void GetDataFromServer()
        {
            try
            {
                var requestData = Encoding.ASCII.GetBytes(GET_DATA_MESSAGE);

                lock (_socketLock)
                {
                    _socket.Send(requestData);

                    // Create a byte array to hold the received message
                    var bytes = new byte[_socket.ReceiveBufferSize];

                    // Receive the message
                    var bytesRead = _socket.Receive(bytes);

                    // Convert the message to a string (This application sends only strings between tiers)
                    var serverResponse = Encoding.ASCII.GetString(bytes, 0, bytesRead);

                    // Display received data
                    AppendTextBox($"Received \"{serverResponse}\" from {_socket.RemoteEndPoint}");
                }
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.TimedOut)
                {
                    AppendTextBox($"Connection timed out waiting for a response from the server");
                }
                else
                {
                    throw;
                }
            }
        }

        // Picks a random port to send data from. Must be unique from other clients to prevent exceptions from occurring
        private int GetOutboundPort()
        {
            var randomNumberGenerator = new Random();
            return randomNumberGenerator.Next(51200, 52000);
        }

        private void Disconnect()
        {
            try
            {
                lock (_socketLock)
                {
                    if (_socket != null && _socket.Connected)
                    {
                        AppendTextBox($"Disconnecting from the server");

                        _socket.Disconnect(false);

                        Thread.Sleep(1000);

                        _socket.Dispose();
                        _socket = null;
                    }
                }
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            RunButton.Enabled = false;
            RunButton.Refresh();

            DiscoverServer();

            if (_serverIpAddress != null)
            {
                GetData();
                Disconnect();
            }

            RunButton.Enabled = true;
            RunButton.Refresh();
        }

        private void AppendTextBox(string message)
        {
            if (InvokeRequired)
            {
                outputTextBlock.BeginInvoke(new delUpdateUiTextBox(AppendTextBox), message);

                return;
            }

            outputTextBlock.Text = $"{DateTime.Now}: {message}{Environment.NewLine}{outputTextBlock.Text}";
            outputTextBlock.Refresh();
        }

        private void HandleException(Exception e)
        {
            MessageBox.Show(e.ToString(), "Error Occurred");
            Environment.Exit(1);
        }
    }
}

using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Server
{
    public partial class Server : Form
    {
        // The default port number to listen for client connections
        private const int NETWORK_DISCOVERY_PORT = 800;
        private const int CLIENT_CONNECTION_PORT = 801;
        private const int MAX_QUEUED_CONNECTIONS = 8192;
        private const int TIMEOUT = 5000;

        private const string DISCOVER_MESSAGE = "DiscoverServer";
        private const string GET_DATA_MESSAGE = "GetData";

        private const string STORE_DATA = "This is data about our store";

        private IPEndPoint _discoveryEndPoint;
        private UdpClient _udpClient;
        private IPEndPoint _clientEndPoint;
        private Socket _socket;
        private IPAddress _localIpAddress;

        public delegate void delUpdateUiTextBox(string text);

        public Server()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, EventArgs ea)
        {
            try
            {
                AppendTextBox($"Server Started...");

                if (_udpClient == null)
                {
                    _udpClient = new UdpClient(NETWORK_DISCOVERY_PORT);
                }

                _clientEndPoint = new IPEndPoint(IPAddress.Any, CLIENT_CONNECTION_PORT);

                Thread.Sleep(300);

                // Sets up different threads to process messages from clients
                var thread = new Thread(() => ListenForClients());
                thread.Start();
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        private void ListenForClients()
        {
            try
            {
                // Busy wait for clients to connect
                while (true)
                {
                    _discoveryEndPoint = new IPEndPoint(IPAddress.Any, NETWORK_DISCOVERY_PORT);
                    var clientRequestData = _udpClient.Receive(ref _discoveryEndPoint);
                    var clientRequest = Encoding.ASCII.GetString(clientRequestData);

                    if (clientRequest.Equals(DISCOVER_MESSAGE))
                    {
                        AppendTextBox($"Received \"{clientRequest}\" from {_discoveryEndPoint.ToString()}. Sending response");
                        var interfaces = NetworkInterface.GetAllNetworkInterfaces().Where(o => o.OperationalStatus == OperationalStatus.Up);

                        // If we have more than 1 interface on the machine, a network connection exists (Assuming no vitual interfaces exist)
                        if (interfaces.Count() > 1)
                        {
                            var upInterface = interfaces.Where(o => o.NetworkInterfaceType != NetworkInterfaceType.Loopback).FirstOrDefault();

                            if (upInterface != null)
                            {
                                // Get the first IPv4 address bound to the NIC and use it
                                var unicastAddress = upInterface.GetIPProperties().UnicastAddresses.Where(o => o.PrefixLength == 24).FirstOrDefault();

                                if (unicastAddress != null)
                                {
                                    _localIpAddress = unicastAddress.Address;
                                }
                                else
                                {
                                    AppendTextBox($"Unable to get IP interface. Make sure you are connected to a network. Failing over to binding to the Loopback interface");
                                    _localIpAddress = IPAddress.Loopback;
                                }
                            }
                            else
                            {
                                AppendTextBox($"Unable to get IP interface. Make sure you are connected to a network. Failing over to binding to the Loopback interface");
                                _localIpAddress = IPAddress.Loopback;
                            }
                        }
                        else
                        {
                            _localIpAddress = IPAddress.Loopback;
                        }

                        var responseData = Encoding.ASCII.GetBytes(_localIpAddress.ToString());

                        _udpClient.Send(responseData, responseData.Length, _discoveryEndPoint);
                    }
                    else
                    {
                        AppendTextBox($"Received \"{clientRequest}\" from {_discoveryEndPoint.ToString()}. Unrecognized message");
                    }

                    // Create a new socket for clients to connect to
                    if (_socket == null)
                    {
                        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

                        // Bind using the default port
                        _socket.Bind(_clientEndPoint);
                        _socket.Listen(MAX_QUEUED_CONNECTIONS);
                    }

                    // Accept incoming connections
                    var socket = _socket.Accept();

                    AppendTextBox($"Client connected from {socket.RemoteEndPoint}");

                    // Sets up different threads to process messages from clients
                    var thread = new Thread(() => ProcessMessages(socket));
                    thread.Start();
                }
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        // The method used to prcess messages from clients
        private void ProcessMessages(Socket socket)
        {
            try
            {
                // Busy wait. Process received messages and check for messages to send to the client
                while (true)
                {
                    // Receive any messages on the socket
                    if (socket.Available > 0)
                    {
                        var bytes = new byte[socket.ReceiveBufferSize];
                        var bytesRead = socket.Receive(bytes);

                        // Convert to a string
                        var dataReceived = Encoding.ASCII.GetString(bytes, 0, bytesRead);

                        if (dataReceived.Equals(GET_DATA_MESSAGE))
                        {
                            AppendTextBox($"Received \"{dataReceived}\" from {socket.RemoteEndPoint}");

                            // Convert and send the response to the client
                            var responseBytes = Encoding.ASCII.GetBytes(STORE_DATA);
                            socket.Send(responseBytes);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                HandleException(e);
            }
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

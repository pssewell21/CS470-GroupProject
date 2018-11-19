using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Client : Form
    {
        // Constants for network functionality
        private const int NETWORK_DISCOVERY_PORT = 800;
        private const int CLIENT_CONNECTION_PORT = 801;
        private const int TIMEOUT = 5000;

        private const string DISCOVER_MESSAGE = "DiscoverServer";
        private const string GET_DATA_MESSAGE = "GetData";
        private const string INVALID_MESSAGE = "BadRequest";

        // Variables for network functionality
        private IPEndPoint _discoveryEndPoint;
        private UdpClient _udpClient;
        private IPEndPoint _dataEndPoint;
        private Socket _socket;
        private IPAddress _serverIpAddress;
               
        public delegate void delUpdateUiTextBox(string text);

        // This lock acts as a mutex to make sure only one action is being performed on the socket at a time
        private readonly object _socketLock = new object();

        public Client()
        {
            InitializeComponent();
        }

        private async Task DiscoverServer()
        {
            // Sets up a different thread to send messages to the server
            await Task.Run(() => FindServer());
        }

        // Thread to discover the server
        private void FindServer()
        {
            try
            {
                // Debug printing
#if DEBUG
                DebugAppendTextBox($"Discovering Server...");
#endif
                // Local variables
                _serverIpAddress = null;
                _udpClient = new UdpClient();
                _udpClient.Client.ReceiveTimeout = TIMEOUT;
                
                // Make the discovery message into a Byte array
                var requestData = Encoding.ASCII.GetBytes(DISCOVER_MESSAGE);

                // Broadcast the discovery message
                _discoveryEndPoint = new IPEndPoint(IPAddress.Broadcast, NETWORK_DISCOVERY_PORT);
                _udpClient.EnableBroadcast = true;
                _udpClient.Send(requestData, requestData.Length, _discoveryEndPoint);

                // Listen for a response
                try
                {
                    var endPoint = new IPEndPoint(IPAddress.Any, 0);
                    var serverResponseData = _udpClient.Receive(ref endPoint);
                    var serverResponse = Encoding.ASCII.GetString(serverResponseData);

#if DEBUG
                    DebugAppendTextBox($"Received \"{serverResponse}\" from {endPoint.ToString()}. Ready to request data");
#endif
                    // Save the IP address the server responds with
                    _serverIpAddress = endPoint.Address;
                }
                catch (SocketException se)
                {
                    if (se.SocketErrorCode == SocketError.TimedOut)
                    {
#if DEBUG
                        DebugAppendTextBox($"Connection timed out waiting for a response from the server");
#endif
                    }
                    else
                    {
                        throw;
                    }
                }

                // Close the UDPclient
                _udpClient.Close();
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        private async Task GetData()
        {
            try
            {
                // Debug printing
#if DEBUG
                DebugAppendTextBox($"Getting data from server...");
#endif

                // Create a new socket to connect to the server with
                if (_socket == null)
                {
                    //Grab a port to use
                    var outboundPort = GetOutboundPort();

                    // Create an endpoint with that port.
                    _dataEndPoint = new IPEndPoint(IPAddress.Any, outboundPort);

                    // Set up the socket
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
#if DEBUG
                            DebugAppendTextBox($"Client is connected using port {outboundPort}");
#endif
                        }
                        else
                        {
#if DEBUG
                            DebugAppendTextBox($"Client is not connected");
#endif
                        }
                    }
                }

                // Sets up a different thread to send messages to the server
                await Task.Run(() => GetDataFromServer());
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        // Thread to retrieve the data from the server
        private void GetDataFromServer()
        {
            try
            {
                // Change the request information string to a Byte array
                var requestData = Encoding.ASCII.GetBytes(GET_DATA_MESSAGE);

                lock (_socketLock)
                {
                    // Send the request
                    _socket.Send(requestData);

                    // Create a byte array to hold the received message
                    var bytes = new byte[_socket.ReceiveBufferSize];

                    // Receive the message
                    var bytesRead = _socket.Receive(bytes);

                    // Convert the message to a string (This application sends only strings between tiers)
                    var serverResponse = Encoding.ASCII.GetString(bytes, 0, bytesRead);

                    // Display received data
#if DEBUG
                    DebugAppendTextBox($"Received \"{serverResponse}\" from {_socket.RemoteEndPoint}");
#else
                    ReleaseAppendTextBox($"{serverResponse}");
#endif
                }
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.TimedOut)
                {
#if DEBUG
                    DebugAppendTextBox($"Connection timed out waiting for a response from the server");
#endif
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

        // Disconnects from the server
        private void Disconnect()
        {
            try
            {
                lock (_socketLock)
                {
                    if (_socket != null && _socket.Connected)
                    {
#if DEBUG
                        DebugAppendTextBox($"Disconnecting from the server");
#endif
                        var requestData = Encoding.ASCII.GetBytes("disconnect");
                        _socket.Send(requestData);
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

        // Button to run the client
        private async void RunButton_Click(object sender, EventArgs e)
        {
            RunButton.Enabled = false;
            RunButton.Refresh();

            await DiscoverServer();

            if (_serverIpAddress != null)
            {
                await GetData();
                Disconnect();
            }

            RunButton.Enabled = true;
            RunButton.Refresh();
        }

        // Debug mode text display function
        private void DebugAppendTextBox(string message)
        {
            if (InvokeRequired)
            {
                outputTextBlock.BeginInvoke(new delUpdateUiTextBox(DebugAppendTextBox), message);

                return;
            }

            outputTextBlock.Text = $"{DateTime.Now}: {message}{Environment.NewLine}{outputTextBlock.Text}";
            outputTextBlock.Refresh();
        }
        
        // Non-Debug text display function
        private void ReleaseAppendTextBox(string message)
        {
            if (InvokeRequired)
            {
                outputTextBlock.BeginInvoke(new delUpdateUiTextBox(ReleaseAppendTextBox), message);

                return;
            }

            outputTextBlock.Text = $"{message}{Environment.NewLine}{outputTextBlock.Text}";
            outputTextBlock.Refresh();
        }

        private void HandleException(Exception e)
        {
            MessageBox.Show(e.ToString(), "Error Occurred");
            Environment.Exit(1);
        }
    }
}

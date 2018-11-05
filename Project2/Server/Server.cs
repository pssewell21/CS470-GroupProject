using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class Server : Form
    {
        // The default port number to listen for client connections
        private const int NETWORK_DISCOVERY_PORT = 800;
        private const int CLIENT_CONNECTION_PORT = 801;
        private const int BUFFER_SIZE = 8192;
        private const int TIMEOUT = 5000;

        private const string DISCOVER_MESSAGE = "DiscoverServer";
        private const string GET_DATA_MESSAGE = "GetData";

        private IPEndPoint _endPoint;
        private Socket _socket;

        public Server()
        {
            InitializeComponent(); 
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            try
            {
                outputTextBlock.Text += $"{DateTime.Now}: Server Started...{Environment.NewLine}";
                outputTextBlock.Refresh();

                //var udpClient = new UdpClient(PORT);

                // Create a new socket for clients to connect to
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

                // Bind to the IP address found above using the default port
                _socket.Bind(new IPEndPoint(IPAddress.Any, PORT));
                _socket.Listen(BUFFER_SIZE);

                Thread.Sleep(1000);

                // Busy wait for clients to connect
                while (true)
                {
                    // Accept incoming connections
                    var socket = _socket.Accept();

                    outputTextBlock.Text += $"Client connected from {socket.RemoteEndPoint}{Environment.NewLine}";
                    outputTextBlock.Refresh();

                    // Sets up async tasks that run on different threads to process messages from clients
                    Task task = new Task(() => ProcessMessages(socket));
                    task.Start();

                    // If the task throws an exception, catch it and rethrow to handle it
                    if (task.Exception != null)
                    {
                        throw task.Exception;
                    }
                }

                //while (true)
                //{
                //    var responseData = Encoding.ASCII.GetBytes("Hello World");
                //
                //    var endPoint = new IPEndPoint(IPAddress.Any, 0);
                //    var clientRequestData = udpClient.Receive(ref endPoint);
                //    var clientRequest = Encoding.ASCII.GetString(clientRequestData);
                //
                //    if (clientRequest.Equals(DISCOVER_MESSAGE))
                //    {
                //        outputTextBlock.Text += $"{DateTime.Now}: Received \"{clientRequest}\" from {endPoint.Address.ToString()}. Sending response{Environment.NewLine}{Environment.NewLine}";
                //        udpClient.Send(responseData, responseData.Length, endPoint);
                //    }
                //    else if (clientRequest.Equals(DISCOVER_MESSAGE))
                //    {
                //
                //    }
                //    else
                //    {
                //        outputTextBlock.Text += $"{DateTime.Now}: Received \"{clientRequest}\" from {endPoint.Address.ToString()}. Unrecognized message{Environment.NewLine}{Environment.NewLine}";
                //    }
                //
                //    outputTextBlock.Refresh();
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error Occurred");
                Environment.Exit(1);
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

                        if (dataReceived.Equals(DISCOVER_MESSAGE))
                        {
                            outputTextBlock.Text += $"{DateTime.Now}: Received \"{dataReceived}\" from {socket.RemoteEndPoint}. Sending Discovery Message{Environment.NewLine}{Environment.NewLine}";


                            //udpClient.Send(responseData, responseData.Length, endPoint);
                        }
                        else if (dataReceived.Equals(GET_DATA_MESSAGE))
                        {
                            outputTextBlock.Text += $"{DateTime.Now}: Received \"{dataReceived}\" from {socket.RemoteEndPoint}. Sending Data Message{Environment.NewLine}{Environment.NewLine}";

                            //udpClient.Send(responseData, responseData.Length, endPoint);
                        }
                        else
                        {
                            outputTextBlock.Text += $"{DateTime.Now}: Received \"{dataReceived}\" from {socket.RemoteEndPoint}. Unrecognized message{Environment.NewLine}{Environment.NewLine}";
                        }

                        // Don't need to wait so busily. I can probably take this out, I don't think it solved the problem 
                        // I was trying to fix, but I don't want to risk breaking anything else and 300ms is plently responsive
                        //Thread.Sleep(300);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}

using System;
using System.Windows.Forms;

namespace Server
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var server = new Server();
            server.Shown += new EventHandler(ServerShownHandler);
            Application.Run(server);
        }

        public static void ServerShownHandler(object sender, EventArgs ea)
        {
            try
            {
                var server = sender as Server;

                server.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error Occurred");
                Environment.Exit(1);
            }
        }
    }
}

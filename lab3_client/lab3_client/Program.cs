using System.Net;
using System.Net.Sockets;
using System.Text;

namespace lab3_client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                Console.WriteLine("Enter server IP address (leave empty for localhost): ");
                string ipAddress = Console.ReadLine();
                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = "127.0.0.1";
                }

                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), 8080);
                clientSocket.Connect(serverEndPoint); // 8080 is the port number the server is listening on, i wont change it
                Console.WriteLine("Connected successfully, congrats");

                Console.WriteLine("Enter yo message, foo: ");
                string message = Console.ReadLine() ?? "This foo sent empty message, gah daym"; // gah daym, this is so sad, can we hit 50 likes?
                byte[] data = Encoding.UTF8.GetBytes(message);
                clientSocket.Send(data);

                byte[] buffer = new byte[1024];
                int bytesRead = clientSocket.Receive(buffer);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Server response: {response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally 
            {
                if (clientSocket.Connected) clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }

        }
    }
}

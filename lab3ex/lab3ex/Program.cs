using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace lab3ex
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Pick a mode:");
            Console.WriteLine("1 - Run as SERVER to receive data");
            Console.WriteLine("2 - Run as CLIENT to send data");
            Console.Write("Your choice: ");

            string mode = Console.ReadLine() ?? "";

            int port = 8081;
            int bufferSize = 1024 * 1024;

            if (mode == "1")
            {
                RunServer(port, bufferSize);
            }
            else if (mode == "2")
            {
                Console.Write("Enter IP address of the server (press Enter for localhost): ");
                string ipInput = Console.ReadLine() ?? "";
                if (string.IsNullOrEmpty(ipInput)) ipInput = "127.0.0.1";

                RunClient(ipInput, port, bufferSize);
            }
            else
            {
                Console.WriteLine("Invalid choice you dummy");
            }
        }
        static void RunServer(int port, int bufferSize)
        {
            Socket server = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Any, port));
            server.Listen(10);

            Socket client = server.Accept();
            Console.WriteLine($"Someone connected^ {client.RemoteEndPoint}");

            byte[] data = new byte[bufferSize];
            long totalBytesReceived = 0;

            Stopwatch timer = new();

            Console.WriteLine("Here we go");

            while (true)
            {
                int bytesRead = client.Receive(data);
                if (bytesRead == 0) break;
                if (!timer.IsRunning) timer.Start();
                totalBytesReceived += bytesRead;
            }

            timer.Stop();
            client.Shutdown(SocketShutdown.Both);
            client.Close();
            server.Close();

            double seconds = timer.Elapsed.TotalSeconds;
            double megabytes = (double)totalBytesReceived / (1024 * 1024);
            double megabytesPerSecond = megabytes / seconds;
            double megabitsPerSecond = megabytesPerSecond * 8;

            Console.WriteLine("\nRESULTS");
            Console.WriteLine($"Received: {megabytes:F2} MB");
            Console.WriteLine($"Time: {seconds:F2} sec");
            Console.WriteLine($"Avg speed: {megabytesPerSecond:F2} MB/sec"); 
            Console.WriteLine($"Speed in bits: {megabitsPerSecond:F2} Mbps");
        }

        static void RunClient(string ip, int port, int bufferSize)
        {
            Socket client = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(IPAddress.Parse(ip), port);
            Console.WriteLine($"\nConnected to {ip}:{port}");

            byte[] data = new byte[bufferSize];
            long totalBytesSent = 0;

            Console.WriteLine("Sending data (+- 5 seconds)");

            Stopwatch sw = Stopwatch.StartNew();

            while (sw.Elapsed.TotalSeconds < 5)
            {
                client.Send(data);
                totalBytesSent += data.Length;
            }

            client.Shutdown(SocketShutdown.Both);
            client.Close();

            double megabytesSent = (double)totalBytesSent / (1024 * 1024);
            Console.WriteLine($"Sent {megabytesSent:F2} MB.");
        }
    }
}

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace lab_3 
{ 
	public class Program
	{
		public static void Main()
		{
            Socket tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 8080);
            tcpSocket.Bind(endPoint);

            tcpSocket.Listen(10);

            Socket client = tcpSocket.Accept();
            Console.WriteLine($"Someone connected: {client.RemoteEndPoint}");

            byte[] buffer = new byte[1024];
            string receivedText = Encoding.UTF8.GetString(buffer, 0, client.Receive(buffer));
            Console.WriteLine($"Received message: {receivedText}");

            byte[] responseBytes = Encoding.UTF8.GetBytes($"YO WE GOT UR MESSAGE ({receivedText})");
            client.Send(responseBytes);

            client.Shutdown(SocketShutdown.Both);
            client.Close();
            tcpSocket.Close();

            Console.WriteLine("Server closed.");
            Console.ReadLine();
        }
	}
}

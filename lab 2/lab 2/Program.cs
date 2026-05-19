using System.Net;
using System.Net.Sockets;
using System.Text;

namespace lab_2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            Console.Write("Введіть локальний порт для прослуховування: ");
            int localPort = int.Parse(Console.ReadLine());

            Console.Write("Введіть IP-адресу віддаленого хоста: ");
            string remoteIp = Console.ReadLine();

            if (remoteIp == string.Empty) remoteIp = "127.0.0.1";

            Console.Write("Введіть порт віддаленого хоста: ");
            int remotePort = int.Parse(Console.ReadLine());

            Socket udpSocket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                string message = string.Empty;
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, localPort);
                udpSocket.Bind(localEndPoint);
                Console.WriteLine($"\n+ Сокет успішно прив'язано до порту {localPort}.");

                Thread receiveThread = new(() => ReceiveData(udpSocket))
                {
                    IsBackground = true
                };
                receiveThread.Start();

                IPEndPoint remoteEndPoint = new(IPAddress.Parse(remoteIp), remotePort);
                Console.WriteLine($"+ Відправка налаштована на {remoteIp}:{remotePort}");
                Console.WriteLine("Пишіть повідомлення та тисніть Enter. Для виходу введіть 'exit'.\n");

                while (message.Trim().ToLower() != "exit")
                {
                    message = Console.ReadLine()!;

                    if (!string.IsNullOrEmpty(message))
                    {
                        byte[] data = Encoding.UTF8.GetBytes(message);

                        udpSocket.SendTo(data, remoteEndPoint);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n! Виникла помилка: {ex.Message}");
            }
            finally
            {
                if (udpSocket != null)
                {
                    udpSocket.Close();
                    Console.WriteLine("\n- Сокет закрито. Роботу завершено.");
                }
            }
        }

        static void ReceiveData(Socket socket)
        {
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[65536];

                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    int bytesRead = socket.ReceiveFrom(buffer, ref remoteEndPoint);

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    Console.WriteLine($"\n>>> Отримано від {remoteEndPoint}: {message}");
                    Console.Write("");
                }
            }
            catch (SocketException)
            {
            }
        }
    }
}

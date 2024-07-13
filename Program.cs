using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SSHPortForwarder
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting SSH Port Forwarder...");
            StartPortForwarding("192.168.25.71", 22, "127.0.0.1", 22).Wait();
        }

        static async Task StartPortForwarding(string remoteHost, int remotePort, string localHost, int localPort)
        {
            TcpListener listener = new TcpListener(System.Net.IPAddress.Parse(localHost), localPort);
            listener.Start();
            Console.WriteLine($"Listening on {localHost}:{localPort}...");

            while (true)
            {
                var localClient = await listener.AcceptTcpClientAsync();
                _ = Task.Run(async () =>
                {
                    using (localClient)
                    {
                        var remoteClient = new TcpClient();
                        try
                        {
                            await remoteClient.ConnectAsync(remoteHost, remotePort);
                            Console.WriteLine($"Connected to {remoteHost}:{remotePort}");

                            using (var localStream = localClient.GetStream())
                            using (var remoteStream = remoteClient.GetStream())
                            {
                                var localToRemote = localStream.CopyToAsync(remoteStream);
                                var remoteToLocal = remoteStream.CopyToAsync(localStream);

                                await Task.WhenAny(localToRemote, remoteToLocal);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }
                        finally
                        {
                            remoteClient.Close();
                        }
                    }
                });
            }
        }
    }
}

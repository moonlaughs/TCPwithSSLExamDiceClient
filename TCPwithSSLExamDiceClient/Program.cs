using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace TCPwithSSLExamDiceClient
{
    class Program
    {
        private static TcpClient _clientSocket;
        private static Stream _stream;
        private static StreamWriter _writer;
        private static StreamReader _reader;

        static void Main(string[] args)
        {
            Console.WriteLine("Inseart port number: ");
            try
            {
                var portNumber = Console.ReadLine();

                string clientCertificateFile = "C:/Users/asus/Documents/school/3rd semester/Certificates/RootCA.cer";
                X509Certificate clientCertificate = new X509Certificate(clientCertificateFile, "mysecret");
                SslProtocols enabledSSLProtocols = SslProtocols.Tls;
                X509CertificateCollection certificateCollection = new X509CertificateCollection { clientCertificate };
                string serverName = "FakeServerName";

                using (_clientSocket = new TcpClient("127.0.0.1", Convert.ToInt32(portNumber)))
                {
                    Stream unsecuredStream = _clientSocket.GetStream();
                    var userCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);
                    var localCertificateCallback = new LocalCertificateSelectionCallback(CertificateSelectionCallback);
                    SslStream sslStream = new SslStream(unsecuredStream, false, userCertificateValidationCallback, localCertificateCallback);
                    sslStream.AuthenticateAsClient(serverName, certificateCollection, enabledSSLProtocols, false);
                    //breaks after that...
                    StreamReader sr = new StreamReader(sslStream);
                    StreamWriter sw = new StreamWriter(sslStream) { AutoFlush = true };
                    Console.WriteLine("Client authenticated");
                    using (_stream = _clientSocket.GetStream())
                    {
                        _writer = new StreamWriter(_stream)
                        {
                            AutoFlush = true
                        };
                        Console.WriteLine("Now you are conected to the server.");
                        Console.WriteLine();
                        Console.WriteLine("DiceRoll");
                        Console.WriteLine("Type your message: [name],[guess],[1/2]");
                        while (true)
                        {
                            Console.WriteLine("Type the message and press ENTER:");
                            string messageFromClient = Console.ReadLine();
                            _writer.WriteLine(messageFromClient);
                            _reader = new StreamReader(_stream);
                            string messageFromServer = _reader.ReadLine();
                            if (messageFromServer != null)
                            {
                                Console.WriteLine(messageFromServer);
                                Console.WriteLine();
                            }
                            else
                            {
                                Console.WriteLine("DISCONNECTED");
                                Console.WriteLine();
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Please insert correct number");
                Console.WriteLine();
            }
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate serverCertificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Console.WriteLine("Client Sender: " + sender);
            Console.WriteLine("Client server certificate : " + serverCertificate);
            Console.WriteLine("Client Policy errors: " + sslPolicyErrors);
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                Console.WriteLine("Client validation of server certificate successful.");
                return false; // true for local
            }
            Console.WriteLine("Errors in certificate validation:");
            Console.WriteLine(sslPolicyErrors);
            return true; // false for local
        }

        private static X509Certificate CertificateSelectionCallback(object sender, string targetHost, X509CertificateCollection localCollection, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return localCollection[0];
        }
    }
}


using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;

namespace App_Quiz
{
    class RMQ
    {
        public ConnectionFactory connectionFactory;
        public IConnection connection;
        public IModel channel;

        public void InitRMQConnection(string host = "cloudrmqserver.pptik.id", int port = 5672, string user = "ubliot",
         string pass = "qwerty1245", string vhost = "/mahasiswaubl")
        {
            connectionFactory = new ConnectionFactory();
            connectionFactory.HostName = host;
            connectionFactory.Port = port;
            connectionFactory.UserName = user;
            connectionFactory.Password = pass;
            connectionFactory.VirtualHost = vhost;
        }

        public void CreateRMQConnection()
        {
            connection = connectionFactory.CreateConnection();
            Console.WriteLine("Koneksi " + (connection.IsOpen ? "Berhasil!" : "Gagal!"));
        }

        public void CreateRMQChannel(string queue_name, string routingKey = "", string exchange_name = "")
        {
            if (connection.IsOpen)
            {
                channel = connection.CreateModel();
                Console.WriteLine("Channel " + (channel.IsOpen ? "Berhasil!" : "Gagal!"));
            }

            if (channel.IsOpen)
            {
                channel.QueueDeclare(queue: queue_name,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                Console.WriteLine("Queue telah dideklarasikan..");
            }
        }

        public void WaitingMessage(string queue_name)
        {
            if (channel.IsOpen)
            {
                channel.QueueDeclare(queue: queue_name,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                string tujuan, inputMsg;

                Console.WriteLine("Type 'q' to quit, 's' to send message, or any other key to start listening.");
                string cmd = Console.ReadLine();


                consumer.Received += (model, ea) =>{
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Pesan diterima: {0}", message);
                    Console.WriteLine("Type 'q' to quit, press another key to send message");
                    cmd = Console.ReadLine();
                    if (cmd == "q")
                    {
                        Disconnect();
                    }
                    else
                    {
                        Console.Write("Masukkan pesan yang akan dikirim atau 'exit' to close.\n>>");
                        Console.Write(">> tujuan: ");
                        tujuan = Console.ReadLine();
                        Console.Write(">> pesan: ");
                        inputMsg = Console.ReadLine();
                        SendMessage(tujuan, inputMsg);
                    }

                };
                channel.BasicConsume(queue: queue_name,
                                     autoAck: true,
                                     consumer: consumer);
                if (cmd == "q")
                {
                    Disconnect();
                }
                else if (cmd == "s")
                {
                    Console.Write("Masukkan pesan yang akan dikirim atau 'exit' to close.\n>>");
                    Console.Write(">> tujuan: ");
                    tujuan = Console.ReadLine();
                    Console.Write(">> pesan: ");
                    inputMsg = Console.ReadLine();
                    SendMessage(tujuan, inputMsg);
                }
            }
        }

        public void SendMessage(string tujuan, string msg = "send")
        {
            byte[] responseBytes = Encoding.UTF8.GetBytes(msg);// konversi pesan dalam bentuk string menjadi byte

            channel.BasicPublish(exchange: "",
                                    routingKey: tujuan,
                                    basicProperties: null,
                                    body: responseBytes);
            Console.WriteLine("Pesan: '" + msg + "' telah dikirim.");
        }

        public void Disconnect()
        {
            channel.Close();
            channel = null;
            Console.WriteLine("Channel ditutup!");
            if (connection.IsOpen)
            {
                connection.Close();
            }

            Console.WriteLine("Koneksi diputus!");
            connection.Dispose();
            connection = null;
        }
        public void Subcribe(string nama_queue)
        {
                        channel.QueueDeclare(queue: nama_queue,
             durable: true,
             exclusive: false,
             autoDelete: false,
             arguments: null);
                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += (model, ea) =>
                        {
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);
                            Console.WriteLine(" [x] Pesan diterima: {0}", message);
                        };
                        channel.BasicConsume(queue: nama_queue,
                        autoAck: true,
            consumer: consumer);
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            RMQ rmq = new RMQ();
            Console.WriteLine("Tekan tombol apapun untuk inisialisasi RMQ parameters.");
            Console.ReadKey();
            rmq.InitRMQConnection(); // inisialisasi parameter (secara default) untuk koneksi ke server RMQ
            Console.WriteLine("Tekan tombol apapun untuk membuka koneksi ke RMQ.");
            Console.ReadKey();
            rmq.CreateRMQConnection(); // memulai koneksi dengan RMQ
            Console.Write("Masukkan nama queue channel untuk mengirim pesan melalui RMQ.\n>> ");
            string queue_name = Console.ReadLine();
            rmq.CreateRMQChannel(queue_name);
            //rmq.Subcribe(queue_name);
            rmq.WaitingMessage(queue_name);

        }
    }


   
}

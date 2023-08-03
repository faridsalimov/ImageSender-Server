using ImageSender_Server.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageSender_Server.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public RelayCommand CreateServerCommand { get; set; }

        private BitmapImage image;
        public BitmapImage Image
        {
            get { return image; }
            set { image = value; OnPropertyChanged(); }
        }

        [Obsolete]
        public MainViewModel()
        {
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();

            CreateServerCommand = new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    var ipAddress = IPAddress.Parse(myIP);
                    var port = 27001;
                    using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        var endPoint = new IPEndPoint(ipAddress, port);
                        socket.Bind(endPoint);
                        socket.Listen(10);
                        var client = socket.Accept();
                        MessageBox.Show($"Client connected {client.RemoteEndPoint}");
                        Task.Run(() =>
                        {
                            var length = 0;
                            var bytes = new byte[300000];
                            do
                            {
                                length = client.Receive(bytes);
                                Image = LoadImage(bytes);
                                break;
                            } while (length > 0);
                        });
                    }
                });
            });
        }

        public static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }
    }
}

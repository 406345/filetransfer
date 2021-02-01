using FileTransferLib;
using FileTransferProtocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileTransferClientWPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        class FileInfo
        {
            public string Root { get; set; }
            public string FullPath { get; set; }
            public string Path { get; set; }
        }

        private const int BLOCK_SIZE = 1024 * 1; // 1M

        string[] args;
        Config config;
        bool canExit = false;
        List<FileInfo> filesToSend = new List<FileInfo>();
        Thread sendThread;
        public MainWindow()
        {
            InitializeComponent();
            this.args = Environment.GetCommandLineArgs();
            this.config = Config.Create(args[0]);

            if (args.Length <= 1)
            {
                lbFileName.Content = "No file assigned(Exit in 3 seconds)";
                pbProgress.Value = 100;
                pbProgress.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 50, 50));
                Utils.DelayAction(3000, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.Close();
                    });

                    return true;
                });

                return;
            }
            else
            {

                pbProgress.Value = 0;

                for (int i = 1; i < args.Length; i++)
                {
                    string fileOrDir = args[i];

                    if (System.IO.File.Exists(fileOrDir))
                    {
                        filesToSend.Add(new FileInfo()
                        {
                            Root = System.IO.Path.GetDirectoryName(fileOrDir),
                            Path = System.IO.Path.GetFileName(fileOrDir),
                            FullPath = fileOrDir
                        });
                    }
                    else if (System.IO.Directory.Exists(fileOrDir))
                    {
                        fileOrDir = fileOrDir + System.IO.Path.DirectorySeparatorChar;
                        var dir = new System.IO.DirectoryInfo(fileOrDir);
                        var files = System.IO.Directory.GetFiles(fileOrDir, "*", System.IO.SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            filesToSend.Add(new FileInfo()
                            {
                                Root = fileOrDir,
                                Path = System.IO.Path.Combine(dir.Name, file.Replace(fileOrDir, "")),
                                FullPath = file
                            });
                        }
                    }
                }


                lbFileName.Content = filesToSend.Count.ToString();
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            sendThread = new Thread(() =>
            {
                foreach (var file in filesToSend)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.pbProgress.IsIndeterminate = false;
                        this.lbFileName.Content = file.FullPath;
                    });
                    this.SendFile(file);
                }

                Process.GetCurrentProcess().Kill();
            });
            sendThread.Start();
        }

        private void SendFile(FileInfo file)
        {
            int count = 0;
            while (count < 3)
            {
                try
                {
                    Socket sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sck.Connect(new IPEndPoint(IPAddress.Parse(config.remote), config.port));
                    SocketEx socket = new SocketEx(sck);

                    System.IO.FileInfo fi = new System.IO.FileInfo(file.FullPath);
                    FileMeta meta = new FileMeta();
                    meta.version = 1;
                    meta.blockSize = BLOCK_SIZE;
                    meta.fileName = file.Path;
                    meta.fileBlockCount = (ulong)((fi.Length + BLOCK_SIZE - 1) / BLOCK_SIZE);
                    meta.fileSize = (ulong)fi.Length;
                    meta.hash = FileTransferLib.Utils.MD5BlockBytes(this.config.password + "|" + meta.fileName);
                    int sent = socket.SendMessage(meta);
                    var metaFlag = socket.ReadMessage<FileResult>(new FileResult());

                    if (metaFlag == null)
                    {
                        ++count;
                        continue;
                    }

                    var fs = System.IO.File.OpenRead(file.FullPath);
                    byte[] buffer = new byte[BLOCK_SIZE];
                    FileData fd = new FileData();

                    for (ulong i = 0; i < meta.fileBlockCount; i++)
                    {
                        int reads = fs.Read(buffer, 0, BLOCK_SIZE);

                        fd.blockId = (uint)i;
                        fd.blockSize = (uint)reads;
                        fd.buffer = buffer;
                        fd.hash = FileTransferLib.Utils.MD5BlockBytes(fd.buffer);
                        socket.SendMessage(fd);

                        this.Dispatcher.Invoke(() =>
                        {
                            this.pbProgress.Value = (double)((i * 100) / meta.fileBlockCount);
                        });

                        var blockResult = socket.ReadMessage<FileResult>(new FileResult());

                        if (blockResult == null)
                        {
                            break;
                        }
                    }

                    var result = socket.ReadMessage<FileResult>(new FileResult());

                    if(result == null)
                    {
                        continue;
                    }

                    return;
                }
                catch (Exception e)
                {
                    count++;
                    if (count == 3)
                        MessageBox.Show(String.Format("File: {0}\nError: {1}", file.FullPath, e.Message), "Send file to remote error ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (sendThread != null)
            {
                sendThread.Abort();
                sendThread = null;
            }
        }
    }
}

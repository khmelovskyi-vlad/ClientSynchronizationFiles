using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientSynchronizationFiles
{
    class Connection
    {
        public Connection()
        {

        }

        private const string ip2 = "192.168.1.240";
        private const int port = 2048;
        private Socket tcpSocket;
        private string pathToFolder = "D:\\temp\\ClientDirectory";

        private byte[] buffer;
        private StringBuilder data;
        List<FileStruct> filesPathsAndTimeCreateOrChangeFiles = new List<FileStruct>();

        public void Client()
        {
            var tcpEndPoint = new IPEndPoint(IPAddress.Parse(ip2), port);
            using (tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    tcpSocket.Connect(tcpEndPoint);
                    AddFilesAndThemTime();
                    NewFiles newFiles = new NewFiles(filesPathsAndTimeCreateOrChangeFiles, tcpSocket);
                    newFiles.FirstCheckFiles();
                    while (true)
                    {
                        ChangeFiles();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        private void ChangeFiles()
        {
            AnswerServer();
            if (data.ToString() == "delete")
            {
                SendMessage("?");
                AnswerServer();
                var filesPaths = Split();
                SendMessage("?");
                DeleterFiles(filesPaths);
            }
            else if (data.ToString() == "new")
            {
                SendMessage("?");
                AnswerServer();
                var filesPaths = Split();
                SendMessage("?");
                AddNewOrChangeFiles(filesPaths, false);
            }
            else if (data.ToString() == "change")
            {
                SendMessage("?");
                AnswerServer();
                var filesPaths = Split();
                SendMessage("?"); // add DeleterFiles(filesPaths);
                AddNewOrChangeFiles(filesPaths, true);
            }
        }
        private void AddNewOrChangeFiles(string[] filesPaths, bool change)
        {
            if (change)
            {
                DeleterFiles(filesPaths); // delete DeleterFiles(filesPaths);
            }
            for (int i = 0; i < filesPaths.Length - 1; i++)
            {
                AnswerServer();
                if (data.ToString() == "?")
                {
                    File.WriteAllText(filesPaths[i], "");
                }
                else
                {
                    File.WriteAllText(filesPaths[i], data.ToString());
                }
                SendMessage("?");
            }
        }
        private void DeleterFiles(string[] filesPaths)
        {
            for (int i = 0; i < filesPaths.Length - 1; i++)
            {
                File.Delete(filesPaths[i]);
            }
        }
        private string[] Split()
        {
            return data.ToString().Split('?');
        }
        private void AddFilesAndThemTime()
        {
            var filesPaths = new List<string>();
            filesPaths.AddRange(Directory.GetFiles(pathToFolder));
            for (int i = 0; i < filesPaths.Count; i++)
            {
                FileStruct file = new FileStruct();
                file.filePath = filesPaths[i];
                file.timeCreateOrChangeFile = File.GetLastWriteTime(filesPaths[i]);
                filesPathsAndTimeCreateOrChangeFiles.Add(file);
            }
        }
        private void AnswerServer()
        {
            buffer = new byte[256];
            data = new StringBuilder();
            do
            {
                var size = tcpSocket.Receive(buffer);
                data.Append(Encoding.ASCII.GetString(buffer, 0, size));
            } while (tcpSocket.Available > 0);
        }
        private void SendMessage(string message)
        {
            tcpSocket.Send(Encoding.ASCII.GetBytes(message));
        }
    }
}

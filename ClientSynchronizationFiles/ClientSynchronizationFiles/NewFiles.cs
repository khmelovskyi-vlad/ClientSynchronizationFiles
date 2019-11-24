using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientSynchronizationFiles
{
    class NewFiles
    {
        public NewFiles(List<FileStruct> filesPathsAndTimeCreateOrChangeFiles, Socket tcpSocket)
        {
            this.filesPathsAndTimeCreateOrChangeFiles = filesPathsAndTimeCreateOrChangeFiles;
            this.tcpSocket = tcpSocket;
        }

        private Socket tcpSocket;
        private string pathToFolder = "D:\\temp\\ClientDirectory";

        private byte[] buffer;
        private StringBuilder data;
        List<FileStruct> filesPathsAndTimeCreateOrChangeFiles = new List<FileStruct>(); // delete  = new List<FileStruct>()

        public void FirstCheckFiles()
        {
            AnswerServer();
            ReciveNewFiles();
        }
        private void ReciveNewFiles()
        {
            var newFilesCount = FindNewFiles();
            for (int i = 0; i < newFilesCount.Count; i++)
            {
                AnswerServer();
                File.Delete(newFilesCount[i]);
                if (data.ToString() != "?")
                {
                    File.AppendAllText(newFilesCount[i], data.ToString());
                }
                else
                {
                    File.AppendAllText(newFilesCount[i], "");
                }
                SendMessage("?");
            }
        }
        private List<string> FindNewFiles()
        {
            var (filesPathsOnServer, listTimeCreateOrChangeFilesOnServer) = Split();
            var newFiles = new List<string>();
            for (int i = 0; i < filesPathsOnServer.Length; i++)
            {
                var haveFile = false;
                for (int j = 0; j < filesPathsAndTimeCreateOrChangeFiles.Count; j++)
                {
                    if (filesPathsOnServer[i] == filesPathsAndTimeCreateOrChangeFiles[j].filePath)
                    {
                        if (listTimeCreateOrChangeFilesOnServer[i] <= filesPathsAndTimeCreateOrChangeFiles[j].timeCreateOrChangeFile)
                        {
                            haveFile = true;
                        }
                        continue;
                    }
                }
                if (!haveFile)
                {
                    AddFilesAndThemTimeToList(filesPathsOnServer[i]);
                    newFiles.Add(filesPathsOnServer[i]);
                }
            }
            var newFileInStringBuilder = new StringBuilder();
            foreach (var newFile in newFiles)
            {
                newFileInStringBuilder.Append($"{newFile}?");
            }
            if (newFileInStringBuilder.Length != 0)
            {
                SendMessage(newFileInStringBuilder.ToString());
            }
            else
            {
                SendMessage("?");
            }
            return newFiles;
        }
        private void AddFilesAndThemTimeToList(string filePath)
        {
            FileStruct file = new FileStruct();
            file.filePath = filePath;
            file.timeCreateOrChangeFile = File.GetLastWriteTime(filePath);
            filesPathsAndTimeCreateOrChangeFiles.Add(file);
        }
        private (string[] filesPathsOnServer, DateTime[] listTimeCreateOrChangeFilesOnServer) Split()
        {
            var filesPathsOnServerStringArray = data.ToString().Split('?');
            var listTimeCreateOrChangeFilesOnServerArray = filesPathsOnServerStringArray[filesPathsOnServerStringArray.Length - 1].Split('*');
            Array.Resize(ref filesPathsOnServerStringArray, filesPathsOnServerStringArray.Length - 1);
            var filesPathsOnServerArray = new DateTime[listTimeCreateOrChangeFilesOnServerArray.Length - 1];
            for (int i = 0; i < filesPathsOnServerArray.Length; i++)
            {
                filesPathsOnServerArray[i] = Convert.ToDateTime(listTimeCreateOrChangeFilesOnServerArray[i]);
            }
            return (filesPathsOnServerStringArray, filesPathsOnServerArray);
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

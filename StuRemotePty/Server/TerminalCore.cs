using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Pty.Net;
namespace StuRemotePty
{
    public class TerminalCore
    {
        IPtyConnection PtyConnection { get; set; }
        string? commandFileName;
        ActionBlock<string> OutSteam { get; }
        PtyOptions ptyOptions;
        public TerminalCore(ActionBlock<string> outSteam, string? commandFileName = "")
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            this.commandFileName = commandFileName;
            OutSteam = outSteam;
            CreatPtyConnection();
        }

        public async void CreatPtyConnection()
        {
            SetTerminalName();

            PtyConnection = await PtyProvider.SpawnAsync(ptyOptions, CancellationToken.None);
            OnData();
        }

        public void InputChat(char c)
        {
            PtyConnection.WriterStream.WriteByte((byte)c);
        }

        private void SetTerminalName()
        {
            if (!string.IsNullOrEmpty(commandFileName))
            {
                return;
            }

            ptyOptions = new PtyOptions()
            {
                Name = "StuRemoteServer",
                Rows = 100,
                Cols = 100,
                Cwd = Environment.CurrentDirectory,
                //ForceWinPty = true,
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                commandFileName = "cmd";
                ptyOptions.ForceWinPty = true;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                commandFileName = "/bin/bash";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                commandFileName = "shell.sh";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                commandFileName = "/bin/bash";
            else
                throw new Exception("os platform not support");

            ptyOptions.App = commandFileName;
        }

        private async void OnData()
        {
            while (true)
            {
                var buff = new byte[1024];
                int count = await PtyConnection.ReaderStream.ReadAsync(buff, 0, 1024, new CancellationTokenSource(1000).Token);
                var data = Encoding.UTF8.GetString(buff, 0, count);
                OutSteam.Post(data);
            }
        }
    }
}

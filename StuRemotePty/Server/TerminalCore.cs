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
        CancellationTokenSource CancellationToken { get; set; }
        bool IsExit;
        public TerminalCore(ActionBlock<string> outSteam, string? commandFileName = "")
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            this.commandFileName = commandFileName;
            OutSteam = outSteam;

            CancellationToken = new CancellationTokenSource();
            CreatPtyConnection().Wait();
        }

        public async Task CreatPtyConnection()
        {
            SetTerminalName();

            PtyConnection = await PtyProvider.SpawnAsync(ptyOptions, CancellationToken.Token);
            IsExit = false;
            CancellationToken.TryReset();
            OnData();
            PtyConnection.ProcessExited += OnProcessExited;
        }

        public async void InputChat(char c)
        {
            if (IsExit)
                await CreatPtyConnection();

            PtyConnection.WriterStream.WriteByte((byte)c);
        }

        public void ShutDown()
        {
            CancellationToken.Cancel();

            PtyConnection.Kill();
            PtyConnection.WaitForExit(0);
            PtyConnection.Dispose();
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

        private void OnData()
        {
            Task.Run(async () =>
            {
                while (!CancellationToken.IsCancellationRequested)
                {
                    var buff = new byte[1024];
                    int count = await PtyConnection.ReaderStream.ReadAsync(buff, 0, 1024, CancellationToken.Token);
                    var data = Encoding.UTF8.GetString(buff, 0, count);
                    OutSteam.Post(data);
                    //int count = PtyConnection.ReaderStream.ReadByte();
                    //Console.Write((char)count);

                }
            });
        }

        private void OnProcessExited(object? sender, PtyExitedEventArgs e)
        {
            IsExit = true;
            CancellationToken.Cancel();
        }
    }
}

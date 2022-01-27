using Grpc.Core;
using StuRemotePty.Client;
using StuRemotePtyMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static StuRemotePtyMessage.RemotePtyService;

namespace StuRemotePty
{
    public class StuRemoteClient
    {
        private Channel channel;
        Task endTask;
        IConsoInputContent consoInputContent;
        public StuRemoteClient(string iP, int port)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            IP = iP;
            Port = port;

            channel = new Channel(IP, Port, channelCredentials);
            TaskCompletionSource = new CancellationTokenSource();

            InitConsoInputContent();
        }

        private void InitConsoInputContent()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                consoInputContent = new ConsoInputContentWindows();
                consoInputContent.Init();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                consoInputContent = new ConsoInputContentLinux();
                consoInputContent.Init();
            }
            else
            {
                consoInputContent = new ConsoInputContentDefault();
            }
        }

        public void Start()
        {
            endTask = Task.Run(() =>
             {
                 Connct();
             });
        }

        public string IP { get; init; }
        public int Port { get; init; }
        public ChannelCredentials? channelCredentials { get; init; } = ChannelCredentials.Insecure;

        private CancellationTokenSource TaskCompletionSource;
        public async Task Shutdowm()
        {
            TaskCompletionSource.Cancel();
            await endTask;
        }

        private async void Connct()
        {
            while (!TaskCompletionSource.IsCancellationRequested)
            {
                try
                {
                    RemotePtyServiceClient remotePtyServiceClient = new RemotePtyServiceClient(channel);
                    using var client = remotePtyServiceClient.CustomInteraction();
                    _ = Task.Run(async () =>
                      {
                          while (await client.ResponseStream.MoveNext())
                          {
                              var data = client.ResponseStream.Current;
                              Console.Write(data.Content);
                          }
                      });

                    while (!TaskCompletionSource.IsCancellationRequested)
                    {
                        int ch = consoInputContent.Read();
                        await client.RequestStream.WriteAsync(new SteamChar() { Key = ch });
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                Console.WriteLine("will reconnect");
            }
        }



    }

}

using Grpc.Core;
using StuRemotePtyMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StuRemotePty
{
    public class StuRemoteServer
    {
        private Server server;
        ServerCredentials? ServerCredentials { get; init; }
        int Port { get; init; }
        string BingIp { get; init; }
        string? CommandFile { get; init; }
        public ServerServiceDefinition[] serverServiceDefinitions;
        public StuRemoteServer(int port, string bingIp = "0.0.0.0", ServerCredentials? serverCredentials = null)
        {
            Port = port;
            BingIp = bingIp;
            ServerCredentials = serverCredentials;

            CreateServer();
        }

        public async Task Shutdown()
        {
            await server.ShutdownAsync();
        }

        private void CreateServer()
        {
            server = new Server()
            {
                Ports = { new ServerPort(BingIp, Port, ServerCredentials ?? ServerCredentials.Insecure) }
            };

            if (serverServiceDefinitions == null)
            {
                // 使用默认
                server.Services.Add(RemotePtyService.BindService(new OnRemotePtyService(CommandFile)));
            }
            else
            {
                Array.ForEach(serverServiceDefinitions, e =>
                {
                    server.Services.Add(e);
                });
            }

            server.Start();
        }

        

    }
}



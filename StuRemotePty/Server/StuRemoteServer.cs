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
        string? CommandFile { get; init; }
        public StuRemoteServer(int port, ServerCredentials? serverCredentials = null)
        {
            Port = port;
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
                Services =
                {
                     RemotePtyService.BindService(new OnRemotePtyService(CommandFile)),
                },
                Ports = { new ServerPort("0.0.0.0", Port, ServerCredentials ?? ServerCredentials.Insecure) }
            };

            server.Start();
        }


    }
}



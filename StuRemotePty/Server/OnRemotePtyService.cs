using Grpc.Core;
using StuRemotePtyMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using static StuRemotePtyMessage.RemotePtyService;

namespace StuRemotePty
{
    public class OnRemotePtyService : RemotePtyServiceBase
    {
        string? commandFileName;
        public OnRemotePtyService(string? commandFileName)
        {
            this.commandFileName = commandFileName;
        }

        public override async Task CustomInteraction(IAsyncStreamReader<SteamChar> requestStream, IServerStreamWriter<ReplayData> responseStream, ServerCallContext context)
        {
            TerminalCore terminalCore = null;
            ActionBlock<string> ResponseString;
            try
            {
                Console.WriteLine($"{context.Host} connet");

                ResponseString = new ActionBlock<string>((data) =>
               {
                   if (context.CancellationToken.IsCancellationRequested)
                   {
                       return;
                   }

                   responseStream.WriteAsync(new ReplayData() { Content = data }).Wait();
               });

                terminalCore = new TerminalCore(ResponseString, commandFileName);
                while (await requestStream.MoveNext())
                {
                    SteamChar content = requestStream.Current;
                    terminalCore.InputChat((char)content.Key);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                terminalCore?.ShutDown();
            }

        }
    }
}

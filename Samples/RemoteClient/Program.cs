// See https://aka.ms/new-console-template for more information
using StuRemotePty;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Vanara.PInvoke;

AutoResetEvent AutoResetEvent = new AutoResetEvent(false);
if (args.Length <= 0/* && Debugger.IsAttached*/)
{
    args = new string[1]
    {
        "127.0.0.1:20000",
    };
}

if (args.Length <= 0)
{
    Console.WriteLine("RemoteClient IP:Port");
    Environment.Exit(0);
}


StuRemoteClient stuRemoteServer = new StuRemoteClient(args[0]);
stuRemoteServer.Start();

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    AutoResetEvent.Set();
};

AutoResetEvent.WaitOne();
await stuRemoteServer.Shutdown();
Console.WriteLine("Shutdown success");
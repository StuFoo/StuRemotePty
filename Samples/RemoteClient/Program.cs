// See https://aka.ms/new-console-template for more information
using StuRemotePty;

StuRemoteClient stuRemoteServer = new StuRemoteClient("127.0.0.1", 20000);
stuRemoteServer.Start();


Console.WriteLine("Input A end process");
while (true) ;

await stuRemoteServer.Shutdowm();
Console.WriteLine("End");
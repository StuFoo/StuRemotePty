// See https://aka.ms/new-console-template for more information
using StuRemotePty;

StuRemoteServer clent = new(20000);

Console.WriteLine("Input A end process");
while (Console.ReadKey().KeyChar != 'A') ;

clent.Shutdown().Wait();
Console.WriteLine("End");
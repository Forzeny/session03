using System;

namespace ChatServer
{
  internal class Program
  {
    public static void Main(string[] args)
    {
      Console.WriteLine("====================================");
      var server = new ChatCoreTest.ChatServer();
      server.Bind(4099);
      server.Start();
    }
  }
}

using Server;
using System;

namespace Projekat;

public class Program
{
    static void Main(string[] args)
    {
        WebServer server = new();

        Thread serverNit = new Thread(async () =>
        {
            await server.StartAsync();
        });
        serverNit.Start();

        Console.ReadLine();
    }
}
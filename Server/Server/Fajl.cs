using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server;

internal class Fajl
{
    public string Response { get; }
    public DateTime RokTrajanja { get; }

    public Fajl(string response, DateTime rok)
    {
        Response = response;
        RokTrajanja = rok;
    }
}
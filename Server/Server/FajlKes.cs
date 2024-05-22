using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server;

internal class FajlKes
{
    private readonly int maxVelicina;
    private readonly TimeSpan timeToLive;
    private readonly ConcurrentDictionary<string, Fajl> kes = new();
    private readonly LinkedList<string> kljucevi = new();
    private readonly SemaphoreSlim semafor = new(1, 1);

    public FajlKes(int maxVelicina, TimeSpan timeToLive)
    {
        this.maxVelicina = maxVelicina;
        this.timeToLive = timeToLive;
    }

    public async Task<Tuple<bool,string>> TryGetAsync(string putanja)
    {
        string response;
        if(kes.TryGetValue(putanja, out Fajl fajl))
        {
            if(fajl.RokTrajanja < DateTime.Now)
            {
                await IzbaciAsync(putanja);
                response = default;
                return new Tuple<bool, string>(false, response);
            }

            await semafor.WaitAsync();
            try
            {
                kljucevi.Remove(putanja);
                kljucevi.AddLast(putanja);
            }
            finally
            {
                semafor.Release();
            }

            response = fajl.Response;
            return new Tuple<bool, string>(true, response);
        }

        response = default;
        return new Tuple<bool, string>(false, response);    
    }

    public async Task DodajIliAzurirajAsync(string putanja, string response)
    {
        DateTime rokTrajanja = DateTime.Now.Add(timeToLive);
        Fajl noviFajl = new Fajl(response, rokTrajanja);

        kes[putanja] = noviFajl;

        await semafor.WaitAsync();
        try
        {
            kljucevi.AddLast(putanja);

            if(kljucevi.Count > maxVelicina)
            {
                string najstarijiKljuc = kljucevi.First.Value;
                kljucevi.RemoveFirst();
                kes.TryRemove(najstarijiKljuc, out _);
            }
        }
        finally
        {
            semafor.Release();
        }
    }

    private async Task IzbaciAsync(string putanja)
    {
        kes.TryRemove(putanja, out _);

        await semafor.WaitAsync();
        try
        {
            kljucevi.Remove(putanja);   
        }
        finally
        {
            semafor.Release();
        }
    }
}

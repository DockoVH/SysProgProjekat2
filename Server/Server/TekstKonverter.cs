using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server;

internal class TekstKonverter
{
    public static async Task<string> BinarniUTekstualni(string putanjaDoBinarnog, string putanjaDoTekstualnog)
    {
        byte[] binSadrzaj = await File.ReadAllBytesAsync(putanjaDoBinarnog);
        string textSadrzaj = Encoding.UTF8.GetString(binSadrzaj);

        await File.WriteAllTextAsync(putanjaDoTekstualnog, textSadrzaj, Encoding.UTF8);
        Console.WriteLine($"Binarni fajl {putanjaDoBinarnog} konvertovan u tekstualni fajl {putanjaDoTekstualnog}.");

        return textSadrzaj;
    }

    public static async Task<string> TekstualniUBinarni(string putanjaDoTekstualnog, string putanjaDoBinarnog)
    {
        string textSadrzaj = await File.ReadAllTextAsync(putanjaDoTekstualnog, Encoding.UTF8);
        byte[] binSadrzaj = Encoding.UTF8.GetBytes(textSadrzaj);

        await File.WriteAllBytesAsync(putanjaDoBinarnog, binSadrzaj);
        Console.WriteLine($"Tekstualni fajl {putanjaDoTekstualnog} konvertovan u binarni fajl {putanjaDoBinarnog}.");

        return Convert.ToBase64String(binSadrzaj);
    }
}

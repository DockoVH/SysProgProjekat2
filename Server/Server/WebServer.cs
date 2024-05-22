using System.Diagnostics;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace Server;

internal class WebServer
{
    private HttpListener listener = new();
    private const int port = 5050;
    private readonly string[] prefix = [$"http://localhost:{port}/", $"http://127.0.0.1:{port}/"];
    private readonly FajlKes kes = new(100, TimeSpan.FromSeconds(180));
    private readonly SemaphoreSlim semafor = new(4, 50);

    public WebServer()
    {
        foreach (var pr in prefix)
        {
            listener.Prefixes.Add(pr);
        }
    }

    public async Task StartAsync()
    {
        ThreadPool.SetMaxThreads(4, 50);

        try
        {
            listener.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Greška prilikom pokretanja servera: {ex.Message}");
            return;
        }

        Console.WriteLine($"Server pokrenut:\n{String.Join("\n", prefix)}");

        while(true)
        {
            HttpListenerContext context = await listener.GetContextAsync();
            await semafor.WaitAsync();
            _ = ObradiZahtevAsync(context);
        }
    }

    private async Task ObradiZahtevAsync(HttpListenerContext context)
    {
        try
        {
            await ObradiZahtevPomAsync(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Greška: ${ex.Message}");
        }
        finally
        {
            semafor.Release();
        }
    }

    private async Task ObradiZahtevPomAsync(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;
        Stopwatch sat = new();

        sat.Start();
        string putanja = request.Url!.LocalPath.TrimStart('/');

        if(request.Url.LocalPath == "/fajlovi/txt")
        {
            List<string> txtFajlovi = await ListaFajlova.TxtFajloviAsync(AppDomain.CurrentDomain.BaseDirectory);
            string jsonOdg = JsonConvert.SerializeObject(txtFajlovi);
            await PosaljiOdgovorAsync(response, jsonOdg);
            sat.Stop();
            Console.WriteLine($"Nit : {Thread.CurrentThread.ManagedThreadId}: Zahtev sa adrese {request.UserHostAddress} obradjen za: {sat.Elapsed.TotalMilliseconds}ms.");
            return;
        }
        else if(request.Url.LocalPath == "/fajlovi/bin")
        {
            List<string> binFajlovi = await ListaFajlova.BinFajloviAsync(AppDomain.CurrentDomain.BaseDirectory);
            string jsonOdg = JsonConvert.SerializeObject(binFajlovi);
            await PosaljiOdgovorAsync(response, jsonOdg);
            sat.Stop();
            Console.WriteLine($"Nit {Thread.CurrentThread.ManagedThreadId}: Zahtev sa adrese {request.UserHostAddress} obradjen za: {sat.Elapsed.TotalMilliseconds}ms.");
            return;
        }

        Tuple<bool, string> rez = await kes.TryGetAsync(putanja);

        if (rez.Item1)
        {
            sat.Stop();
            Console.WriteLine($"Nit {Thread.CurrentThread.ManagedThreadId}: Vraćen keširani odgovor za: {putanja} za {sat.Elapsed.TotalMilliseconds}ms.");
            await PosaljiOdgovorAsync(response, rez.Item2);
            return;
        }

        try
        {
            string punaPutanja = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, putanja);
            string tekst;

            if (!File.Exists(punaPutanja))
            {
                Console.WriteLine($"Nit {Thread.CurrentThread.ManagedThreadId}: Fajl ne postoji.");
                response.StatusCode = (int)HttpStatusCode.NotFound;
                await PosaljiOdgovorAsync(response, "Fajl ne postoji");
                return;
            }

            string ext = Path.GetExtension(punaPutanja);
            if (ext.Equals(".txt", StringComparison.OrdinalIgnoreCase))
            {
                string binPutanja = $"{punaPutanja.Remove(punaPutanja.IndexOf(".txt"))}Bin.bin";
                tekst = await TekstKonverter.TekstualniUBinarni(punaPutanja, binPutanja);
                tekst = JsonConvert.SerializeObject(tekst);
            }
            else
            {
                string txtPutanja = $"{punaPutanja.Remove(punaPutanja.IndexOf("Bin.bin"))}.txt";
                tekst = await TekstKonverter.BinarniUTekstualni(punaPutanja, txtPutanja);
                tekst = JsonConvert.SerializeObject(tekst);
            }

            await kes.DodajIliAzurirajAsync(putanja, tekst);
            await PosaljiOdgovorAsync(response, tekst);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Nit {Thread.CurrentThread.ManagedThreadId}: Greška prilikom obrade zahteva: {ex.Message}");
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await PosaljiOdgovorAsync(response, "Greška prilikom obrade zahteva");
        }
        finally
        {
            sat.Stop();
            Console.WriteLine($"Nit {Thread.CurrentThread.ManagedThreadId}: Zahtev sa adrese {request.UserHostAddress} obradjen za: {sat.Elapsed.TotalMilliseconds}ms.");
            response.Close();
        }
    }

    private async Task PosaljiOdgovorAsync(HttpListenerResponse response, string body)
    {
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.ContentType = "application/json";
        byte[] buff = Encoding.UTF8.GetBytes(body);
        response.ContentLength64 = buff.Length;
        await response.OutputStream.WriteAsync(buff, 0, buff.Length);
    }
}

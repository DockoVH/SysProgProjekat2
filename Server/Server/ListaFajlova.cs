using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class ListaFajlova
    {

        public static Task<List<string>> TxtFajloviAsync(string rootDir)
        {
            return NadjiFajlove(rootDir, "txt");
        }

        public static Task<List<string>> BinFajloviAsync(string rootDir)
        {
            return NadjiFajlove(rootDir, "bin");
        }

        private static async Task<List<string>> NadjiFajlove(string rootDir, string ekstenzija)
        {
            List<string> punePutanje = Directory.GetFiles(rootDir, $"*.{ekstenzija}").ToList();
            ConcurrentBag<string> imenaFajlova = new();

            await Task.Run(() =>
            {
                Parallel.ForEach(punePutanje, fajl =>
                {
                    imenaFajlova.Add(Path.GetFileName(fajl));
                });
            });

            return imenaFajlova.ToList();
        }
    }
}

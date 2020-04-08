using System;
using System.IO;
using Newtonsoft.Json;

namespace BeToJE
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string data = File.ReadAllText("states.json");
            BlockMap[] map = JsonConvert.DeserializeObject<BlockMap[]>(data);
            
            BlockMapConverter c = new BlockMapConverter(map);
            c.Convert();
            
            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
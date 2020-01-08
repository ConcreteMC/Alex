using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace BeToJE
{
    public class BlockMapConverter
    {
        private BlockMap[] Map { get; }
        public BlockMapConverter(BlockMap[] map)
        {
            Map = map;
        }

        public void Convert()
        {
            Dictionary<string, Unique> uniqueStateTypes = new Dictionary<string, Unique>();
            
            foreach (var item in Map)
            {
                foreach (var i in item.States)
                {
                    if (!uniqueStateTypes.ContainsKey(i.Name))
                    {
                        var u = new Unique();
                        u.Type = i.Type;
                        
                        if (!u.Values.Contains(i.Value))
                            u.Values.Add(i.Value);
                        
                        if (!u.FoundInBlocks.Contains(item.Name))
                            u.FoundInBlocks.Add(item.Name);
                        
                        uniqueStateTypes.Add(i.Name, u);
                    }
                    else
                    {
                        var u = uniqueStateTypes[i.Name];
                        if (!u.Values.Contains(i.Value))
                            u.Values.Add(i.Value);
                        
                        if (!u.FoundInBlocks.Contains(item.Name))
                            u.FoundInBlocks.Add(item.Name);
                    }
                }
            }
            
            File.WriteAllText("result.json", JsonConvert.SerializeObject(uniqueStateTypes, Formatting.Indented));
        }

        public class Unique
        {
            public long Type { get; set; }
            public List<string> FoundInBlocks { get; set; } = new List<string>();
            public List<string> Values { get; set; } = new List<string>();
        }
    }
}
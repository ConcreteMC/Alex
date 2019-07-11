using System;
using System.IO;
using System.Threading;
using Alex.Worlds.Bedrock;

namespace ChunkPerformance
{
    public class Testing
    {
        private ChunkProcessor _chunkProcessor { get; set; }
        // private Queue<byte[]> _
        private byte[] data;

        public Testing()
        {
            _chunkProcessor = new ChunkProcessor(4, false, CancellationToken.None);
            
            Random rnd = new Random();
            var files = Directory.GetFiles("samplechunkdata");
            var file = files[rnd.Next() % files.Length - 1];

            data = File.ReadAllBytes(file);
        }


        public void Run()
        {
            for (int i = 0; i < 1000; i++)
            {
                _chunkProcessor.HandleChunkData(data, 0, 0, column => { });
            }
        }
    }
}
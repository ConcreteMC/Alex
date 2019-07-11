using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Alex.Worlds.Bedrock;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [CoreJob(true)]
    [RPlotExporter, RankColumn]
    public class ChunkMeshBuilding
    {
        private ChunkProcessor _chunkProcessor { get; set; }
       // private Queue<byte[]> _
       private byte[] data;
        [GlobalSetup]
        public void Setup()
        {
            _chunkProcessor = new ChunkProcessor(4, false, CancellationToken.None);
            
            Random rnd = new Random();
            var files = Directory.GetFiles("samplechunkdata");
            var file = files[rnd.Next() % files.Length - 1];

            data = File.ReadAllBytes(file);
        }

        [IterationSetup]
        public void IterationSetup()
        {
            
        }
        
        [Benchmark]
        public void Process()
        {
            _chunkProcessor.HandleChunkData(data, 0, 0, column => { });
        }
    }
}
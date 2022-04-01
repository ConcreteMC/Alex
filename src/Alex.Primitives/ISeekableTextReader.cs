using System;
using System.Threading;
using System.Threading.Tasks;

namespace Alex.Interfaces
{
    public interface ISeekableTextReader
    {
        /// <inheritdoc />
        int Read();

        int Read(char[] buffer, int index, int count);
        int Read(Span<char> buffer);

        /// <inheritdoc />
        int Peek();

        int Position { get; set; }
        int Length { get; }
        int ReadUntil(char c, out string result);
        int ReadSingleWord(out string result);
        int ReadQuoted(out string result);
        Task<int> ReadAsync(char[] buffer, int index, int count);
        ValueTask<int> ReadAsync(Memory<char> buffer, CancellationToken cancellationToken);
        int ReadBlock(char[] buffer, int index, int count);
        int ReadBlock(Span<char> buffer);
        Task<int> ReadBlockAsync(char[] buffer, int index, int count);
        ValueTask<int> ReadBlockAsync(Memory<char> buffer, CancellationToken cancellationToken);
        string ReadLine();
        Task<string> ReadLineAsync();
        string ReadToEnd();
        Task<string> ReadToEndAsync();
    }
}
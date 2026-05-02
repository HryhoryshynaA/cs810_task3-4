using System.Diagnostics;
using System.Text;
using System.Threading;

internal static class Program
{
    private static readonly uint[] K =
    [
        0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5,
        0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
        0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3,
        0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
        0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc,
        0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
        0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7,
        0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
        0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13,
        0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
        0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3,
        0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
        0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5,
        0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
        0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208,
        0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2,
    ];

    private static readonly uint[] H0 =
    [
        0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a,
        0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19,
    ];

    private static uint RotR(uint x, int n) => (x >> n) | (x << (32 - n));
    private static uint Ch(uint x, uint y, uint z) => (x & y) ^ (~x & z);
    private static uint Maj(uint x, uint y, uint z) => (x & y) ^ (x & z) ^ (y & z);
    private static uint Big0(uint x) => RotR(x, 2) ^ RotR(x, 13) ^ RotR(x, 22);
    private static uint Big1(uint x) => RotR(x, 6) ^ RotR(x, 11) ^ RotR(x, 25);
    private static uint Small0(uint x) => RotR(x, 7) ^ RotR(x, 18) ^ (x >> 3);
    private static uint Small1(uint x) => RotR(x, 17) ^ RotR(x, 19) ^ (x >> 10);

    private static uint Pack(ReadOnlySpan<byte> bytes) =>
        ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];

    private static void Expand(uint[] w)
    {
        for (var i = 16; i < 64; i++)
            w[i] = unchecked(Small1(w[i - 2]) + w[i - 7] + Small0(w[i - 15]) + w[i - 16]);
    }

    private static void Compress(uint[] w, Span<uint> h)
    {
        var a = h[0];
        var b = h[1];
        var c = h[2];
        var d = h[3];
        var e = h[4];
        var f = h[5];
        var g = h[6];
        var hh = h[7];

        for (var i = 0; i < 64; i++)
        {
            var t1 = unchecked(hh + Big1(e) + Ch(e, f, g) + K[i] + w[i]);
            var t2 = unchecked(Big0(a) + Maj(a, b, c));
            hh = g;
            g = f;
            f = e;
            e = unchecked(d + t1);
            d = c;
            c = b;
            b = a;
            a = unchecked(t1 + t2);
        }

        h[0] = unchecked(h[0] + a);
        h[1] = unchecked(h[1] + b);
        h[2] = unchecked(h[2] + c);
        h[3] = unchecked(h[3] + d);
        h[4] = unchecked(h[4] + e);
        h[5] = unchecked(h[5] + f);
        h[6] = unchecked(h[6] + g);
        h[7] = unchecked(h[7] + hh);
    }

    private static uint FinalFirstWord(ulong counter, uint[] block0Schedule, uint[] block1Schedule)
    {
        Span<uint> h = stackalloc uint[8];
        H0.CopyTo(h);

        block0Schedule[3] = (uint)(counter >> 32);
        block0Schedule[4] = (uint)counter;
        Expand(block0Schedule);

        Compress(block0Schedule, h);
        Compress(block1Schedule, h);
        return h[0];
    }

    private static string ToHex(ReadOnlySpan<byte> bytes)
    {
        var chars = new char[bytes.Length * 2];
        const string alphabet = "0123456789abcdef";
        for (var i = 0; i < bytes.Length; i++)
        {
            chars[i * 2] = alphabet[bytes[i] >> 4];
            chars[i * 2 + 1] = alphabet[bytes[i] & 0x0f];
        }

        return new string(chars);
    }

    private static byte[] BuildPrefix(ulong counter)
    {
        var prefix = new byte[20];
        Encoding.ASCII.GetBytes("cs810-task2-".AsSpan(), prefix);
        for (var i = 0; i < 8; i++)
            prefix[12 + i] = (byte)(counter >> (56 - 8 * i));
        return prefix;
    }

    public static void Main(string[] args)
    {
        if (args.Length >= 2 && args[0] == "--check" && ulong.TryParse(args[1], out var checkCounter))
        {
            var checkPrefixBytes = Encoding.ASCII.GetBytes("cs810-task2-");
            var checkMessageBytes = Encoding.ASCII.GetBytes("give my friend 2 bitcoins for a pizza");
            var checkBlock0 = MakeBlock0(checkPrefixBytes, checkMessageBytes);
            var checkBlock1 = new uint[64];
            checkBlock1[15] = (uint)((20 + checkMessageBytes.Length) * 8);
            Expand(checkBlock1);
            Console.WriteLine($"first_word={FinalFirstWord(checkCounter, checkBlock0, checkBlock1):x8}");
            Console.WriteLine($"prefix_hex={ToHex(BuildPrefix(checkCounter))}");
            return;
        }

        var threads = args.Length > 0 && int.TryParse(args[0], out var parsedThreads)
            ? Math.Max(1, parsedThreads)
            : Environment.ProcessorCount;

        var prefixBytes = Encoding.ASCII.GetBytes("cs810-task2-");
        var messageBytes = Encoding.ASCII.GetBytes("give my friend 2 bitcoins for a pizza");

        var block1 = new uint[64];
        block1[15] = (uint)((20 + messageBytes.Length) * 8);
        Expand(block1);

        long total = 0;
        var found = 0;
        ulong foundCounter = 0;
        var sw = Stopwatch.StartNew();

        using var timer = new Timer(_ =>
        {
            var done = Interlocked.Read(ref total);
            var rate = done / Math.Max(0.001, sw.Elapsed.TotalSeconds);
            Console.Error.WriteLine($"{done:n0} hashes, {rate:n0} H/s");
        }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

        Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = threads }, threadId =>
        {
            var block0 = MakeBlock0(prefixBytes, messageBytes);

            var local = 0;
            for (var counter = (ulong)threadId; Volatile.Read(ref found) == 0; counter += (ulong)threads)
            {
                if (FinalFirstWord(counter, block0, block1) == 0)
                {
                    foundCounter = counter;
                    Volatile.Write(ref found, 1);
                    break;
                }

                if (++local == 4096)
                {
                    Interlocked.Add(ref total, local);
                    local = 0;
                }
            }

            if (local > 0)
                Interlocked.Add(ref total, local);
        });

        var prefix = BuildPrefix(foundCounter);
        Console.WriteLine($"counter={foundCounter}");
        Console.WriteLine($"prefix_hex={ToHex(prefix)}");
        Console.WriteLine("prefix_ascii_head=cs810-task2-");
    }

    private static uint[] MakeBlock0(byte[] prefixBytes, byte[] messageBytes)
    {
        var block0 = new uint[64];
        block0[0] = Pack(prefixBytes.AsSpan(0, 4));
        block0[1] = Pack(prefixBytes.AsSpan(4, 4));
        block0[2] = Pack(prefixBytes.AsSpan(8, 4));
        block0[5] = Pack(messageBytes.AsSpan(0, 4));
        block0[6] = Pack(messageBytes.AsSpan(4, 4));
        block0[7] = Pack(messageBytes.AsSpan(8, 4));
        block0[8] = Pack(messageBytes.AsSpan(12, 4));
        block0[9] = Pack(messageBytes.AsSpan(16, 4));
        block0[10] = Pack(messageBytes.AsSpan(20, 4));
        block0[11] = Pack(messageBytes.AsSpan(24, 4));
        block0[12] = Pack(messageBytes.AsSpan(28, 4));
        block0[13] = Pack(messageBytes.AsSpan(32, 4));
        block0[14] = ((uint)messageBytes[36] << 24) | 0x00800000;
        return block0;
    }
}

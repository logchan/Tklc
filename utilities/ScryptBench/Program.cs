using System;
using System.Diagnostics;
using System.Text;
using Tklc.Cryptography;
using Tklc.Framework.Extensions;

namespace ScryptBench {
    class Program {
        static void Main(string[] args) {
            var password = "this is a password";
            var salt = Encoding.UTF8.GetBytes("and this is a salt");

            var sw = new Stopwatch();

            if (args.Length == 0 || args[0] == "scrypt") {
                sw.Start();
                var key = Scrypt.Derive(password, salt, 1048576, 8, 1, 32);
                sw.Stop();
                Console.WriteLine($"Scrypt: {key.ToHexString()}");
                Console.WriteLine($"Scrypt: {sw.ElapsedMilliseconds}");
            }

            if (args.Length == 0 || args[0] == "sse") {
                sw.Restart();
                var key = ScryptSse.Derive(password, salt, 1048576, 8, 1, 32);
                sw.Stop();
                Console.WriteLine($"ScryptSse: {key.ToHexString()}");
                Console.WriteLine($"ScryptSse: {sw.ElapsedMilliseconds}");
            }
        }
    }
}

using System;
using Tklc.Drawing.Gif;

namespace GifDump
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                var gif = new GifFile(args[0]);
                Console.WriteLine(gif.ToString());
                foreach (var frame in gif.Frames)
                {
                    Console.WriteLine($"Frame: at ({frame.Top}, {frame.Left}) size [{frame.Width}, {frame.Height}], delay {frame.ControlExtension?.DelayTime}");
                }

                Console.WriteLine("Done");
                Console.ReadKey();
            }
        }
    }
}

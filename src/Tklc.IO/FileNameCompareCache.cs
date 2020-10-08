namespace Tklc.IO {
    /// <summary>
    /// Cache for accelerating file name comparison, useful when most file names are alike.
    /// (Currently unused).
    /// </summary>
    public sealed class FileNameCompareCache {
        public string LongestPrefix { get; set; }
    }
}

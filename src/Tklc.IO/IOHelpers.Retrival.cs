using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tklc.IO {
    public static partial class IOHelpers {
        /// <summary>
        /// Get files from a directory, optionally sorted and filtered.
        /// </summary>
        /// <param name="path">The directory's path</param>
        /// <param name="naturalSort">Sort in natural order</param>
        /// <param name="predicate">Filter</param>
        /// <returns>A list of files</returns>
        public static List<string> GetFiles(string path, bool naturalSort = true, Func<FileInfo, bool> predicate = null) {
            if (predicate == null) {
                predicate = f => true;
            }

            if (!Directory.Exists(path)) {
                throw new DirectoryNotFoundException(path);
            }

            var files = (from f in new DirectoryInfo(path).GetFiles()
                         where predicate(f)
                         select f.Name).ToList();
            if (naturalSort) {
                files.Sort(FileNameNaturalCompare);
            }

            return files;
        }

        /// <summary>
        /// Get a sibling of a file in a directory. Returns null if the file or the directory does not exist.
        /// </summary>
        /// <param name="fileName">The file's name</param>
        /// <param name="directory">The directory</param>
        /// <param name="delta">The distance of the sibling, may be negative</param>
        /// <param name="predicate">Filters the sibling files</param>
        /// <param name="files">Cached list of file names</param>
        public static string GetSibling(string fileName, string directory, int delta, Func<FileInfo, bool> predicate = null, List<string> files = null) {
            if (delta == 0) {
                return fileName;
            }

            if (files == null) {
                files = GetFiles(directory, predicate: predicate);
            }

            var index = files.IndexOf(fileName);
            if (index < 0 || index >= files.Count) {
                // this is really weird
                return null;
            }

            index += delta;
            if (index < 0 || index >= files.Count) {
                return null;
            }

            return files[index];
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FolderDiff
{
    enum SDOperation
    {
        None = 0,
        Add = 1,
        Edit = 2,
        Delete = 3
    }

    class Program
    {
        private static Dictionary<string, string> folder1File2MD5Mappping = new Dictionary<string, string>();
        private static Dictionary<string, string> folder2File2MD5Mappping = new Dictionary<string, string>();

        private static HashSet<string> folder1Set = new HashSet<string>();
        private static HashSet<string> folder2Set = new HashSet<string>();

        private static string folder1 = @"d:\temp\folder1";
        private static string folder2 = @"d:\temp\folder2";
        private static string outputPath = @"";

        private static void outputSD(string path, SDOperation op)
        {
            string opStr = op.ToString();

            string pathStr = $"{folder2}{path}";
            if (op == SDOperation.Delete || op == SDOperation.Edit)
            {
                pathStr = $"{folder1}{path}";
            }

            // FileInfo is not disposable
            if (new FileInfo(pathStr).Length == 0)
            {
#if DEBUG
                Console.WriteLine($"{pathStr}: size is 0, bypass");
#endif
                return;
            }

            if (!string.IsNullOrEmpty(outputPath) && File.Exists(outputPath))
            {
                using (StreamWriter sw = new StreamWriter(outputPath, true))
                {
                    sw.WriteLine($"{opStr}:{pathStr}");
                }
            }
            else
            {
                Console.WriteLine($"{opStr}:{pathStr}");
            }
        }

        static void Main(string[] args)
        {
#if DEBUG
            folder1 = @"d:\temp\folder1";
            folder2 = @"d:\temp\folder2";
            outputPath = @"";
#else
            if (args.Length < 2)
            {
                Console.Write("Usage: FolderDiff.exe folder1 folder2 [outputpath]");
                return;
            }

            folder1 = args[0];
            folder2 = args[1];
            outputPath = args.Length > 2 ? args[2] : string.Empty;
#endif

            if (!Directory.Exists(folder1))
            {
                Console.WriteLine($"Error: {folder1} does not exist");
                return;
            }

            if (!Directory.Exists(folder2))
            {
                Console.WriteLine($"Error: {folder2} does not exist");
                return;
            }

            if (File.Exists(outputPath))
            {
                try
                {
                    File.Create(outputPath).Close();
                }
                catch (DirectoryNotFoundException ex)
                {
                    Console.Write($"Error: {ex.Message}");
                    return;
                }
                catch (IOException ex)
                {
                    Console.Write($"Error: {ex.Message}");
                    return;
                }
            }

            var folder1FileList = Directory.GetFiles(folder1, "*", SearchOption.AllDirectories);
            var folder2FileList = Directory.GetFiles(folder2, "*", SearchOption.AllDirectories);

            foreach (var file in folder1FileList)
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(file))
                    {
                        // ignore case
                        string path = file.Replace(folder1, "").ToLower();
#if DEBUG
                        Console.WriteLine("[folder1] file: {0}, path: {1}", file, path);
#endif
                        folder1Set.Add(path);
                        folder1File2MD5Mappping[path] = System.Text.Encoding.Default.GetString(md5.ComputeHash(stream));
                    }
                }
            }

            foreach (var file in folder2FileList)
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(file))
                    {
                        // ignore case
                        string path = file.Replace(folder2, "").ToLower();
#if DEBUG
                        Console.WriteLine("[folder2] file: {0}, path: {1}", file, path);
#endif
                        folder2Set.Add(path);
                        folder2File2MD5Mappping[path] = System.Text.Encoding.Default.GetString(md5.ComputeHash(stream));
                    }
                }
            }

            var inFolder2AndInFolder1Set = folder2Set.Intersect(folder1Set);
            foreach (var file in inFolder2AndInFolder1Set)
            {
                if (folder1File2MD5Mappping[file] != folder2File2MD5Mappping[file])
                {
#if DEBUG
                    Console.WriteLine("Compare: [{0}], different", file);
#endif
                    outputSD(file, SDOperation.Edit);
                }
                else
                {
#if DEBUG
                    Console.WriteLine("Compare: [{0}], the same", file);
#endif
                }
            }

            var inFolder1ButNotInFolder2Set = folder1Set.Except(folder2Set);
            foreach (var file in inFolder1ButNotInFolder2Set)
            {
#if DEBUG
                Console.WriteLine("Compare: [{0}], not exist in folder2", file);
#endif
                outputSD(file, SDOperation.Delete);
            }

            var inFolder2ButNotInFolder1Set = folder2Set.Except(folder1Set);
            foreach (var file in inFolder2ButNotInFolder1Set)
            {
#if DEBUG
                Console.WriteLine("Compare: [{0}], not exist in folder1", file);
#endif
                outputSD(file, SDOperation.Add);
            }

            if (!string.IsNullOrEmpty(outputPath) && File.Exists(outputPath))
            {
                Console.Write($"Result is successfully published to {outputPath}");
            }

#if DEBUG
            Console.ReadLine();
#endif
        }
    }
}

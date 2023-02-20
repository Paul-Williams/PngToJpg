#nullable enable

using PW.IO.FileSystemObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks.Dataflow;

namespace PngToJpg {
  static class Program {
    static async System.Threading.Tasks.Task Main(string[] args) {


      // One or more command line arguments are required.
      if (args.Length == 0) {
        Log("Command line argument(s) missing. Requires path(s) to PNG file(s), a path to a directory containing PNG files, or a path to a text file containing PNG file paths.");
        return;
      }

      // If there is a single command line argument, which points to a valid directory, then convert all the PNG's within that directory.
      // Otherwise get all the command line arguments that are PNG file paths.
      var pngFilePaths = GetFilePathsFromCommandLine(args);


      if (pngFilePaths.Length == 0) {
        Log("None of the command line arguments or the supplied path contain a PNG file path.");
        return;
      }

      // File extension to use for saved Jpegs.
      var jpgExt = (FileExtension)".jpg";

      var converter = new DataFlowImageConverter(90L, Log);

      var converted = new List<FilePath>();

      foreach (var pngFilePath in pngFilePaths.ToFilePaths()) {
        if (pngFilePath.Exists) {
          // Re-create the files full path, but with a JPG extension instead of a PNG extension.
          var jpgFile = pngFilePath.ChangeExtension(jpgExt);

          // Convert the PNG only if a JPG with the same name does not already exist.
          if (!jpgFile.Exists) {
            Console.WriteLine($"Converting: {jpgFile}");
            converter.Post(pngFilePath, jpgFile);
            converted.Add(pngFilePath);
          }
          else Console.WriteLine("File already exists: " + jpgFile);
        }
        else Console.WriteLine("File not found: " + pngFilePath);

      }
      converter.Complete();
      await converter.Completion;

      // Need a command line switch for this !!
      if (converted.Count != 0) {
        try {
          converted.ForEach(FileSystem.Recycle); // TODO: This is REALLY slow when the are lots of files.
        }
        catch (Exception ex) {

          Console.WriteLine("Cannot delete file: " + ex.Message);
        }

      }
      else {
        Console.WriteLine("No files were converted.");
      }
    }

    private static string[] GetFilePathsFromCommandLine(string[] args) {

      return args.Length == 1 && args[0].EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
          ? File.ReadAllLines(args[0])
          : args.Length == 1 && Directory.Exists(args[0])
          ? Directory.GetFiles(args[0], "*.png", new EnumerationOptions() { IgnoreInaccessible = true, RecurseSubdirectories = true })  
          : args;
    }


    private static IEnumerable<FilePath> ToFilePaths(this IEnumerable<string> collection) => collection.Select(x => (FilePath)x);


    private static List<string> LogEntries { get; } = new List<string>();

    private static void Log (string entry)
    {
      LogEntries.Add(entry);
      Console.WriteLine(entry);
    }


  }
}

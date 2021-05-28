#nullable enable

using PW.IO.FileSystemObjects;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace PngToJpg
{
  static class Program
  {
    static async System.Threading.Tasks.Task Main(string[] args)
    {
      // One or more command line arguments are required.
      if (args.Length == 0)
      {
        Console.WriteLine("Command line argument(s) missing. Requires path(s) to PNG file(s) or a path to a directory containing PNG files.");
        return;
      }

      // If there is a single command line argument, which points to a valid directory, then convert all the PNG's within that directory.
      // Otherwise get all the command line arguments that are PNG file paths.
      var pngPaths = (args.Length == 1 && Directory.Exists(args[0]))
        ? Directory.GetFiles(args[0], "*.png")
        : args.Where(x => x.EndsWith(".png")).ToArray();


      if (pngPaths.Length == 0)
      {
        Console.WriteLine("None of the command line arguments or the supplied path contain a PNG file path.");
        return;
      }

      // File extension to use for saved Jpegs.
      var jpgExt = (FileExtension)".jpg";

      var converter = new DataFlowImageConverter(90L);

      foreach (var pngFile in pngPaths.Select(x => (FilePath)x))
      {
        if (pngFile.Exists)
        {
          // Re-create the files full path, but with a JPG extension instead of a PNG extension.
          var jpgFile = pngFile.ChangeExtension(jpgExt);

          // Convert the PNG only if a JPG with the same name does not already exist.
          if (!jpgFile.Exists) converter.Loader.Post(new(pngFile, jpgFile));
          else Console.WriteLine("File already exists: " + jpgFile.Value);
        }
        else Console.WriteLine("File not found: " + pngFile.Value);

      }
      converter.Loader.Complete();
      await converter.Saver.Completion;

    }
  }
}

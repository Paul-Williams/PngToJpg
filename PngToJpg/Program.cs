#nullable enable

using PW.IO.FileSystemObjects;
using System;
using System.Drawing;
using System.Linq;

namespace PngToJpg
{
  static class Program
  {
    static void Main(string[] args)
    {
      if (args.Length == 0)
      {
        Console.WriteLine("Command line argument(s) missing. Requires path(s) to PNG file(s).");
        return;
      }

      var pngPaths = args.Where(x => x.EndsWith(".png")).ToArray();

      if (pngPaths.Length == 0)
      {
        Console.WriteLine("None of the command line arguments is a PNG file path.");
        return;
      }

      // Used to save as Jpeg of specific quality.
      var codecInfo = ImageHelper.GetJpegCodecInfo();

      if (codecInfo is null)
      {
        Console.WriteLine("Unable to get Jpeg codec info.");
        return;
      }

      // Quality for saved Jpegs
      var encoderParams = ImageHelper.GetQualityEncoder(90L);

      // File extension to use for saved Jpegs.
      var jpgExt = (FileExtension)".jpg";


      foreach (var pngFile in pngPaths.Select(x => (FilePath)x))
      {
        if (pngFile.Exists)
        {
          var jpgFile = pngFile.ChangeExtension(jpgExt);

          if (!jpgFile.Exists)
          {
            using var pngImg = Image.FromFile(pngFile.Value);
            pngImg.Save(pngFile.ChangeExtension(jpgExt).Value, codecInfo, encoderParams);
          }
          else Console.WriteLine("File already exists: " + jpgFile.Value);


        }
        else Console.WriteLine("File not found: " + pngFile.Value);

      }



    }
  }
}

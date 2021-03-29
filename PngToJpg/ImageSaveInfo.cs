#nullable enable

using PW.IO.FileSystemObjects;
using System;
using System.Drawing;

namespace PngToJpg
{
  public class ImageSaveInfo
  {
    public FilePath OutputFile { get; }
    public Image Image { get; }

    public ImageSaveInfo(FilePath outputFile, Image image)
    {
      OutputFile = outputFile ?? throw new ArgumentNullException(nameof(outputFile));
      Image = image ?? throw new ArgumentNullException(nameof(image));
    }

  }

}



#nullable enable

using PW.IO.FileSystemObjects;
using System;

namespace PngToJpg
{
  public class FilePair
  {

    public FilePath InputFile { get; }
    public FilePath OutputFile { get; }

    public FilePair(FilePath inputFile, FilePath outputFile)
    {
      InputFile = inputFile ?? throw new ArgumentNullException(nameof(inputFile));
      OutputFile = outputFile ?? throw new ArgumentNullException(nameof(outputFile));
    }

  }

}



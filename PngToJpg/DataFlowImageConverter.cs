using PW.IO.FileSystemObjects;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace PngToJpg;

/// <summary>
/// Converts PNG -> JPG using DataFlowBlocks. Post items to <see cref="Loader"/> then await <see cref="Saver"/>.Completion
/// </summary>
internal class DataFlowImageConverter : IDataflowBlock
{
  static DataFlowImageConverter()
  {
    CodecInfo = ImageHelper.GetJpegCodecInfo() ?? throw new Exception("Unable to get Jpeg codec info.");
  }

  private Action<string>? Log { get; }

  private Image? TryLoadImage(FilePath image)
  {
    try
    {
      return Image.FromFile((string)image);
    }
    catch (Exception ex)
    {
      Log?.Invoke($"Error loading {image} '{ex.Message}'");
      return null;
    }
  }

  /// <summary>
  /// Creates a new instance to convert images to JPG with the specified quality.
  /// </summary>
  /// <param name="quality">Value 0 through 100</param>
  public DataFlowImageConverter(long quality, Action<string>? log)
  {
    EncoderParams = ImageHelper.GetQualityEncoder(quality);
    Log = log;

    Loader = new(x => (x.InputFile, x.OutputFile, TryLoadImage(x.InputFile)), new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1 });

    Saver = new(x =>
    {
      if (x.Image is Image img)
      {
        img.Save((string)x.OutputFile, CodecInfo, EncoderParams);
        img.Dispose();

        if (OriginalFileAction == OriginalFileOption.Delete) System.IO.File.Delete((string)x.InputFile);
        else if (OriginalFileAction == OriginalFileOption.Recycle) FileSystem.Recycle(x.InputFile);
      }

    }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 3 });

    Loader.LinkTo(Saver, new DataflowLinkOptions { PropagateCompletion = true });
  }

  public Task Completion => Saver.Completion;

  public enum OriginalFileOption { Leave, Recycle, Delete };

  // Default was previously recycling, but it takes an age and thus far processing has been reliable.
  // Therefore, going forward, by default, we are just gonna delete the original, for speed.
  public OriginalFileOption OriginalFileAction { get; set; } = OriginalFileOption.Delete;

  // Used to save as Jpeg of specific quality.
  private static ImageCodecInfo CodecInfo { get; }

  // Quality for saved Jpegs
  private EncoderParameters EncoderParams { get; }
  /// <summary>
  /// Loads images to be converted.
  /// </summary>
  private TransformBlock<(FilePath InputFile, FilePath OutputFile), (FilePath InputFile, FilePath OutputFile, Image? Image)> Loader { get; }

  /// <summary>
  /// Converts and saves images supplied by the loader block.
  /// </summary>
  private ActionBlock<(FilePath InputFile, FilePath OutputFile, Image? Image)> Saver { get; }


  public void Complete() => Loader.Complete();

  public void Fault(Exception exception)
  {
    ((IDataflowBlock)Loader).Fault(exception);
  }

  /// <summary>
  /// Adds a file to the conversion queue.
  /// </summary>
  public bool Post(FilePath inputFile, FilePath outputFile) =>
    inputFile is null ? throw new ArgumentNullException(nameof(inputFile))
    : outputFile is null ? throw new ArgumentNullException(nameof(outputFile))
    : Loader.Post((inputFile, outputFile));
}



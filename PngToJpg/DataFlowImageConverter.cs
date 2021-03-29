#nullable enable

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks.Dataflow;

namespace PngToJpg
{
  /// <summary>
  /// Converts PNG -> JPG using DataFlowBlocks. Post items to <see cref="Loader"/> then await <see cref="Saver"/>.Completion
  /// </summary>
  internal class DataFlowImageConverter
  {
    // Quality for saved Jpegs
    private EncoderParameters EncoderParams { get; }

    // Used to save as Jpeg of specific quality.
    private static ImageCodecInfo CodecInfo { get; }

    private static DataflowLinkOptions LinkOptions { get; }

    static DataFlowImageConverter()
    {
      CodecInfo = ImageHelper.GetJpegCodecInfo() ?? throw new Exception("Unable to get Jpeg codec info.");
      LinkOptions = new DataflowLinkOptions { PropagateCompletion = true };
    }

    /// <summary>
    /// Creates a new instance to convert images to JPG with the specified quality.
    /// </summary>
    /// <param name="quality">Value 0 through 100</param>
    public DataFlowImageConverter(long quality)
    {
      EncoderParams = ImageHelper.GetQualityEncoder(quality);

      Loader = new(x => new ImageSaveInfo(x.OutputFile, Image.FromFile(x.InputFile.Value)), new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 2 });

      Saver = new(x =>
      {
        x.Image.Save(x.OutputFile.Value, CodecInfo, EncoderParams);
        x.Image.Dispose();
      }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 2 });

      Loader.LinkTo(Saver, LinkOptions);
    }

    /// <summary>
    /// Loads images to be converted.
    /// </summary>
    public TransformBlock<FilePair, ImageSaveInfo> Loader { get; }

    /// <summary>
    /// Converts and saves images supplied by the loader block.
    /// </summary>
    public ActionBlock<ImageSaveInfo> Saver { get; }

  }

}



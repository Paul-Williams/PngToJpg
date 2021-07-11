#nullable enable

using PW.IO.FileSystemObjects;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace PngToJpg {
  /// <summary>
  /// Converts PNG -> JPG using DataFlowBlocks. Post items to <see cref="Loader"/> then await <see cref="Saver"/>.Completion
  /// </summary>
  internal class DataFlowImageConverter : IDataflowBlock {
    // Quality for saved Jpegs
    private EncoderParameters EncoderParams { get; }

    // Used to save as Jpeg of specific quality.
    private static ImageCodecInfo CodecInfo { get; }

    //private static DataflowLinkOptions LinkOptions { get; }

    static DataFlowImageConverter() {
      CodecInfo = ImageHelper.GetJpegCodecInfo() ?? throw new Exception("Unable to get Jpeg codec info.");
      //LinkOptions = new DataflowLinkOptions { PropagateCompletion = true };
    }

    /// <summary>
    /// Adds a file to the conversion queue.
    /// </summary>
    public bool Post(FilePath inputFile, FilePath outputFile) =>
      inputFile is null ? throw new ArgumentNullException(nameof(inputFile))
      : outputFile is null ? throw new ArgumentNullException(nameof(outputFile))
      : Loader.Post((inputFile, outputFile));

    public void Complete() => Loader.Complete();

    public void Fault(Exception exception) {
      ((IDataflowBlock)Loader).Fault(exception);
    }

    public Task Completion => Saver.Completion;

    public bool DeleteOriginal { get; set; } = true;

    /// <summary>
    /// Creates a new instance to convert images to JPG with the specified quality.
    /// </summary>
    /// <param name="quality">Value 0 through 100</param>
    public DataFlowImageConverter(long quality) {
      EncoderParams = ImageHelper.GetQualityEncoder(quality);

      Loader = new(x => (x.OutputFile, x.InputFile, Image.FromFile(x.InputFile.Value)), new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1 });

      Saver = new(x => {
        x.Image.Save(x.OutputFile.Value, CodecInfo, EncoderParams);
        x.Image.Dispose();
        if (DeleteOriginal) FileSystem.SendToRecycleBin(x.InputFile);
      }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 3 });

      Loader.LinkTo(Saver, new DataflowLinkOptions { PropagateCompletion = true });
    }

    /// <summary>
    /// Loads images to be converted.
    /// </summary>
    private TransformBlock<(FilePath InputFile, FilePath OutputFile), (FilePath InputFile, FilePath OutputFile, Image Image)> Loader { get; }

    /// <summary>
    /// Converts and saves images supplied by the loader block.
    /// </summary>
    private ActionBlock<(FilePath InputFile, FilePath OutputFile, Image Image)> Saver { get; }

  }

}



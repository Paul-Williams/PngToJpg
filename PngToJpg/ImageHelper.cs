# nullable enable

using System;
using System.Drawing.Imaging;
using System.Linq;

// See: https://stackoverflow.com/questions/1484759/quality-of-a-saved-jpg-in-c-sharp

namespace PngToJpg
{
  internal static class ImageHelper
  {
    public static ImageCodecInfo? GetImageCodecInfo(ImageFormat format) 
      => ImageCodecInfo.GetImageDecoders().FirstOrDefault(x => x.FormatID == format.Guid);

    public static ImageCodecInfo? GetJpegCodecInfo() => GetImageCodecInfo(ImageFormat.Jpeg);


    public static EncoderParameters GetQualityEncoder(long quality)
    {
      if (quality is < 1 or > 100) throw new ArgumentException($"'{nameof(quality)}' must be between 0 and 100 (inclusive).");

      var encoderParams = new EncoderParameters(1);
      encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
      return encoderParams;
    }

  }
}

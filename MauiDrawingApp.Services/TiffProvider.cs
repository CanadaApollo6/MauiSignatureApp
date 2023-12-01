using CoreGraphics;
using Foundation;
using ImageIO;
using UIKit;
using MauiDrawingApp.Interfaces;

namespace MauiDrawingApp.Services;

public class TiffProvider : ITiffProvider
{
    public byte[] GetImage(Stream streamImage)
    {
        using UIImage image = UIImage.LoadFromData(NSData.FromStream(streamImage), scale: 1);
        return GetImage(image);
    }

    public byte[] GetImage(UIImage image)
    {
        if (image == null)
        {
            return Array.Empty<byte>();
        }

        using NSData data = ToTiff(image, (nint)image.Size.Width, (nint)image.Size.Height);
        byte[] bytes = data.ToArray();
#if DEBUG
        // For debugging purposes write out the signature tiff file for reviewing
        // var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        //     "signature.tiff");
        // if (data.Save(path, atomically: true))
        // {
        //     Console.WriteLine(path);
        // }

        // File.WriteAllBytes(path, bytes);
#endif
        return bytes;
    }

    /// <summary>
    /// Creates a Data buffer of the supplied image in a tiff packbit format SAP can decode and apply to a Smart Form
    /// </summary>
    /// <returns>The tiff data buffer.</returns>
    /// <param name="image">Image.</param>
    /// <param name="width">Desired tiff image Width.</param>
    /// <param name="height">Desired tiff image Height.</param>
    public NSData ToTiff(UIImage image, nint width, nint height)
    {
        // Data container for the tiff image, caller will need to dispose
        var data = new NSMutableData();

        // Context for writing the tiff format to the data container
        using var context = CGImageDestination.Create(data, MobileCoreServices.UTType.TIFF, 1);
        // Property dictionary for the tiff encoder
        var tiffInfo = new CGImagePropertiesTiff { };
        tiffInfo.Dictionary[ImageIO.CGImageProperties.TIFFCompression] =
            new NSNumber(32773); //NSTIFFCompressionPackBits
                                 // https://developer.apple.com/library/mac/documentation/Cocoa/Reference/ApplicationKit/Classes/NSBitmapImageRep_Class/#//apple_ref/c/tdef/NSTIFFCompression
                                 // NOTE: this is documented available on OSX only but has been tested on iOS
                                 // NOTE: SAP only supports uploading tiff files in uncompressed and packbit formats

        // Create a new bitmap context with greyscale and no alpha channel
        using (var bitmap = new CGBitmapContext(null, width, height, image.CGImage.BitsPerComponent,
            image.CGImage.BytesPerRow, CGColorSpace.CreateGenericGray(), CGImageAlphaInfo.None))
        {
            // bitmap area rect
            var rect = new CGRect(0, 0, bitmap.Width, bitmap.Height);

            // Set the background to white
            bitmap.SetFillColor(UIColor.White.CGColor);
            bitmap.FillRect(rect);
            // Improve image scaling
            bitmap.InterpolationQuality = CGInterpolationQuality.High;
            // Draw the signature on the background scaled to the output dimentions
            bitmap.DrawImage(rect, image.CGImage);

            // Add the bitmap to the output image context
            context.AddImage(bitmap.ToImage(), new CGImageDestinationOptions
            {
                TiffDictionary = tiffInfo,
                DestinationBackgroundColor = UIColor.White.CGColor,
            });
        }

        // Flush to the data buffer
        context.Close();

        // return the data buffer
        return data;
    }
}
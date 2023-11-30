using Foundation;
using UIKit;

namespace MauiDrawingApp.Interfaces;

public interface ITiffProvider
{
    public byte[] GetImage(Stream streamImage);
    public byte[] GetImage(UIImage image);
    public NSData ToTiff(UIImage image, nint width, nint height);
}
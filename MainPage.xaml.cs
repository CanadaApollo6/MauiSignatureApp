using UIKit;
using MauiDrawingApp.Interfaces;

namespace MauiDrawingApp;

public partial class MainPage : ContentPage
{
    private readonly ITiffProvider _tiffProvider;
	public MainPage(ITiffProvider tiffProvider)
	{
        _tiffProvider = tiffProvider;
		InitializeComponent();
	}
    private void DrawBoard_DrawingLineCompleted(object sender, CommunityToolkit.Maui.Core.DrawingLineCompletedEventArgs e)
    {
        ImageView.Dispatcher.Dispatch(async() =>
        {
            var stream = await DrawBoard.GetImageStream(300, 300);
            ImageView.Source = ImageSource.FromStream(() => stream);
        });
    }
    void Button_Clicked(object sender, EventArgs e)
    {
        DrawBoard.Lines.Clear();
        ImageView.Source = null;
    }
    public async void OnAccept(object sender, EventArgs e)
    {
        using var stream = await DrawBoard.GetImageStream(300, 300);
        stream.Position = 0;
        var imageBytes = _tiffProvider.GetImage(stream);

        #if WINDOWS
            await System.IO.File.WriteAllBytesAsync(@"C:\Users\riels\Documents\test.tiff", imageBytes);
        #elif ANDROID

        #elif __IOS__ || __MACCATALYST__
            var image = new UIImage(Foundation.NSData.FromArray(imageBytes));
            image.SaveToPhotosAlbum((image, error) => {});

        #endif
        DrawBoard.Lines.Clear();
        ImageView.Source = null;
    }
}

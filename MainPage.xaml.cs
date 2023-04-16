namespace MauiDrawingApp;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}
    private void DrawBoard_DrawingLineCompleted(System.Object sender, CommunityToolkit.Maui.Core.DrawingLineCompletedEventArgs e)
    {
        ImageView.Dispatcher.Dispatch(async() =>
        {
            var stream = await DrawBoard.GetImageStream(300, 300);
            ImageView.Source = ImageSource.FromStream(() => stream);
        });
    }
    void Button_Clicked(System.Object sender, System.EventArgs e)
    {
        DrawBoard.Lines.Clear();
    }
}


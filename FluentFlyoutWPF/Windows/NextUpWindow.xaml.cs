using FluentFlyout.Classes;
using FluentFlyout.Classes.Settings;
using FluentFlyout.Classes.Utils;
using FluentFlyoutWPF.Classes;
using MicaWPF.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FluentFlyoutWPF.Windows;

/// <summary>
/// Interaction logic for NextUpWindow.xaml
/// </summary>
public partial class NextUpWindow : MicaWindow
{
    MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
    public NextUpWindow(string title, string artist, BitmapImage thumbnail)
    {
        WindowStartupLocation = WindowStartupLocation.Manual;
        Left = -Width - 9999; // move window out of bounds to prevent flickering, maybe needs better solution
        Top = 9999;
        WindowHelper.SetNoActivate(this);
        InitializeComponent();
        WindowHelper.SetTopmost(this);
        CustomWindowChrome.CaptionHeight = 0;
        CustomWindowChrome.UseAeroCaptionButtons = false;
        CustomWindowChrome.GlassFrameThickness = new Thickness(0);
        if (SettingsManager.Current.NextUpAcrylicWindowEnabled)
        {
            WindowBlurHelper.EnableBlur(this);
        }
        else
        {
            WindowBlurHelper.DisableBlur(this);
        }

        var titleWidth = StringWidth.GetStringWidth(title);
        var artistWidth = StringWidth.GetStringWidth(artist);

        string UpNextText = Application.Current.FindResource("NextUpWindow_UpNextText") as string;
        var UpNextTitleWidth = StringWidth.GetStringWidth(UpNextText) + 14;
        if (titleWidth > artistWidth) Width = titleWidth + UpNextTitleWidth + 86;
        else Width = artistWidth + UpNextTitleWidth + 86;
        if (Width > 400) Width = 400; // max width to prevent window from being too wide
        TitleTrans.To = -(Width / 2) + (UpNextTitleWidth / 2) + 12;
        BackgroundTrans.To = -(Width / 2);
        SongTitle.Text = title;
        SongArtist.Text = artist;
        SongImage.ImageSource = thumbnail;
        if (SongImage.ImageSource == null)
            SongImagePlaceholder.Visibility = Visibility.Visible;
        else { SongImagePlaceholder.Visibility = Visibility.Collapsed; BackgroundGradientStopColor.Color = ImageHelper.GetDominantColor(thumbnail); }
        Show();
        mainWindow.OpenAnimation(this);


        async void wait()
        {
            await Task.Delay(SettingsManager.Current.NextUpDuration);
            mainWindow.CloseAnimation(this);
            await Task.Delay(mainWindow.getDuration());
            Close();
        }

        wait();
    }
}
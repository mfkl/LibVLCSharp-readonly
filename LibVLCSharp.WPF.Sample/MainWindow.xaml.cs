using System.Windows.Interop;
using VideoLAN.LibVLC;

namespace LibVLCSharp.WPF.Sample
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
            {
                var window = GetWindow(this);
                var wi = new WindowInteropHelper(window);

                var instance = new Instance();
                var mp = new MediaPlayer(instance)
                {
                    Hwnd = wi.Handle,
                    Media = new Media(instance, "http://www.quirksmode.org/html5/videos/big_buck_bunny.mp4", Media.FromType.FromLocation)
                };
                mp.Play();
            };
        }
    }
}
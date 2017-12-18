using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using VideoLAN.LibVLC;

namespace LibVLCSharp.WPF.Sample
{
    public class MediaElement : UserControl, IDisposable, INotifyPropertyChanged
    {
        static MediaElementCore _core;

        public MediaElement()
        {
            Loaded += OnLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var window = Window.GetWindow(this);
            var wi = new WindowInteropHelper(window);

            _core = new MediaElementCore(action => Dispatcher.InvokeAsync(action))
            {
                Media = new MyMedia(Media.FromType.FromLocation,
                    "http://www.quirksmode.org/html5/videos/big_buck_bunny.mp4"),
                DrawableSurface = wi.Handle
            };

            _core.PropertyChanged += OnCorePropertyChanged;

            _core.Play();
        }

        void OnCorePropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            
        }

        public static DependencyProperty HardwareAccelerationProperty { get; } = DependencyProperty.Register("HardwareAcceleration", typeof(bool), typeof(MediaElement),
            new PropertyMetadata(false));

        public bool HardwareAcceleration
        {
            get => _core.HardwareAcceleration;
            set
            {
                if (_core != null)
                    _core.HardwareAcceleration = value;
            }
        }

        public static DependencyProperty PositionProperty { get; } = DependencyProperty.Register("Position",
            typeof(TimeSpan), typeof(MediaElement),
            new PropertyMetadata(TimeSpan.Zero, (d, e) =>
            {
                _core.Position = (float)e.NewValue;
            }));
        

        public void Dispose()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
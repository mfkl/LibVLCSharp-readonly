using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VideoLAN.LibVLC.Annotations;
using VideoLAN.LibVLC.Events;

namespace VideoLAN.LibVLC
{
    public class MediaElementCore : INotifyPropertyChanged, IDisposable
    {
        readonly Instance _instance;
        readonly MediaPlayer _mp;
        bool _hardwareAcceleration;
        readonly Action<Action> _uiDispatcher;

        public MediaElementCore(Action<Action> uiDispatcher)
        {
            if(uiDispatcher == null)
                throw new NullReferenceException(nameof(uiDispatcher));

            _uiDispatcher = uiDispatcher;
            _instance = new Instance();  
            _mp = new MediaPlayer(_instance);
            Subscribe(_mp.EventManager);
        }
        
        /// <summary>
        /// Windows or macOS
        /// </summary>
        public IntPtr DrawableSurface
        {
            set
            {
                if (value == IntPtr.Zero) throw new NullReferenceException();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    _mp.Hwnd = value;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    _mp.NsObject = value;
            }
        }

        /// <summary>
        /// Linux
        /// </summary>
        public uint XWindow
        {
            set => _mp.XWindow = value;
        }

        void Subscribe(MediaPlayerEventManager eventManager)
        {
            eventManager.Buffering += OnBuffering;
            eventManager.PositionChanged += OnPositionChanged;
            eventManager.AudioDevice += OnAudioDevice;
            eventManager.Backward += OnBackward;
            eventManager.ChapterChanged += OnChapterChanged;
            eventManager.Corked += OnCorked;
            eventManager.VolumeChanged += OnVolumeChanged;
            // more...
        }

        void Unsubscribe(MediaPlayerEventManager eventManager)
        {
            eventManager.Buffering -= OnBuffering;
            eventManager.PositionChanged -= OnPositionChanged;
            eventManager.AudioDevice -= OnAudioDevice;
            eventManager.Backward -= OnBackward;
            eventManager.ChapterChanged -= OnChapterChanged;
            eventManager.Corked -= OnCorked;
            eventManager.VolumeChanged -= OnVolumeChanged;
            // more...
        }

        void OnVolumeChanged(object sender, MediaPlayerVolumeChangedEventArgs mediaPlayerVolumeChangedEventArgs)
        {
            
        }

        void OnCorked(object sender, EventArgs eventArgs)
        {
            
        }

        void OnChapterChanged(object sender, MediaPlayerChapterChangedEventArgs mediaPlayerChapterChangedEventArgs)
        {
            
        }

        void OnBackward(object sender, EventArgs eventArgs)
        {
            
        }

        void OnAudioDevice(object sender, MediaPlayerAudioDeviceEventArgs mediaPlayerAudioDeviceEventArgs)
        {
            
        }

        public void OnPositionChanged(object sender, MediaPlayerPositionChangedEventArgs mediaPlayerPositionChangedEventArgs)
        {
            OnPropertyChanged(nameof(Position));
        }

        void OnBuffering(object sender, MediaPlayerBufferingEventArgs mediaPlayerBufferingEventArgs)
        {
            Buffering = mediaPlayerBufferingEventArgs.Cache;
            OnPropertyChanged(nameof(Buffering)); 
        }

        public bool Play() => _mp.Play();
        public void Pause() => _mp.Pause();
        public void Stop() => _mp.Stop();

        public float Position
        {
            get => _mp.Position;
            set
            {
                _mp.Position = value;
                OnPropertyChanged(nameof(Position));
            } 
        }

        float _buffering;

        public float Buffering
        {
            get => _buffering;
            private set
            {
                _buffering = value;
                OnPropertyChanged(nameof(Buffering));
            }
        }

        IMedia _media;
        public IMedia Media
        {
            get => _media;
            set
            {
                if (_media == value) return;
                if (_mp.Media != null)
                    Unsubscribe(_mp.Media.EventManager);
                _media = value;
                var media = new Media(_instance, _media.Url, _media.Type);
                Subscribe(media.EventManager);
                _mp.Media = media;
            }
        }

        void Subscribe(MediaEventManager mediaEventManager)
        {
            mediaEventManager.DurationChanged += MediaEventManagerOnDurationChanged;
            mediaEventManager.MediaFreed += MediaEventManagerOnMediaFreed;
            mediaEventManager.MetaChanged += MediaEventManagerOnMetaChanged;

            mediaEventManager.ParsedChanged += MediaEventManagerOnParsedChanged;

            mediaEventManager.StateChanged += MediaEventManagerOnStateChanged;
            mediaEventManager.SubItemAdded += MediaEventManagerOnSubItemAdded;
            mediaEventManager.SubItemTreeAdded += MediaEventManagerOnSubItemTreeAdded;
        }

        void MediaEventManagerOnSubItemTreeAdded(object sender, MediaSubItemTreeAddedEventArgs mediaSubItemTreeAddedEventArgs)
        {
            Debug.WriteLine("SubItemTreeAdded");
        }

        void MediaEventManagerOnSubItemAdded(object sender, MediaSubItemAddedEventArgs mediaSubItemAddedEventArgs)
        {
            Debug.WriteLine("SubItemAdded");
        }

        void MediaEventManagerOnStateChanged(object sender, MediaStateChangedEventArgs mediaStateChangedEventArgs)
        {
            Debug.WriteLine("MediaEventManagerOnStateChanged");

            OnPropertyChanged(nameof(State));
        }

        void MediaEventManagerOnParsedChanged(object sender, MediaParsedChangedEventArgs mediaParsedChangedEventArgs)
        {
            Debug.WriteLine("MediaEventManagerOnParsedChanged");

            OnPropertyChanged(nameof(MediaParsedStatus));
        }

        void MediaEventManagerOnMetaChanged(object sender, MediaMetaChangedEventArgs mediaMetaChangedEventArgs)
        {
            //mediaMetaChangedEventArgs.
            //OnPropertyChanged(nameof(MetadataType));
        }

        void MediaEventManagerOnMediaFreed(object sender, MediaFreedEventArgs mediaFreedEventArgs)
        {
            Debug.WriteLine("MediaFreed");
        }

        void MediaEventManagerOnDurationChanged(object sender, MediaDurationChangedEventArgs mediaDurationChangedEventArgs)
        {
            Debug.WriteLine("MediaEventManagerOnDurationChanged");
            OnPropertyChanged(nameof(Duration));
        }

        void Unsubscribe(MediaEventManager mediaEventManager)
        {
            mediaEventManager.DurationChanged -= MediaEventManagerOnDurationChanged;
            mediaEventManager.MediaFreed -= MediaEventManagerOnMediaFreed;
            mediaEventManager.MetaChanged -= MediaEventManagerOnMetaChanged;
            mediaEventManager.ParsedChanged -= MediaEventManagerOnParsedChanged;
            mediaEventManager.StateChanged -= MediaEventManagerOnStateChanged;
            mediaEventManager.SubItemAdded -= MediaEventManagerOnSubItemAdded;
            mediaEventManager.SubItemTreeAdded -= MediaEventManagerOnSubItemTreeAdded;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            _uiDispatcher.Invoke(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }

        public bool HardwareAcceleration
        {
            get => _hardwareAcceleration;
            set
            {
                _hardwareAcceleration = value;
                OnPropertyChanged(nameof(HardwareAcceleration));
            }
        }

        public VLCState State => _mp.State;
        public Media.MediaParsedStatus MediaParsedStatus => _mp.Media.ParsedStatus;
        public long Duration => _mp.Media.Duration;

        public void Dispose()
        {
            _instance?.Dispose();
            _mp?.Dispose();
        }
    }

    public interface IMedia
    {
        Media.FromType Type { get; }
        string Url { get; }
    }

    public class MyMedia : IMedia
    {
        public Media.FromType Type { get; }
        public string Url { get; }

        public MyMedia(Media.FromType type, string url)
        {
            Type = type;
            Url = url;
        }
    }
}

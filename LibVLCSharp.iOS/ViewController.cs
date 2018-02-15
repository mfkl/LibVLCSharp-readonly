using System;
using System.Diagnostics;

using UIKit;

using VideoLAN.LibVLC;

namespace LibVLCSharp.iOS
{
    public partial class ViewController : UIViewController
    {
        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
           
            var instance = new Instance();

            var mp = new VideoLAN.LibVLC.MediaPlayer(instance)
            {
                Media = new Media(instance, "http://www.quirksmode.org/html5/videos/big_buck_bunny.mp4", Media.FromType.FromLocation),
                NsObject = View.Handle
            };

            Debug.WriteLine($"Play succeeded: {mp.Play()}");
        }
    }
}
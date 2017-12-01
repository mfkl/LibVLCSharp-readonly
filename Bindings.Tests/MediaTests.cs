using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using VideoLAN.LibVLC;
using VideoLAN.LibVLC.Manual;

namespace Bindings.Tests
{
    [TestFixture]
    public class MediaTests
    {
        [Test]
        public void CreateMedia()
        {
            var instance = new Instance();

            var media = new Media(instance, Path.GetTempFileName(), Media.FromType.FromPath);

            Assert.AreNotEqual(IntPtr.Zero, media.NativeReference);
        }

        [Test]
        public void CreateMediaFail()
        {
            Assert.Throws<ArgumentNullException>(() => new Media(null, Path.GetTempFileName(), Media.FromType.FromPath));
            Assert.Throws<ArgumentNullException>(() => new Media(new Instance(), string.Empty, Media.FromType.FromPath));
        }

        [Test]
        public void ReleaseMedia()
        {
            var media = new Media(new Instance(), Path.GetTempFileName(), Media.FromType.FromPath);

            media.Dispose();

            Assert.AreEqual(IntPtr.Zero, media.NativeReference);
        }

        [Test]
        public void CreateMediaFromStream()
        {
            var media = new Media(new Instance(), new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate));
            Assert.AreNotEqual(IntPtr.Zero, media.NativeReference);
        }

        [Test]
        public void AddOption()
        {
            var media = new Media(new Instance(), new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate));
            media.AddOption("-sout-all");
        }

        [Test]
        public void CreateRealMedia()
        {
            var instance = new Instance();
            var media = new Media(instance, RealMediaPath, Media.FromType.FromPath);
            
            Assert.False(media.IsParsed);
            media.Parse();

            //await media.ParseAsync();
            Assert.True(media.IsParsed);
            Assert.NotZero(media.Duration);
            Assert.NotZero(media.Tracks.First().Data.Audio.Channels);
            Assert.AreEqual(Media.MediaParsedStatus.Done, media.ParsedStatus);
            Assert.AreEqual(Media.MediaType.File, media.Type);
        }

        /// <summary>
        /// add media file for tests in \bin\x64\Debug\net47
        /// </summary>
        string RealMediaPath
        {
            get
            {
                var dir = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                //var binDir = Path.Combine(dir, "..\\..\\..\\");
                var files = Directory.GetFiles(dir);
                return files.First();
            }
        }
        
        [Test]
        public void Duplicate()
        {
            var media = new Media(new Instance(), new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate));
            var duplicate = media.Duplicate();
            Assert.AreNotEqual(duplicate.NativeReference, media.NativeReference);
        }

        [Test]
        public void CreateMediaFromFileStream()
        {
            // TODO: fix this.
            var media = new Media(new Instance(), new FileStream(RealMediaPath, FileMode.Open, FileAccess.Read, FileShare.Read));
            media.Parse();
            Assert.NotZero(media.Tracks.First().Data.Audio.Channels);
        }

        [Test]
        public void SetMetadata()
        {
            var media = new Media(new Instance(), Path.GetTempFileName(), Media.FromType.FromPath);
            const string test = "test";
            media.SetMeta(Media.MetadataType.ShowName, test);
            Assert.True(media.SaveMeta());
            Assert.AreEqual(test, media.Meta(Media.MetadataType.ShowName));
        }

        [Test]
        public async Task AsyncParse()
        {
            var media = new Media(new Instance(), RealMediaPath, Media.FromType.FromPath);
            var result = await media.ParseAsyncWithOptions();
            Assert.True(result);
        }

        [Test]
        public async Task AsyncParseTimeoutStop()
        {
            //TODO: fix
            Assert.Inconclusive();
            var media = new Media(new Instance(), RealMediaPath, Media.FromType.FromPath);
            var called = false;

            media.EventManager.ParsedChanged += (sender, args) =>
            {
                Assert.True(args.ParsedStatus == Media.MediaParsedStatus.Timeout);
                called = true;
            };
            var result = await media.ParseAsyncWithOptions(timeout: 1);
            Assert.False(result);
            Assert.True(called);
        }

        [Test]
        public async Task AsyncParseCancel()
        {
            //TODO: fix
            Assert.Inconclusive();
            var media = new Media(new Instance(), RealMediaPath, Media.FromType.FromPath);
            var called = false;
            media.EventManager.ParsedChanged += (sender, args) =>
            {
                Assert.True(args.ParsedStatus == Media.MediaParsedStatus.Failed);
                called = true;
            };
            // current code cancels tasks before the parsing even starts so parseStatus is never set to failed.
            var result = await media.ParseAsyncWithOptions(cancellationToken: new CancellationToken(canceled: true));
            Assert.False(result);
            Assert.True(called);
        }
    }
}

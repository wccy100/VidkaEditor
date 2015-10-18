using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vidka.Core;

namespace Vidka.Components
{
    public class AudioPlayerNAudio : IAudioPlayer
    {
        private const double NAUDIO_SYNC_OFFSET = -0.5;
        private const double NAUDIO_SLEEP_FRAME_TIME = 0.1;

        private List<AudioClipToPlay> clipsToPlay;
        private List<DeviceAndItsData> devicesPlaying;
        private List<DeviceAndItsData> devicesPaused;
        //private List<DeviceAndItsData> waveOutDevices;

        public AudioPlayerNAudio()
        {
            clipsToPlay = new List<AudioClipToPlay>();
            devicesPlaying = new List<DeviceAndItsData>();
            devicesPaused = new List<DeviceAndItsData>();
        }

        public void AddAudioClip(double secOffset, double secIn, double secOut, string filename, object clipObj)
        {
            clipsToPlay.Add(new AudioClipToPlay {
                Filename = filename,
                SecOffset = secOffset,
                SecFileStart = secIn,
                SecFileEnd = secOut,
                ClipObj = clipObj
            });

        }
        public void Clear()
        {
            clipsToPlay.Clear();
        }
        public void WeAreHereStartPlaying(double curSec)
        {
            var allClipsUnderThisSecNew = clipsToPlay
                .Where(x => curSec >= x.SecOffset && curSec <= x.SecEndAbs)      // within time bounds
                .Where(x => !devicesPlaying.Any(dev => dev.WhatIAmPlaying == x)) // exclude those already playing.. leave them alone
                .Where(x => !devicesPaused.Any(dev => dev.WhatIAmPlaying == x)); // exclude those previously paused.. we will resume them later
            foreach (var clip in allClipsUnderThisSecNew)
            {
                StartPlayingThisClip(clip, curSec);
            }
            lock (this)
            {
                foreach (var device in devicesPaused)
                {
                    var syncOffsetSec = curSec - device.WhatIAmPlaying.SecOffset; // would be 0 if curSec == clip.SecOffset
                    var clipSecStart = device.WhatIAmPlaying.SecFileStart + device.WhatIAmPlaying.SecOffset;
                    device.WaveReader.CurrentTime = TimeSpan.FromSeconds(clipSecStart + syncOffsetSec + NAUDIO_SYNC_OFFSET);
                    device.Wave.Play();
                    devicesPlaying.Add(device);
                }
                devicesPaused.Clear();
            }
        }

        private void StartPlayingThisClip(AudioClipToPlay clip, double curSec)
        {
            cxzxc(string.Format("start of {0} {1}-{2}", clip.Filename, clip.SecFileStart, clip.SecFileEnd));
            // create the device
            IWavePlayer waveOutDevice = new WaveOut();
            var deviceAndShit = new DeviceAndItsData(waveOutDevice, clip);
            lock (this) { devicesPlaying.Add(deviceAndShit); }
            Thread ttt = new Thread(() =>
            {
                using (var reader = OpenAudioFile(clip.Filename))
                {
                    deviceAndShit.WaveReader = reader;
                    // init the playback stuff
                    waveOutDevice.Init(reader);
                    var syncOffsetSec = curSec - clip.SecOffset; // would be 0 if curSec == clip.SecOffset
                    var clipSecStart = clip.SecFileStart + syncOffsetSec;
                    var clipSecEnd = clip.SecFileEnd;
                    reader.CurrentTime = TimeSpan.FromSeconds(clipSecStart + NAUDIO_SYNC_OFFSET);
                    var endTs = TimeSpan.FromSeconds(clipSecEnd + NAUDIO_SYNC_OFFSET);
                    var sleepTs = TimeSpan.FromSeconds(NAUDIO_SLEEP_FRAME_TIME);
                    deviceAndShit.PlaybackStateRequest = PlaybackState.Playing;
                    waveOutDevice.Play();
                    while (reader.CurrentTime < endTs)
                    {
                        Thread.Sleep(sleepTs);
                        if (waveOutDevice.PlaybackState == PlaybackState.Playing && deviceAndShit.PlaybackStateRequest == PlaybackState.Paused)
                            waveOutDevice.Pause();
                        if (waveOutDevice.PlaybackState == PlaybackState.Paused && deviceAndShit.PlaybackStateRequest == PlaybackState.Playing)
                            waveOutDevice.Play();
                        if (deviceAndShit.StopHasBeenInvoked)
                            break;
                        if (waveOutDevice.PlaybackState == PlaybackState.Stopped)
                            break;
                    }
                    waveOutDevice.Stop(); // no harm calling stop 2x right?
                    lock (this) {
                        devicesPlaying.Remove(deviceAndShit);
                        devicesPaused.Remove(deviceAndShit);
                    }
                    cxzxc(string.Format("stop of {0} {1}-{2}", clip.Filename, clip.SecFileStart, clip.SecFileEnd));
                }
            });
            ttt.Start();
        }

        public void PauseAll()
        {
            lock (this)
            {
                foreach (var devicePlaying in devicesPlaying)
                {
                    devicePlaying.Wave.Pause();
                    devicePlaying.PlaybackStateRequest = PlaybackState.Paused;
                    devicesPaused.Add(devicePlaying);
                }
                devicesPlaying.Clear();
            }
        }
        public void StopWhateverYouArePlaying()
        {
            StopAllDevices(devicesPlaying);
            StopAllDevices(devicesPaused);
        }
        private void StopAllDevices(List<DeviceAndItsData> deviceList)
        {
            lock (this)
            {
                foreach (var ddd in deviceList)
                {
                    ddd.StopHasBeenInvoked = true;
                    ddd.Wave.Stop();
                }
                deviceList.Clear();
            }
        }

        private WaveStream OpenAudioFile(string filename)
        {
            var extension = Path.GetExtension(filename).ToLower();
            if (extension == ".wav" || extension == ".au")
                return new WaveFileReader(filename);
            return new Mp3FileReader(filename);
        }

        private void cxzxc(string s)
        {
            //VideoShitbox.ConsoleSingleton.cxzxc(s);
        }

        //public void PlayAudioClip(string filename, double clipSecStart, double clipSecEnd)
        //{
        //    Thread ttt = new Thread(() =>
        //    {
        //        IWavePlayer waveOutDevice = new WaveOut();
        //        using (var reader = OpenAudioFile(filename))
        //        {
        //            var deviceAndShit = new DeviceAndItsData(waveOutDevice);
        //            waveOutDevices.Add(deviceAndShit);
        //            waveOutDevice.Init(reader);
        //            reader.CurrentTime = TimeSpan.FromSeconds(clipSecStart + NAUDIO_SYNC_OFFSET);
        //            var endTs = TimeSpan.FromSeconds(clipSecEnd + NAUDIO_SYNC_OFFSET);
        //            var sleepTs = TimeSpan.FromSeconds(NAUDIO_SLEEP_FRAME_TIME);
        //            waveOutDevice.Play();
        //            while (reader.CurrentTime < endTs)
        //            {
        //                Thread.Sleep(sleepTs);
        //                if (deviceAndShit.StopHasBeenInvoked)
        //                    break;
        //                if (waveOutDevice.PlaybackState == PlaybackState.Playing && deviceAndShit.PlaybackStateRequest == PlaybackState.Paused)
        //                    waveOutDevice.Pause();
        //                if (waveOutDevice.PlaybackState == PlaybackState.Paused && deviceAndShit.PlaybackStateRequest == PlaybackState.Playing)
        //                    waveOutDevice.Play();
        //                if (deviceAndShit.StopHasBeenInvoked)
        //                    break;
        //                if (waveOutDevice.PlaybackState == PlaybackState.Stopped)
        //                    break;
        //            }
        //            waveOutDevice.Stop(); // no harm calling stop 2x right?
        //        }
        //    });
        //    ttt.Start();
        //}

        /// <summary>
        /// Code from Ilya Franker
        /// </summary>
        //public static void PlayAllOfFile(string srcFilename, Action donePlaying = null)
        //{
        //    Thread ttt = new Thread(() =>
        //    {
        //        IWavePlayer waveOutDevice = new WaveOut();
        //        using (var reader = new WaveFileReader(srcFilename))
        //        {
        //            waveOutDevice.Init(reader);
        //            reader.CurrentTime = TimeSpan.MaxValue;
        //            //Seek(0, System.IO.SeekOrigin.Begin);
        //            waveOutDevice.Play();
        //            Thread.Sleep(reader.TotalTime);
        //        }
        //        if (donePlaying != null)
        //            donePlaying();
        //    });
        //    ttt.Start();
        //}

        private class AudioClipToPlay
        {
            public string Filename { get; set; }
            public double SecOffset { get; set; }
            public double SecFileStart { get; set; }
            public double SecFileEnd { get; set; }
            public object ClipObj { get; set; }

            public double SecEndAbs { get {
                return SecOffset + SecFileEnd - SecFileStart;
            } }
        }

        private class DeviceAndItsData
        {
            public DeviceAndItsData(IWavePlayer waveOutDevice, AudioClipToPlay clip)
            {
                this.Wave = waveOutDevice;
                this.WhatIAmPlaying = clip;
            }

            public IWavePlayer Wave { get; set; }
            public WaveStream WaveReader;
            public AudioClipToPlay WhatIAmPlaying { get; set; }
            public bool StopHasBeenInvoked { get; set; }

            public PlaybackState PlaybackStateRequest { get; set; }
        }
    }
}

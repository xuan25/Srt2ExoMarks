using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaInfoLib;

namespace Srt2ExoMarks
{
    class Info
    {
        MediaInfo mediaInfo;

        public Info(string path)
        {
            mediaInfo = new MediaInfo();
            mediaInfo.Open(path);
        }

        public bool GetHasAudio()
        {
            return mediaInfo.Count_Get(StreamKind.Audio) > 0;
        }

        public float GetFrameRate()
        {
            return float.Parse(mediaInfo.Get(StreamKind.Video, 0, "FrameRate"));
        }

        public ulong GetFrameCount()
        {
            return ulong.Parse(mediaInfo.Get(StreamKind.Video, 0, "FrameCount"));
        }

        public uint GetWidth()
        {
            return uint.Parse(mediaInfo.Get(StreamKind.Video, 0, "Width"));
        }

        public uint GetHeight()
        {
            return uint.Parse(mediaInfo.Get(StreamKind.Video, 0, "Height"));
        }

        public uint GetSamplingRate()
        {
            return uint.Parse(mediaInfo.Get(StreamKind.Audio, 0, "SamplingRate"));
        }

        public uint GetChannels()
        {
            return uint.Parse(mediaInfo.Get(StreamKind.Audio, 0, "Channel(s)"));
        }

        public void Close()
        {
            mediaInfo.Close();
        }
    }
}

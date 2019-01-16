using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Srt2ExoMarks
{
    class Exo
    {
        public class Item
        {
            public class SubItem
            {
                public ulong index;
                public uint subIndex;
                public string _name;
                Dictionary<string, string> properties;

                public SubItem(ulong index, uint subIndex, string _name)
                {
                    this.index = index;
                    this.subIndex = subIndex;
                    this._name = _name;
                    properties = new Dictionary<string, string>();
                }

                public void AddProperty(string name, string value)
                {
                    properties.Add(name, value);
                }

                public override string ToString()
                {
                    string str = "";
                    str += "[" + index + "." + subIndex + "]\r\n";
                    str += "_name=" + _name + "\r\n";
                    foreach (KeyValuePair<string, string> i in properties)
                    {
                        str += i.Key + "=" + i.Value + "\r\n";
                    }
                    return str;
                }
            }

            public ulong index;
            public ulong start;
            public ulong end;
            public uint layer;
            public uint group;
            public uint overlay;
            public uint camera;
            public bool audio;
            public bool chain;
            public List<SubItem> SubItems;

            public Item(ulong index, ulong start, ulong end, uint layer, uint group, uint overlay, uint camera, bool audio, bool chain)
            {
                this.index = index;
                this.start = start;
                this.end = end;
                this.layer = layer;
                this.group = group;
                this.overlay = overlay;
                this.camera = camera;
                this.audio = audio;
                this.chain = chain;
                SubItems = new List<SubItem>();
            }

            public void AddSubItem(SubItem subItem)
            {
                SubItems.Add(subItem);
            }

            public override string ToString()
            {
                string str = "";
                str += "[" + index + "]\r\n";
                str += "start=" + start + "\r\n";
                str += "end=" + end + "\r\n";
                str += "layer=" + layer + "\r\n";
                str += "group=" + group + "\r\n";
                if(overlay != 0)
                    str += "overlay=" + overlay + "\r\n";
                str += "camera=" + camera + "\r\n";
                if (audio)
                    str += "audio=1\r\n";
                if (chain)
                    str += "chain=1\r\n";
                foreach (SubItem i in SubItems)
                    str += i;
                return str;
            }
        }

        public uint width;
        public uint height;
        public float rate;
        public uint scale;
        public ulong length;
        public uint audio_rate;
        public uint audio_ch;
        public List<Item> Items;

        public Exo(uint width, uint height, float rate, uint scale, ulong length, uint audio_rate, uint audio_ch)
        {
            this.width = width;
            this.height = height;
            this.rate = rate;
            this.scale = scale;
            this.length = length;
            this.audio_rate = audio_rate;
            this.audio_ch = audio_ch;
            Items = new List<Item>();
        }

        public void AddItem(Item item)
        {
            Items.Add(item);
        }

        public override string ToString()
        {
            string str = "";
            str += "[exedit]\r\n";
            str += "width=" + width + "\r\n";
            str += "height=" + height + "\r\n";
            str += "rate=" + rate + "\r\n";
            str += "scale=" + scale + "\r\n";
            str += "length=" + length + "\r\n";
            str += "audio_rate=" + audio_rate + "\r\n";
            str += "audio_ch=" + audio_ch + "\r\n";
            foreach (Item i in Items)
                str += i;
            return str;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Graphics {
    public class ColorMap {
        public IO.File File { get; private set; }
        public Color[] Colors { get; private set; }

        public ColorMap(IO.File file) {
            this.File = file;

            Build();
        }

        private void Build() {
            byte[] buffer = File.GetData();
            this.Colors = new Color[buffer.Length / 4];

            for (int i = 0; i < buffer.Length; i += 4) {
                byte r = buffer[i];
                byte g = buffer[i + 1];
                byte b = buffer[i + 2];
                byte a = buffer[i + 3];
                this.Colors[i / 4] = Color.FromArgb(a, r, g, b);
            }
        }
    }
}

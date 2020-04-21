﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using SaintCoinach.IO;

using Directory = SaintCoinach.IO.Directory;
using File = SaintCoinach.IO.File;

namespace SaintCoinach.Imaging {
    /// <summary>
    ///     Image file stored inside SqPack.
    /// </summary>
    public class ImageFile : File {
        #region Fields

        private WeakReference<byte[]> _BufferCache;
        private WeakReference<Image> _ImageCache;

        #endregion

        #region Properties

        public ImageHeader ImageHeader { get; private set; }
        public int Width { get { return ImageHeader.Width; } }
        public int Height { get { return ImageHeader.Height; } }
        public ImageFormat Format { get { return ImageHeader.Format; } }

        #endregion

        #region Constructors

        public ImageFile(Pack pack, FileCommonHeader commonHeader)
            : base(pack, commonHeader) {
            Stream stream = GetSourceStream();
            stream.Position = CommonHeader.EndOfHeader;
            ImageHeader = new ImageHeader(stream);
        }

        #endregion

        #region Read

        public Image GetImage() {
            if (_ImageCache != null && _ImageCache.TryGetTarget(out Image image)) return image;

            image = ImageConverter.Convert(this);

            if (_ImageCache == null)
                _ImageCache = new WeakReference<Image>(image);
            else
                _ImageCache.SetTarget(image);

            return image;
        }

        public override byte[] GetData() {
            if (_BufferCache != null && _BufferCache.TryGetTarget(out byte[] buffer)) return buffer;

            buffer = Read();

            if (_BufferCache == null)
                _BufferCache = new WeakReference<byte[]>(buffer);
            else
                _BufferCache.SetTarget(buffer);

            return buffer;
        }

        private byte[] Read() {
            Stream sourceStream = GetSourceStream();
            IEnumerable<int> offsets = GetBlockOffsets();

            byte[] data;
            using (MemoryStream dataStream = new MemoryStream((int)CommonHeader.Length)) {
                foreach (int offset in offsets) {
                    sourceStream.Position = ImageHeader.EndOfHeader + offset;
                    ReadBlock(sourceStream, dataStream);
                }
                data = dataStream.ToArray();
            }
            return data;
        }

        private IEnumerable<int> GetBlockOffsets() {
            const int CountOffset = 0x14;
            const int EntryLength = 0x14;
            const int BlockInfoOffset = 0x18;

            short count = BitConverter.ToInt16(CommonHeader._Buffer, CountOffset);
            int currentOffset = 0;
            List<int> offsets = new List<int>();

            for (int i = BlockInfoOffset + count * EntryLength; i + 2 <= CommonHeader._Buffer.Length; i += 2) {
                ushort len = BitConverter.ToUInt16(CommonHeader._Buffer, i);
                if (len == 0)
                    break;
                offsets.Add(currentOffset);
                currentOffset += len;
            }

            return offsets.ToArray();
        }

        #endregion
    }
}

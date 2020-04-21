using System;
using System.Collections.Generic;
using System.IO;

using SaintCoinach.IO;

using Directory = SaintCoinach.IO.Directory;
using File = SaintCoinach.IO.File;

namespace SaintCoinach.Graphics {
    /// <summary>
    ///     Model file stored inside SqPack.
    /// </summary>
    public class ModelFile : File {
        #region Static

        public const int PartsCount = 0x0B;

        #endregion

        #region Fields

        private readonly WeakReference<byte[]>[] _PartsCache = new WeakReference<byte[]>[PartsCount];
        private int[] _BlockOffsets;
        private WeakReference<byte[]> _CombinedCache;
        private WeakReference<ModelDefinition> _DefinitionCache;

        #endregion

        #region Constructors

        public ModelFile(Pack pack, FileCommonHeader commonHeader) : base(pack, commonHeader) { }

        #endregion

        #region Read

        public ModelDefinition GetModelDefinition() {
            ModelDefinition def;
            if (_DefinitionCache != null && _DefinitionCache.TryGetTarget(out def))
                return def;

            def = new ModelDefinition(this);
            if (_DefinitionCache == null)
                _DefinitionCache = new WeakReference<ModelDefinition>(def);
            else
                _DefinitionCache.SetTarget(def);

            return def;
        }

        public override byte[] GetData() {
            byte[] data;

            if (_CombinedCache != null && _CombinedCache.TryGetTarget(out data)) return data;

            data = new byte[0];
            int o = 0;

            for (int i = 0; i < PartsCount; ++i) {
                byte[] part = GetPart(i);
                Array.Resize(ref data, data.Length + part.Length);
                Array.Copy(part, 0, data, o, part.Length);
                o += part.Length;
            }

            if (_CombinedCache == null)
                _CombinedCache = new WeakReference<byte[]>(data);
            else
                _CombinedCache.SetTarget(data);

            return data;
        }

        public byte[] GetPart(int part) {
            if (part >= PartsCount)
                throw new ArgumentOutOfRangeException("part");

            byte[] buffer;

            if (_PartsCache[part] != null && _PartsCache[part].TryGetTarget(out buffer)) return buffer;

            buffer = ReadPart(part);

            if (_PartsCache[part] == null)
                _PartsCache[part] = new WeakReference<byte[]>(buffer);
            else
                _PartsCache[part].SetTarget(buffer);

            return buffer;
        }

        private byte[] ReadPart(int part) {
            const int MinimumBlockOffset = 0x9C;
            const int BlockCountOffset = 0xB2;

            if (_BlockOffsets == null)
                _BlockOffsets = GetBlockOffsets();

            short minBlock = BitConverter.ToInt16(CommonHeader._Buffer, MinimumBlockOffset + 2 * part);
            short blockCount = BitConverter.ToInt16(CommonHeader._Buffer, BlockCountOffset + 2 * part);

            Stream sourceStream = GetSourceStream();
            byte[] buffer;

            using (MemoryStream dataStream = new MemoryStream()) {
                for (int i = 0; i < blockCount; ++i) {
                    sourceStream.Position = CommonHeader.EndOfHeader + _BlockOffsets[minBlock + i];
                    ReadBlock(sourceStream, dataStream);
                }

                buffer = dataStream.ToArray();
            }

            return buffer;
        }

        private int[] GetBlockOffsets() {
            const int BlockInfoOffset = 0xD0;

            int currentOffset = 0;
            List<int> offsets = new List<int>();
            for (int i = BlockInfoOffset; i + 2 <= CommonHeader._Buffer.Length; i += 2) {
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

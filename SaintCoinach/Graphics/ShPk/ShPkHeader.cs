using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SaintCoinach.Graphics.ShPk {
    public class ShPkHeader {
        #region Static

        public const int Size = 0x48;

        #endregion

        #region Properties

        public int FileLength { get; private set; }
        public int ShaderDataOffset { get; private set; }
        public int ParameterListOffset { get; private set; }
        public int VertexShaderCount { get; private set; }
        public int PixelShaderCount { get; private set; }
        public int ScalarParameterCount { get; private set; }
        public int ResourceParameterCount { get; private set; }
        public IReadOnlyList<ShaderHeader> ShaderHeaders { get; private set; }
        public IReadOnlyList<ParameterHeader> ParameterHeaders { get; private set; }

        #endregion

        #region Constructors

        #region Constructor

        public ShPkHeader(byte[] buffer) {
            const uint Magic = 0x6B506853;
            const uint SupportedFormat = 0x00395844;

            if (BitConverter.ToUInt32(buffer, 0x00) != Magic)
                throw new NotSupportedException("File is not a ShPk.");
            if (BitConverter.ToUInt32(buffer, 0x08) != SupportedFormat)
                throw new NotSupportedException("Shader format is not supported.");

            FileLength = BitConverter.ToInt32(buffer, 0x0C);
            ShaderDataOffset = BitConverter.ToInt32(buffer, 0x10);
            ParameterListOffset = BitConverter.ToInt32(buffer, 0x14);
            VertexShaderCount = BitConverter.ToInt32(buffer, 0x18);
            PixelShaderCount = BitConverter.ToInt32(buffer, 0x1C);

            ScalarParameterCount = BitConverter.ToInt32(buffer, 0x28);
            ResourceParameterCount = BitConverter.ToInt32(buffer, 0x2C);

            int offset = Size;
            List<ShaderHeader> shs = new List<ShaderHeader>();
            for (int i = 0; i < VertexShaderCount; ++i) {
                ShaderHeader sh = new ShaderHeader(ShaderType.Vertex, buffer, offset);
                offset += sh.Size;
                shs.Add(sh);
            }
            for (int i = 0; i < PixelShaderCount; ++i) {
                ShaderHeader sh = new ShaderHeader(ShaderType.Pixel, buffer, offset);
                offset += sh.Size;
                shs.Add(sh);
            }
            ShaderHeaders = new ReadOnlyCollection<ShaderHeader>(shs);


            int c1 = BitConverter.ToInt32(buffer, 0x24);
            offset += c1 * 0x08;

            List<ParameterHeader> para = new List<ParameterHeader>();
            for (int i = 0; i < ScalarParameterCount; ++i) {
                ParameterHeader p = new ParameterHeader(ParameterType.Scalar, buffer, offset);
                offset += ParameterHeader.Size;
                para.Add(p);
            }
            for (int i = 0; i < ResourceParameterCount; ++i) {
                ParameterHeader p = new ParameterHeader(ParameterType.Resource, buffer, offset);
                offset += ParameterHeader.Size;
                para.Add(p);
            }
            ParameterHeaders = new ReadOnlyCollection<ParameterHeader>(para);
        }

        #endregion

        #endregion
    }
}

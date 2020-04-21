using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotSquish {
    public static class Squish {
        public static int GetStorageRequirements(int width, int height, SquishOptions flags) {
            int blockCount = ((width + 3) / 4) * ((height + 3) / 4);
            int blockSize = flags.HasFlag(SquishOptions.DXT1) ? 8 : 16;
            return blockCount * blockSize;
        }

        #region On buffer
        public static byte[] CompressBlock(byte[] rgba, SquishOptions flags) {
            return CompressBlockMasked(rgba, 0xFFFF, flags);
        }
        public static byte[] CompressBlockMasked(byte[] rgba, int mask, SquishOptions flags) {
            throw new NotImplementedException();
        }
        public static byte[] DecompressBlock(byte[] block, int blockOffset, SquishOptions flags) {
            // Get the block locations
            int colOff = blockOffset;
            int alphaOff = blockOffset;
            if ((flags & (SquishOptions.DXT3 | SquishOptions.DXT5)) != 0)
                colOff += 8;

            // Decompress colour.
            byte[] rgba = ColourBlock.DecompressColour(block, colOff, flags.HasFlag(SquishOptions.DXT1));

            // Decompress alpha seperately if necessary.
            if (flags.HasFlag(SquishOptions.DXT3))
                Alpha.DecompressAlphaDxt3(block, alphaOff, rgba, 0);
            else if (flags.HasFlag(SquishOptions.DXT5))
                Alpha.DecompressAlphaDxt5(block, alphaOff, rgba, 0);

            return rgba;
        }

        public static byte[] CompressImage(byte[] rgba, int width, int height, SquishOptions flags) {
            throw new NotImplementedException();
        }
        public static byte[] DecompressImage(byte[] blocks, int width, int height, SquishOptions flags) {
            return DecompressImage(blocks, 0, width, height, flags);
        }
        public static byte[] DecompressImage(byte[] blocks, int offset, int width, int height, SquishOptions flags) {
            byte[] argb = new byte[4 * width * height];
            int bytesPerBlock = flags.HasFlag(SquishOptions.DXT1) ? 8 : 16;

            int blockOffset = offset;
            // Loop over blocks.
            for (int y = 0; y < height; y += 4) {
                for (int x = 0; x < width; x += 4) {
                    // Decompress the block.
                    byte[] targetRgba = DecompressBlock(blocks, blockOffset, flags);

                    // Write the decompressed pixels to the correct image locations.
                    int sourcePixelOffset = 0;
                    for (int py = 0; py < 4; ++py) {
                        for (int px = 0; px < 4; ++px) {
                            // Get the target location.
                            int sx = x + px;
                            int sy = y + py;
                            if (sx < width && sy < height) {
                                int targetPixelOffset = 4 * ((width * sy) + sx);
                                // Copy the rgba value
                                argb[targetPixelOffset + 0] = targetRgba[sourcePixelOffset + 2];
                                argb[targetPixelOffset + 1] = targetRgba[sourcePixelOffset + 1];
                                argb[targetPixelOffset + 2] = targetRgba[sourcePixelOffset + 0];
                                argb[targetPixelOffset + 3] = targetRgba[sourcePixelOffset + 3];
                            }
                            sourcePixelOffset += 4;
                        }
                    }

                    // advance
                    blockOffset += bytesPerBlock;
                }
            }
            return argb;
        }
        public static Image DecompressToBitmap(byte[] blocks, int width, int height, SquishOptions flags) {
            return DecompressToBitmap(blocks, 0, width, height, flags);
        }
        public static unsafe Image DecompressToBitmap(byte[] blocks, int offset, int width, int height, SquishOptions flags) {
            byte[] fullBuffer = new byte[4 * width * height];
            int bufferOffset = 0;

            int bytesPerBlock = flags.HasFlag(SquishOptions.DXT1) ? 8 : 16;
            int blockOffset = offset;
            // Loop over blocks.
            for (int y = 0; y < height; y += 4) {
                for (int x = 0; x < width; x += 4) {
                    // Decompress the block.
                    byte[] targetRgba = DecompressBlock(blocks, blockOffset, flags);


                    // Write the decompressed pixels to the correct image locations.
                    int sourcePixelOffset = 0;
                    for (int py = 0; py < 4; ++py) {
                        for (int px = 0; px < 4; ++px) {
                            // Get the target location.
                            int sx = x + px;
                            int sy = y + py;
                            if (sx < width && sy < height) {
                                int i = 4 * (sx + (sy * width));
                                fullBuffer[bufferOffset + i + 0] = targetRgba[sourcePixelOffset + 2];
                                fullBuffer[bufferOffset + i + 1] = targetRgba[sourcePixelOffset + 1];
                                fullBuffer[bufferOffset + i + 2] = targetRgba[sourcePixelOffset + 0];
                                fullBuffer[bufferOffset + i + 3] = targetRgba[sourcePixelOffset + 3];
                            }

                            sourcePixelOffset += 4; // Skip this pixel as it is outside the image.
                        }
                    }

                    // advance
                    blockOffset += bytesPerBlock;
                }
            }
            Image ret;
            fixed (byte* p = fullBuffer) {
                IntPtr ptr = (IntPtr)p;
                Bitmap tempImage = new Bitmap(width, height, 4 * width, System.Drawing.Imaging.PixelFormat.Format32bppArgb, ptr);
                ret = new Bitmap(tempImage);
            }
            return ret;
        }
        #endregion

        #region On stream
        public static void CompressBlock(Stream input, Stream output, SquishOptions flags) {
            CompressBlockMasked(input, output, 0xFFFF, flags);
        }
        public static void CompressBlockMasked(Stream input, Stream output, int mask, SquishOptions flags){
            throw new NotImplementedException();
        }
        public static void DecompressBlock(Stream input, Stream output, SquishOptions flags) {
            throw new NotImplementedException();
        }
        public static void CompressImage(Stream input, Stream output, int width, int height, SquishOptions flags) {
            throw new NotImplementedException();
        }
        public static void DecompressImage(Stream input, Stream output, int width, int height, SquishOptions flags) {
            throw new NotImplementedException();
        }
        #endregion
    }
}

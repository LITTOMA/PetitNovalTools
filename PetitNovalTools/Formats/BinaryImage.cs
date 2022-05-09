using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PetitNovalTools.Formats
{
    public enum BinaryPixelFormat : ushort
    {
        Rgba32 = 1,
        Rgba16 = 99,
        Rgb565
    }

    public class BinaryImage
    {
        public BinaryPixelFormat PixelFormat { get; set; }
        public ushort Width { get; private set; }
        public ushort Height { get; private set; }
        public Image Image
        {
            get => GetImage();
            set => SetImage(value);
        }

        private byte[] data;
        private bool isCompressed;

        public BinaryImage(ushort width, ushort height)
        {
            PixelFormat = BinaryPixelFormat.Rgba32;
            Width = width;
            Height = height;
            data = new byte[Width * Height * 4 + 6];
            isCompressed = true;
        }

        public BinaryImage(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var reader = new BinaryReader(stream);
                var header = reader.ReadBytes(4);
                if (header[0] == 0x10)
                {
                    isCompressed = true;
                    var sizeBuffer = new byte[4];
                    Array.Copy(header, 1, sizeBuffer, 0, 3);
                    var decompressedSize = BinaryPrimitives.ReadInt32LittleEndian(sizeBuffer);
                    data = Compression.LZ10.Decompress(stream, decompressedSize);
                }
                else
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    data = reader.ReadBytes((int)stream.Length);
                }
            }

            if (data == null)
            {
                throw new Exception($"Failed to read file: {path}");
            }

            PixelFormat = (BinaryPixelFormat)BinaryPrimitives.ReadUInt16LittleEndian(data);
            Width = BinaryPrimitives.ReadUInt16LittleEndian(data.Skip(2).Take(2).ToArray());
            Height = BinaryPrimitives.ReadUInt16LittleEndian(data.Skip(4).Take(2).ToArray());
        }

        public void Save(string path)
        {
            var ushortBuffer = new byte[2];
            BinaryPrimitives.WriteUInt16LittleEndian(ushortBuffer, (ushort)PixelFormat);
            Array.Copy(ushortBuffer, 0, data, 0, 2);
            BinaryPrimitives.WriteUInt16LittleEndian(ushortBuffer, Width);
            Array.Copy(ushortBuffer, 0, data, 2, 2);
            BinaryPrimitives.WriteUInt16LittleEndian(ushortBuffer, Height);
            Array.Copy(ushortBuffer, 0, data, 4, 2);

            using (var fs = File.Create(path))
            {
                var writer = new BinaryWriter(fs);
                if (isCompressed)
                {
                    var compressedData = Compression.LZ10.Compress(new MemoryStream(data));

                    var sizeBuffer = new byte[4];
                    BinaryPrimitives.WriteInt32LittleEndian(sizeBuffer, data.Length);
                    writer.Write((byte)0x10);
                    writer.Write(sizeBuffer.Take(3).ToArray());
                    writer.Write(compressedData);
                }
                else
                {
                    writer.Write(data);
                }
                fs.Flush();
            }
        }

        private Image GetImage()
        {
            if (data != null)
            {
                switch (PixelFormat)
                {
                    default:
                        throw new NotSupportedException($"Pixel format: {PixelFormat}");
                    case BinaryPixelFormat.Rgba32:
                        return Image.LoadPixelData<Rgba32>(data.Skip(6).ToArray(), Width, Height);
                    case BinaryPixelFormat.Rgba16:
                        return Image.LoadPixelData<Bgra4444>(data.Skip(6).ToArray(), Width, Height);
                    case BinaryPixelFormat.Rgb565:
                        return Image.LoadPixelData<Bgr565>(data.Skip(6).ToArray(), Width, Height);
                }
            }

            return null;
        }

        private void SetImage(Image value)
        {
            if (value == null)
            {
                return;
            }

            if (value.Width != Width || value.Height != Height)
            {
                throw new NotSupportedException("Changing image size is not supported");
            }

            switch (PixelFormat)
            {
                default:
                    break;
                case BinaryPixelFormat.Rgba32:
                    var img = value.CloneAs<Rgba32>();
                    byte[] pixels = new byte[img.Width * img.Height * Unsafe.SizeOf<Rgba32>()];
                    img.CopyPixelDataTo(pixels);
                    Array.Copy(pixels, 0, data, 6, pixels.Length);
                    break;
            }
        }

        public static bool IsValidBinaryImage(string path)
        {
            try
            {
                BinaryImage image = new BinaryImage(path);
                var img = image.GetImage();
                if (img == null)
                    return false;
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{ex.Message}, file: {path}");
                return false;
            }
        }
    }
}

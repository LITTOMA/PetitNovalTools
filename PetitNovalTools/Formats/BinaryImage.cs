using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PetitNovalTools.Formats
{
    public enum BinaryPixelFormat : ushort
    {
        Rgba32 = 1,
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
            data = new byte[Width * Height * 4];
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
    }
}

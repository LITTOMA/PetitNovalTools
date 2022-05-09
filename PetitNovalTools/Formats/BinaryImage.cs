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
        Rgb888 = 0,
        Rgba8888,
        Rgb565,
        Rgba4444,
        Rgba5551,
        La44,
        La88,
        L8,
        A8,
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
            PixelFormat = BinaryPixelFormat.Rgba8888;
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
                    case BinaryPixelFormat.Rgb888:
                        return Image.LoadPixelData<Rgb24>(data.Skip(6).ToArray(), Width, Height);
                    case BinaryPixelFormat.Rgba8888:
                        return Image.LoadPixelData<Rgba32>(data.Skip(6).ToArray(), Width, Height);
                    case BinaryPixelFormat.Rgba4444:
                        return Image.LoadPixelData<Rgba4444>(data.Skip(6).ToArray(), Width, Height);
                    case BinaryPixelFormat.Rgb565:
                        return Image.LoadPixelData<Bgr565>(data.Skip(6).ToArray(), Width, Height);
                    case BinaryPixelFormat.Rgba5551:
                        return Image.LoadPixelData<Bgra5551>(data.Skip(6).ToArray(), Width, Height);
                    case BinaryPixelFormat.La88:
                        return Image.LoadPixelData<La16>(data.Skip(6).ToArray(), Width, Height);
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

            byte[] pixels = null;
            switch (PixelFormat)
            {
                default:
                    break;
                case BinaryPixelFormat.Rgba8888:
                    Image<Rgba32> img32 = value.CloneAs<Rgba32>();
                    pixels = new byte[img32.Width * img32.Height * Unsafe.SizeOf<Rgba32>()];
                    img32.CopyPixelDataTo(pixels);
                    Array.Copy(pixels, 0, data, 6, pixels.Length);
                    break;
                case BinaryPixelFormat.Rgb888:
                    var img24 = value.CloneAs<Rgb24>();
                    pixels = new byte[img24.Width * img24.Height * Unsafe.SizeOf<Rgb24>()];
                    img24.CopyPixelDataTo(pixels);
                    Array.Copy(pixels, 0, data, 6, pixels.Length);
                    break;
                case BinaryPixelFormat.Rgb565:
                    var img565 = value.CloneAs<Bgr565>();
                    pixels = new byte[img565.Width * img565.Height * Unsafe.SizeOf<Bgr565>()];
                    img565.CopyPixelDataTo(pixels);
                    Array.Copy(pixels, 0, data, 6, pixels.Length);
                    break;
                case BinaryPixelFormat.Rgba4444:
                    var img4444 = value.CloneAs<Rgba4444>();
                    pixels = new byte[img4444.Width * img4444.Height * Unsafe.SizeOf<Rgba4444>()];
                    img4444.CopyPixelDataTo(pixels);
                    Array.Copy(pixels, 0, data, 6, pixels.Length);
                    break;
                case BinaryPixelFormat.Rgba5551:
                    var img5551 = value.CloneAs<Bgra5551>();
                    pixels = new byte[img5551.Width * img5551.Height * Unsafe.SizeOf<Bgra5551>()];
                    img5551.CopyPixelDataTo(pixels);
                    Array.Copy(pixels, 0, data, 6, pixels.Length);
                    break;
                case BinaryPixelFormat.La88:
                    var img88 = value.CloneAs<La16>();
                    pixels = new byte[img88.Width * img88.Height * Unsafe.SizeOf<La16>()];
                    img88.CopyPixelDataTo(pixels);
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

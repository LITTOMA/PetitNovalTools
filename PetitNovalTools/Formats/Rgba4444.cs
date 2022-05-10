// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Formats;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.PixelFormats
{
    /// <summary>
    /// Packed pixel type containing unsigned normalized values, ranging from 0 to 1, using 4 bits each for x, y, z, and w.
    /// <para>
    /// Ranges from [0, 0, 0, 0] to [1, 1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    public partial struct Rgba4444 : IPixel<Rgba4444>, IPackedVector<ushort>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rgba4444"/> struct.
        /// </summary>
        /// <param name="x">The x-component</param>
        /// <param name="y">The y-component</param>
        /// <param name="z">The z-component</param>
        /// <param name="w">The w-component</param>
        public Rgba4444(float x, float y, float z, float w)
            : this(new Vector4(x, y, z, w))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rgba4444"/> struct.
        /// </summary>
        /// <param name="vector">The vector containing the components for the packed vector.</param>
        public Rgba4444(Vector4 vector) => this.PackedValue = Pack(ref vector);

        /// <inheritdoc/>
        public ushort PackedValue { get; set; }

        /// <summary>
        /// Compares two <see cref="Rgba4444"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Rgba4444"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="Rgba4444"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>

        public static bool operator ==(Rgba4444 left, Rgba4444 right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="Rgba4444"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Rgba4444"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="Rgba4444"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>

        public static bool operator !=(Rgba4444 left, Rgba4444 right) => !left.Equals(right);

        /// <inheritdoc />
        public readonly PixelOperations<Rgba4444> CreatePixelOperations() => new PixelOperations();

        /// <inheritdoc/>

        public void FromScaledVector4(Vector4 vector) => this.FromVector4(vector);

        /// <inheritdoc/>

        public readonly Vector4 ToScaledVector4() => this.ToVector4();

        /// <inheritdoc />

        public void FromVector4(Vector4 vector) => this.PackedValue = Pack(ref vector);

        /// <inheritdoc />

        public readonly Vector4 ToVector4()
        {
            const float Max = 1 / 15F;

            return new Vector4(
                (this.PackedValue >> 0x0C) & 0x0F,
                (this.PackedValue >> 0x08) & 0x0F,
                (this.PackedValue >> 0x04) & 0x0F,
                (this.PackedValue >> 0x00) & 0x0F
                ) * Max;
        }

        /// <inheritdoc />

        public void FromArgb32(Argb32 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />

        public void FromBgra5551(Bgra5551 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />

        public void FromBgr24(Bgr24 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />

        public void FromBgra32(Bgra32 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />

        public void FromAbgr32(Abgr32 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>

        public void FromL8(L8 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>

        public void FromL16(L16 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>

        public void FromLa16(La16 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>

        public void FromLa32(La32 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />

        public void FromRgb24(Rgb24 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />

        public void FromRgba32(Rgba32 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />

        public void ToRgba32(ref Rgba32 dest) => dest.FromScaledVector4(this.ToScaledVector4());

        /// <inheritdoc/>

        public void FromRgb48(Rgb48 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>

        public void FromRgba64(Rgba64 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        public override readonly bool Equals(object obj) => obj is Rgba4444 other && this.Equals(other);

        /// <inheritdoc />

        public readonly bool Equals(Rgba4444 other) => this.PackedValue.Equals(other.PackedValue);

        /// <inheritdoc />
        public override readonly string ToString()
        {
            var vector = this.ToVector4();
            return FormattableString.Invariant($"Rgba4444({vector.Z:#0.##}, {vector.Y:#0.##}, {vector.X:#0.##}, {vector.W:#0.##})");
        }

        /// <inheritdoc />

        public override readonly int GetHashCode() => this.PackedValue.GetHashCode();


        private static ushort Pack(ref Vector4 vector)
        {
            vector = Clamp(vector, Vector4.Zero, Vector4.One);
            return (ushort)(0
                | (((int)Math.Round(vector.W * 15F) & 0x0F) << 0x00)
                | (((int)Math.Round(vector.X * 15F) & 0x0F) << 0x0C)
                | (((int)Math.Round(vector.Y * 15F) & 0x0F) << 0x08)
                | (((int)Math.Round(vector.Z * 15F) & 0x0F) << 0x04)
                );
        }
        public static Vector4 Clamp(Vector4 value, Vector4 min, Vector4 max)
           => Vector4.Min(Vector4.Max(value, min), max);

        /// <summary>
        /// Provides optimized overrides for bulk operations.
        /// </summary>
        internal class PixelOperations : PixelOperations<Rgba4444>
        {
            private static readonly Lazy<PixelTypeInfo> LazyInfo =
                new Lazy<PixelTypeInfo>(() => new PixelTypeInfo(16, PixelAlphaRepresentation.Associated), true);

            /// <inheritdoc />
            public override PixelTypeInfo GetPixelTypeInfo() => LazyInfo.Value;
        }
    }
}
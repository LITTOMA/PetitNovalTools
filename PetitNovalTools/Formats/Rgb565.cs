// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Formats;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.PixelFormats
{
    /// <summary>
    /// Packed pixel type containing unsigned normalized values ranging from 0 to 1.
    /// The x and z components use 5 bits, and the y component uses 6 bits.
    /// <para>
    /// Ranges from [0, 0, 0, 1] to [1, 1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    public partial struct Rgb565 : IPixel<Rgb565>, IPackedVector<ushort>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rgb565"/> struct.
        /// </summary>
        /// <param name="x">The x-component</param>
        /// <param name="y">The y-component</param>
        /// <param name="z">The z-component</param>
        public Rgb565(float x, float y, float z)
            : this(new Vector3(x, y, z))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rgb565"/> struct.
        /// </summary>
        /// <param name="vector">
        /// The vector containing the components for the packed value.
        /// </param>
        public Rgb565(Vector3 vector) => this.PackedValue = Pack(ref vector);

        /// <inheritdoc/>
        public ushort PackedValue { get; set; }

        /// <summary>
        /// Compares two <see cref="Rgb565"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Rgb565"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="Rgb565"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>

        public static bool operator ==(Rgb565 left, Rgb565 right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="Rgb565"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Rgb565"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="Rgb565"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>

        public static bool operator !=(Rgb565 left, Rgb565 right) => !left.Equals(right);

        /// <inheritdoc />
        public readonly PixelOperations<Rgb565> CreatePixelOperations() => new PixelOperations();

        /// <inheritdoc/>

        public void FromScaledVector4(Vector4 vector) => this.FromVector4(vector);

        /// <inheritdoc/>

        public readonly Vector4 ToScaledVector4() => this.ToVector4();

        /// <inheritdoc />

        public void FromVector4(Vector4 vector)
        {
            var vector3 = new Vector3(vector.X, vector.Y, vector.Z);
            this.PackedValue = Pack(ref vector3);
        }

        /// <inheritdoc />

        public readonly Vector4 ToVector4() => new(this.ToVector3(), 1F);

        /// <inheritdoc/>

        public void FromArgb32(Argb32 source) => this.FromVector4(source.ToVector4());

        /// <inheritdoc/>

        public void FromBgra5551(Bgra5551 source) => this.FromVector4(source.ToVector4());

        /// <inheritdoc />

        public void FromBgr24(Bgr24 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>

        public void FromBgra32(Bgra32 source) => this.FromVector4(source.ToVector4());

        /// <inheritdoc/>

        public void FromAbgr32(Abgr32 source) => this.FromVector4(source.ToVector4());

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

        public void FromRgba32(Rgba32 source) => this.FromVector4(source.ToVector4());

        /// <inheritdoc />

        public void ToRgba32(ref Rgba32 dest) => dest.FromScaledVector4(this.ToScaledVector4());

        /// <inheritdoc/>

        public void FromRgb48(Rgb48 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>

        public void FromRgba64(Rgba64 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <summary>
        /// Expands the packed representation into a <see cref="Vector3"/>.
        /// The vector components are typically expanded in least to greatest significance order.
        /// </summary>
        /// <returns>The <see cref="Vector3"/>.</returns>

        public readonly Vector3 ToVector3() => new(
                       ((this.PackedValue >> 11) & 0x1F) * (1F / 31F),
                       ((this.PackedValue >> 5) & 0x3F) * (1F / 63F),
                       (this.PackedValue & 0x1F) * (1F / 31F));

        /// <inheritdoc />
        public override readonly bool Equals(object obj) => obj is Rgb565 other && this.Equals(other);

        /// <inheritdoc />

        public readonly bool Equals(Rgb565 other) => this.PackedValue.Equals(other.PackedValue);

        /// <inheritdoc />
        public override readonly string ToString()
        {
            var vector = this.ToVector3();
            return FormattableString.Invariant($"Rgb565({vector.Z:#0.##}, {vector.Y:#0.##}, {vector.X:#0.##})");
        }

        /// <inheritdoc />

        public override readonly int GetHashCode() => this.PackedValue.GetHashCode();


        private static ushort Pack(ref Vector3 vector)
        {
            vector = Vector3.Clamp(vector, Vector3.Zero, Vector3.One);

            return (ushort)((((int)Math.Round(vector.X * 31F) & 0x1F) << 11)
                   | (((int)Math.Round(vector.Y * 63F) & 0x3F) << 5)
                   | ((int)Math.Round(vector.Z * 31F) & 0x1F));
        }

        internal class PixelOperations : PixelOperations<Rgb565>
        {
            private static readonly Lazy<PixelTypeInfo> LazyInfo =
                new Lazy<PixelTypeInfo>(() => new PixelTypeInfo(16, PixelAlphaRepresentation.None), true);

            /// <inheritdoc />
            public override PixelTypeInfo GetPixelTypeInfo() => LazyInfo.Value;
        }
    }
}
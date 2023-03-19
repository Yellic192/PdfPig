using IccProfile.Parsers;
using System;
using System.Linq;

namespace IccProfile.Tags
{
    /// <summary>
    /// TODO
    /// </summary>
    public class IccLutABType : IIccTagType
    {
        /// <summary>
        /// A to B or B to A
        /// </summary>
        public LutABType Type { get; }

        /// <inheritdoc/>
        public byte[] RawData { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public int NumberOfInputChannels { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public int NumberOfOutputChannels { get; }

        /// <summary>
        /// Offset to first “B” curve
        /// </summary>
        public int OffsetFirstBCurve { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public IccBaseCurveType[] BCurves { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public int OffsetMatrix { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public float[] Matrix { get; }

        /// <summary>
        /// Offset to first “M” curve
        /// </summary>
        public int OffsetFirstMCurve { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public IccBaseCurveType[] MCurves { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public int OffsetClut { get; }

        /// <summary>
        /// Offset to first “A” curve
        /// </summary>
        public int OffsetFirstACurve { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public IccBaseCurveType[] ACurves { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public float[][][] Clut { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public IccLutABType(LutABType type, int numberOfInputChannels, int numberOfOutputChannels,
            int offsetFirstBCurve, IccBaseCurveType[] bCurves,
            int offsetMatrix, float[] matrix,
            int offsetFirstMCurve, IccBaseCurveType[] mCurves,
            int offsetClut,
            int offsetFirstACurve, IccBaseCurveType[] aCurves,
            float[][][] clut,
            byte[] rawData)
        {
            Type = type;
            NumberOfInputChannels = numberOfInputChannels;
            NumberOfOutputChannels = numberOfOutputChannels;
            OffsetFirstBCurve = offsetFirstBCurve;
            BCurves = bCurves;
            OffsetMatrix = offsetMatrix;
            Matrix = matrix;
            OffsetFirstMCurve = offsetFirstMCurve;
            MCurves = mCurves;
            OffsetClut = offsetClut;
            OffsetFirstACurve = offsetFirstACurve;
            ACurves = aCurves;
            Clut = clut;
            RawData = rawData;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IccLutABType Parse(byte[] bytes)
        {
            string typeSignature = IccTagsHelper.GetString(bytes, 0, 4);

            if (typeSignature != "mAB " && typeSignature != "mBA ")
            {
                throw new ArgumentException(nameof(typeSignature));
            }

            LutABType type = typeSignature == "mAB " ? LutABType.AB : LutABType.BA;

            // Reserved, shall be set to 0
            // 4 to 7
            //byte[] reserved = bytes.Skip(4).Take(4).ToArray();

            // Number of Input Channels (i)
            // 8
            byte input = bytes.Skip(8).Take(1).ToArray()[0];

            // Number of Output Channels (o)
            // 9
            byte output = bytes.Skip(9).Take(1).ToArray()[0];

            // Reserved for padding, shall be set to 0
            // 10 to 11
            //byte[] padding = bytes.Skip(10).Take(2).ToArray();

            // Offset to first “B” curve
            // 12 to 15
            uint offsetFirstB = IccTagsHelper.ReadUInt32(bytes.Skip(12).Take(4).ToArray());

            // Offset to matrix
            // 16 to 19
            uint offsetMatrix = IccTagsHelper.ReadUInt32(bytes.Skip(16).Take(4).ToArray());

            // Offset to first “M” curve
            // 20 to 23
            uint offsetFirstM = IccTagsHelper.ReadUInt32(bytes.Skip(20).Take(4).ToArray());

            // Offset to CLUT
            // 24 to 27
            uint offsetClut = IccTagsHelper.ReadUInt32(bytes.Skip(24).Take(4).ToArray());

            // Offset to first “A” curve
            // 28 to 31
            uint offsetFirstA = IccTagsHelper.ReadUInt32(bytes.Skip(28).Take(4).ToArray());

            int offset = 0;
            IccBaseCurveType[] aCurves = null;
            if (offsetFirstA > 0)
            {
                aCurves = new IccBaseCurveType[type == LutABType.AB ? input : output];
                for (byte a = 0; a < aCurves.Length; a++)
                {
                    var curve = IccBaseCurveType.Parse(bytes.Skip((int)offsetFirstA + offset).ToArray());
                    aCurves[a] = curve;
                    offset += AdjustOffset(curve.BytesRead);
                }
            }

            offset = 0;
            IccBaseCurveType[] bCurves = new IccBaseCurveType[type == LutABType.AB ? output : input];
            for (byte b = 0; b < bCurves.Length; b++)
            {
                var curve = IccBaseCurveType.Parse(bytes.Skip((int)offsetFirstB + offset).ToArray());
                bCurves[b] = curve;
                offset += AdjustOffset(curve.BytesRead);
            }

            IccBaseCurveType[] mCurves = null;
            if (offsetFirstM > 0)
            {
                offset = 0;
                mCurves = new IccBaseCurveType[type == LutABType.AB ? output : input];
                for (byte m = 0; m < mCurves.Length; m++)
                {
                    var curve = IccBaseCurveType.Parse(bytes.Skip((int)offsetFirstM + offset).ToArray());
                    mCurves[m] = curve;
                    offset += AdjustOffset(curve.BytesRead);
                }
            }

            float[] matrix = null;
            if (offsetMatrix > 0)
            {
                matrix = new float[3 * 4];
                byte[] matrixData = bytes.Skip((int)offsetMatrix).Take(matrix.Length * 4).ToArray();
                matrix = IccTagsHelper.Reads15Fixed16Array(matrixData);
            }

            // CLUT
            float[][][] clut = null;
            if (offsetClut > 0)
            {
                byte[] clutGridPoint = bytes.Skip((int)offsetClut).Take(input).ToArray();
                byte precision = bytes.Skip((int)offsetClut + 16).Take(1).ToArray()[0];
                //byte[] clutPadding = bytes.Skip((int)offsetClut + 17).Take(3).ToArray();
                byte[] clutData = bytes.Skip((int)offsetClut + 20).ToArray();

                Func<byte[], float> reader;
                if (precision == 1)
                {
                    reader = new Func<byte[], float>(array =>
                    {
                        return IccTagsHelper.ReadUInt8(array) / 255f;
                    });
                }
                else
                {
                    reader = new Func<byte[], float>(array =>
                    {
                        return IccTagsHelper.ReadUInt16(array) / 65_535f;
                    });
                }

                // Below does not seem correct, almost there but not yet
                int l = 0;
                clut = new float[input][][];
                for (byte i = 0; i < clut.Length; i++)
                {
                    float[][] grid = new float[clutGridPoint[i]][];
                    for (int k = 0; k < grid.Length; k++)
                    {
                        float[] oArray = new float[output];
                        for (int o = 0; o < oArray.Length; o++)
                        {
                            oArray[o] = reader(clutData.Skip(l).Take(precision).ToArray());
                            l += precision;
                        }
                        grid[k] = oArray;
                    }
                    clut[i] = grid;
                }
            }

            return new IccLutABType(type,
                input, output,
                (int)offsetFirstB, bCurves,
                (int)offsetMatrix, matrix,
                (int)offsetFirstM, mCurves,
                (int)offsetClut,
                (int)offsetFirstA, aCurves,
                clut,
                bytes); // TODO - only take relevant bytes
        }

        private static int AdjustOffset(int offset)
        {
            // Each curve and processing element shall start on a 4-byte boundary.
            // To achieve this, each item shall be followed by up to three 00h pad bytes as needed.
            return offset + (offset % 4);
        }
    }

    /// <summary>
    /// A to B or B to A
    /// </summary>
    public enum LutABType : byte
    {
        /// <summary>
        /// A to B
        /// </summary>
        AB = 0,

        /// <summary>
        /// B to A
        /// </summary>
        BA = 1
    }
}

using IccProfile.Parsers;
using System;

namespace IccProfile.Tags
{
    /// <summary>
    /// TODO
    /// </summary>
    public class IccBaseLutType : IIccTagType
    {
        /// <inheritdoc/>
        public byte[] RawData { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public int NumberOfInputChannels { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public int NumberOfInputEntries { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public int NumberOfOutputChannels { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public int NumberOfOutputEntries { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public int NumberOfClutPoint { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public float E1 { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public float E2 { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public float E3 { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public float E4 { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public float E5 { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public float E6 { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public float E7 { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public float E8 { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public float E9 { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public float[][] InputTable { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public float[][][] ClutTable { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public float[][] OutputTable { get; }

        /// <summary>
        /// TODO
        /// </summary>
        protected IccBaseLutType(int numberOfInputChannels,
            int numberOfInputEntries,
            int numberOfOutputChannels,
            int numberOfOutputEntries,
            int numberOfClutPoint,
            float e1, float e2, float e3, float e4, float e5,
            float e6, float e7, float e8, float e9, float[][] inputTable, float[][][] clutTable,
            float[][] outputTable, byte[] rawData)
        {
            NumberOfInputChannels = numberOfInputChannels;
            NumberOfInputEntries = numberOfInputEntries;
            NumberOfOutputChannels = numberOfOutputChannels;
            NumberOfOutputEntries = numberOfOutputEntries;
            NumberOfClutPoint = numberOfClutPoint;
            E1 = e1;
            E2 = e2;
            E3 = e3;
            E4 = e4;
            E5 = e5;
            E6 = e6;
            E7 = e7;
            E8 = e8;
            E9 = e9;
            InputTable = inputTable;
            ClutTable = clutTable;
            OutputTable = outputTable;
            RawData = rawData;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static IccBaseLutType Parse(byte[] bytes)
        {
            string typeSignature = IccTagsHelper.GetString(bytes, 0, 4);

            if (typeSignature != "mft1" && typeSignature != "mft2")
            {
                throw new ArgumentException(nameof(typeSignature));
            }

            switch(typeSignature)
            {
                case "mft1":
                    return IccLut8Type.Parse(bytes);
                case "mft2":
                    return IccLut16Type.Parse(bytes);
                default:
                    throw new ArgumentException(nameof(typeSignature));
            }
        }
    }
}

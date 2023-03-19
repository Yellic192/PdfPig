using IccProfile.Parsers;
using System;
using System.Linq;

namespace IccProfile.Tags
{
    /// <summary>
    /// TODO
    /// </summary>
    public class IccLut16Type : IccBaseLutType
    {
        /// <summary>
        /// TODO
        /// </summary>
        protected IccLut16Type(int numberOfInputChannels,
            int numberOfInputEntries,
            int numberOfOutputChannels,
            int numberOfOutputEntries,
            int numberOfClutPoint,
            float e1, float e2, float e3, float e4, float e5, float e6, float e7, float e8, float e9,
            float[][] inputTable, float[][][] clutTable, float[][] outputTable, byte[] rawData)
            : base(numberOfInputChannels, numberOfInputEntries,
                  numberOfOutputChannels, numberOfOutputEntries,
                  numberOfClutPoint,
                  e1, e2, e3, e4, e5, e6, e7, e8, e9,
                  inputTable, clutTable, outputTable, rawData)
        { }

        /// <summary>
        /// TODO
        /// </summary>
        new public static IccLut16Type Parse(byte[] bytes)
        {
            string typeSignature = IccTagsHelper.GetString(bytes, 0, 4);

            if (typeSignature != "mft2")
            {
                throw new ArgumentException(nameof(typeSignature));
            }

            // Reserved, shall be set to 0
            // 4 to 7
            //byte[] reserved = bytes.Skip(4).Take(4).ToArray();

            // Number of Input Channels (i)
            // 8
            byte input = bytes.Skip(8).Take(1).ToArray()[0];

            // Number of Output Channels (o)
            // 9
            byte output = bytes.Skip(9).Take(1).ToArray()[0];

            // Number of CLUT grid points (identical for each side) (g)
            // 10
            byte clutGridPoints = bytes.Skip(10).Take(1).ToArray()[0];

            //byte reserved2 = bytes.Skip(11).Take(1).ToArray()[0];

            // Encoded e1 parameter
            // 12 to 15
            float e1 = IccTagsHelper.Reads15Fixed16Number(bytes.Skip(12).Take(4).ToArray());

            // Encoded e2 parameter
            // 16 to 19
            float e2 = IccTagsHelper.Reads15Fixed16Number(bytes.Skip(16).Take(4).ToArray());

            // Encoded e2 parameter
            // 20 to 23
            float e3 = IccTagsHelper.Reads15Fixed16Number(bytes.Skip(20).Take(4).ToArray());

            // Encoded e2 parameter
            // 24 to 27
            float e4 = IccTagsHelper.Reads15Fixed16Number(bytes.Skip(24).Take(4).ToArray());

            // Encoded e2 parameter
            // 28 to 31
            float e5 = IccTagsHelper.Reads15Fixed16Number(bytes.Skip(28).Take(4).ToArray());

            // Encoded e2 parameter
            // 32 to 35
            float e6 = IccTagsHelper.Reads15Fixed16Number(bytes.Skip(32).Take(4).ToArray());

            // Encoded e2 parameter
            // 36 to 39
            float e7 = IccTagsHelper.Reads15Fixed16Number(bytes.Skip(36).Take(4).ToArray());

            // Encoded e2 parameter
            // 40 to 43
            float e8 = IccTagsHelper.Reads15Fixed16Number(bytes.Skip(40).Take(4).ToArray());

            // Encoded e2 parameter
            // 44 to 47
            float e9 = IccTagsHelper.Reads15Fixed16Number(bytes.Skip(44).Take(4).ToArray());

            // Number of input table entries (n)
            // 48 to 49
            int inputTableEntries = IccTagsHelper.ReadUInt16(bytes.Skip(48).Take(2).ToArray());

            // Number of output table entries (m)
            // 50 to 51
            int outputTableEntries = IccTagsHelper.ReadUInt16(bytes.Skip(50).Take(2).ToArray());

            // Input tables
            // 52 to 51+(2ni)
            int l = 0;
            int inputTableBytesL = 2 * inputTableEntries * input;
            float[][] inputTable = new float[input][];
            var tableBytes = bytes.Skip(52).Take(inputTableBytesL).ToArray();
            for (int i = 0; i < inputTable.Length; i++)
            {
                float[] domain = new float[inputTableEntries];
                for (int j = 0; j < domain.Length; j++)
                {
                    domain[j] = IccTagsHelper.ReadUInt16(tableBytes.Skip(l).Take(2).ToArray()) / 65_535f;
                    l += 2;
                }
                inputTable[i] = domain;
            }

            // CLUT values
            l = 0;
            int clutValuesBytesL = 2 * (int)Math.Pow(clutGridPoints, input) * output;
            tableBytes = bytes.Skip(52 + inputTableBytesL).Take(clutValuesBytesL).ToArray();

            // Below does not seem correct, almost there but not yet
            float[][][] clut = new float[input][][];
            for (byte i = 0; i < clut.Length; i++)
            {
                float[][] grid = new float[clutGridPoints][];
                for (int k = 0; k < grid.Length; k++)
                {
                    float[] oArray = new float[output];
                    for (int o = 0; o < oArray.Length; o++)
                    {
                        oArray[o] = IccTagsHelper.ReadUInt16(tableBytes.Skip(l).Take(2).ToArray()) / 65_535f;
                        l += 2;
                    }
                    grid[k] = oArray;
                }
                clut[i] = grid;
            }

            // Output tables
            l = 0;
            //int outputBytesL = 2 * outputTableEntries * output;
            float[][] outputTable = new float[output][];
            tableBytes = bytes.Skip(52 + inputTableBytesL + clutValuesBytesL).ToArray();
            for (int i = 0; i < outputTable.Length; i++)
            {
                float[] domain = new float[outputTableEntries];
                for (int j = 0; j < domain.Length; j++)
                {
                    domain[j] = IccTagsHelper.ReadUInt16(tableBytes.Skip(l).Take(2).ToArray()) / 65_535f;
                    l += 2;
                }
                outputTable[i] = domain;
            }

            return new IccLut16Type(input, inputTableEntries,
                output, outputTableEntries,
                clutGridPoints,
                e1, e2, e3, e4, e5, e6, e7, e8, e9,
                inputTable, clut, outputTable, bytes);
        }
    }
}

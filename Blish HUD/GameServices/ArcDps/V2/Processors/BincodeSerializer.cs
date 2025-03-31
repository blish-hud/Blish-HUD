using Gw2Sharp.ChatLinks.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.GameServices.ArcDps.V2.Processors {
    public static class BincodeSerializer {
        public static class FloatConverter {
            public static class Float32Converter {
                public static float Convert(BinaryReader reader) => reader.ReadSingle();
            }

            public static class Float64Converter {
                public static double Convert(BinaryReader reader) => reader.ReadDouble();
            }
        }

        public static class IntConverter {
            public static bool UseVarint { get; set; } = true;

            public class VarintEncoding {
                public static readonly VarintEncoding Instance = new VarintEncoding();

                public ulong ConvertUnsigned(BinaryReader reader) {
                    byte firstByte = reader.ReadByte();

                    return firstByte < 251
                        ? firstByte
                        : firstByte == 251
                            ? reader.ReadUInt16()
                            : firstByte == 252
                                                    ? reader.ReadUInt32()
                                                    : firstByte == 253 ? reader.ReadUInt64() : throw new InvalidOperationException("Varint Encoding size was Int128");
                }

                public long Convert(BinaryReader reader) {
                    ulong unsigned = ConvertUnsigned(reader);
                    return UnZigZag(unsigned);
                }

                private long UnZigZag(ulong unsigned) => unsigned == 0 ? 0 : unsigned % 2 == 0 ? (long)(unsigned / 2) : (long)((unsigned + 1) / 2) * -1;
            }

            public static class Int8Converter {
                public static sbyte Convert(BinaryReader reader) => UseVarint ? (sbyte)VarintEncoding.Instance.Convert(reader) : reader.ReadSByte();

                public static byte ConvertUnsigned(BinaryReader reader) => reader.ReadByte();
            }

            public static class Int16Converter {
                public static short Convert(BinaryReader reader) => UseVarint ? (short)VarintEncoding.Instance.Convert(reader) : reader.ReadInt16();

                public static ushort ConvertUnsigned(BinaryReader reader) => UseVarint ? (ushort)VarintEncoding.Instance.ConvertUnsigned(reader) : reader.ReadUInt16();
            }

            public static class Int32Converter {
                public static int Convert(BinaryReader reader) => UseVarint ? (int)VarintEncoding.Instance.Convert(reader) : reader.ReadInt32();

                public static uint ConvertUnsigned(BinaryReader reader) => UseVarint ? (uint)VarintEncoding.Instance.ConvertUnsigned(reader) : reader.ReadUInt32();
            }

            public static class Int64Converter {
                public static long Convert(BinaryReader reader) => UseVarint ? VarintEncoding.Instance.Convert(reader) : reader.ReadInt64();

                public static ulong ConvertUnsigned(BinaryReader reader) => UseVarint ? VarintEncoding.Instance.ConvertUnsigned(reader) : reader.ReadUInt64();
            }

            public static class ISizeConverter {
                public static long Convert(BinaryReader reader) => UseVarint ? VarintEncoding.Instance.Convert(reader) : reader.ReadInt64();
            }

            public static class USizeConverter {
                public static ulong Convert(BinaryReader reader) => UseVarint ? VarintEncoding.Instance.ConvertUnsigned(reader) : reader.ReadUInt64();
            }
        }

        public static class BoolConverter {
            public static bool Convert(BinaryReader reader) => reader.ReadBoolean();
        }

        public static class CollectionConverter {
            public static class ArrayConverter {
                // TODO: Maybe make this more performant in code generation and generate the specific count of 
                public static IEnumerable<T> Convert<T>(BinaryReader binaryReader, Func<BinaryReader, T> converter, int size) {
                    for (int i = 0; i < size; i++) {
                        yield return converter(binaryReader);
                    }
                }
            }

            public static class StringConverter {
                public static string Convert(BinaryReader reader) {
                    ulong size = IntConverter.USizeConverter.Convert(reader);
                    return Encoding.UTF8.GetString(reader.ReadBytes((int)size));
                }
            }

            public static class VariableLengthConverter {
                public static IEnumerable<T> Convert<T>(BinaryReader reader, Func<BinaryReader, T> converter) {
                    int size = (int)IntConverter.USizeConverter.Convert(reader);

                    for (int i = 0; i < size; i++) {
                        yield return converter(reader);
                    }
                }
            }
        }
    }

    public class BincodeBinaryReader : BinaryReader {
        public Converter Convert { get; set; }

        public BincodeBinaryReader(Stream input) : base(input) {
            this.Convert = new Converter(this);
        }

        public BincodeBinaryReader(Stream input, Encoding encoding) : base(input, encoding) {
            this.Convert = new Converter(this);
        }

        public BincodeBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen) {
            this.Convert = new Converter(this);
        }

        public class Converter {
            private readonly BinaryReader reader;

            internal Converter(BinaryReader reader) {
                this.reader = reader;
            }

            public float ParseFloat() => BincodeSerializer.FloatConverter.Float32Converter.Convert(reader);

            public double ParseDouble() => BincodeSerializer.FloatConverter.Float64Converter.Convert(reader);

            public sbyte ParseSByte() => BincodeSerializer.IntConverter.Int8Converter.Convert(reader);

            public byte ParseByte() => BincodeSerializer.IntConverter.Int8Converter.ConvertUnsigned(reader);

            public short ParseShort() => BincodeSerializer.IntConverter.Int16Converter.Convert(reader);

            public ushort ParseUShort() => BincodeSerializer.IntConverter.Int16Converter.ConvertUnsigned(reader);

            public int ParseInt() => BincodeSerializer.IntConverter.Int32Converter.Convert(reader);

            public uint ParseUInt() => BincodeSerializer.IntConverter.Int32Converter.ConvertUnsigned(reader);

            public long ParseLong() => BincodeSerializer.IntConverter.Int64Converter.Convert(reader);

            public ulong ParseULong() => BincodeSerializer.IntConverter.Int64Converter.ConvertUnsigned(reader);

            public string ParseString() => BincodeSerializer.CollectionConverter.StringConverter.Convert(reader);

            public T[] ParseArray<T>(int size, Func<BinaryReader, T> creationFunc) => BincodeSerializer.CollectionConverter.ArrayConverter.Convert(reader, creationFunc, size).ToArray();

            public List<T> ParseList<T>(Func<BinaryReader, T> creationFunc) => BincodeSerializer.CollectionConverter.VariableLengthConverter.Convert(reader, creationFunc).ToList();

            public long ParseISize() => BincodeSerializer.IntConverter.ISizeConverter.Convert(reader);

            public ulong ParseUSize() => BincodeSerializer.IntConverter.USizeConverter.Convert(reader);

            public bool ParseBool() => BincodeSerializer.BoolConverter.Convert(reader);
        }
    }
}

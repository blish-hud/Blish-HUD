using Gw2Sharp.ChatLinks.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.GameServices.ArcDps.V2 {
    public static class BincodeSerializer {
        public static class FloatConverter {
            public static class Float32Converter {
                public static float Convert(BinaryReader reader) {
                    return reader.ReadSingle();
                }
            }

            public static class Float64Converter {
                public static double Convert(BinaryReader reader) {
                    return reader.ReadDouble();
                }
            }
        }

        public static class IntConverter {
            public static bool UseVarint { get; set; } = true;

            public class VarintEncoding {
                public static readonly VarintEncoding Instance = new VarintEncoding();

                public ulong ConvertUnsigned(BinaryReader reader) {
                    var firstByte = reader.ReadByte();

                    if (firstByte < 251) {
                        return firstByte;
                    } else if (firstByte == 251) {
                        return reader.ReadUInt16();
                    } else if (firstByte == 252) {
                        return reader.ReadUInt32();
                    } else if (firstByte == 253) {
                        return reader.ReadUInt64();
                    } else {
                        throw new InvalidOperationException("Varint Encoding size was Int128");
                    }
                }

                public long Convert(BinaryReader reader) {
                    var unsigned = ConvertUnsigned(reader);
                    return UnZigZag(unsigned);
                }

                private long UnZigZag(ulong unsigned) {
                    if (unsigned == 0) {
                        return 0;
                    } else if (unsigned % 2 == 0) {
                        return (long)(unsigned / 2);
                    } else {
                        return (long)((unsigned + 1) / 2) * -1;
                    }
                }
            }

            public static class Int8Converter {
                public static sbyte Convert(BinaryReader reader) {
                    if (UseVarint) {
                        return (sbyte)VarintEncoding.Instance.Convert(reader);
                    }
                    return reader.ReadSByte();
                }

                public static byte ConvertUnsigned(BinaryReader reader) {
                    if (UseVarint) {
                        return (byte)VarintEncoding.Instance.ConvertUnsigned(reader);
                    }
                    return reader.ReadByte();
                }
            }

            public static class Int16Converter {
                public static short Convert(BinaryReader reader) {
                    if (UseVarint) {
                        return (short)VarintEncoding.Instance.Convert(reader);
                    }
                    return reader.ReadInt16();
                }

                public static ushort ConvertUnsigned(BinaryReader reader) {
                    if (UseVarint) {
                        return (ushort)VarintEncoding.Instance.ConvertUnsigned(reader);
                    }
                    return reader.ReadUInt16();
                }
            }

            public static class Int32Converter {
                public static int Convert(BinaryReader reader) {
                    if (UseVarint) {
                        return (int)VarintEncoding.Instance.Convert(reader);
                    }
                    return reader.ReadInt32();
                }

                public static uint ConvertUnsigned(BinaryReader reader) {
                    if (UseVarint) {
                        return (uint)VarintEncoding.Instance.ConvertUnsigned(reader);
                    }
                    return reader.ReadUInt32();
                }
            }

            public static class Int64Converter {
                public static long Convert(BinaryReader reader) {
                    if (UseVarint) {
                        return VarintEncoding.Instance.Convert(reader);
                    }
                    return reader.ReadInt64();
                }

                public static ulong ConvertUnsigned(BinaryReader reader) {
                    if (UseVarint) {
                        return VarintEncoding.Instance.ConvertUnsigned(reader);
                    }
                    return reader.ReadUInt64();
                }
            }

            public static class ISizeConverter {
                public static long Convert(BinaryReader reader) {
                    if (UseVarint) {
                        return VarintEncoding.Instance.Convert(reader);
                    }
                    return reader.ReadInt64();
                }
            }

            public static class USizeConverter {
                public static ulong Convert(BinaryReader reader) {
                    if (UseVarint) {
                        return VarintEncoding.Instance.ConvertUnsigned(reader);
                    }
                    return reader.ReadUInt64();
                }
            }
        }

        public static class BoolConverter {
            public static bool Convert(BinaryReader reader) {
                return reader.ReadBoolean();
            }
        }

        public static class CollectionConverter {
            public static class ArrayConverter {
                // TODO: Maybe make this more performant in code generation and generate the specific count of 
                public static IEnumerable<T> Convert<T>(BinaryReader binaryReader, Func<BinaryReader, T> converter, int size) {
                    for (var i = 0; i < size; i++) {
                        yield return converter(binaryReader);
                    }
                }
            }

            public static class StringConverter {
                public static string Convert(BinaryReader reader) {
                    var size = IntConverter.USizeConverter.Convert(reader);
                    return Encoding.UTF8.GetString(reader.ReadBytes((int)size));
                }
            }

            public static class VariableLengthConverter {
                public static IEnumerable<T> Convert<T>(BinaryReader reader, Func<BinaryReader, T> converter) {
                    var size = (int)IntConverter.USizeConverter.Convert(reader);

                    for (var i = 0; i < size; i++) {
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

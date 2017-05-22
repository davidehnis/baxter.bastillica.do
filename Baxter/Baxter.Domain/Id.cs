using System;
using System.Security.Cryptography;
using System.Text;

namespace Baxter.Domain
{
    //<summary>A slightly modified version of the unique id GUID construct</summary>
    public struct Id
    {
        #region Private Fields
        public const int Version = 5;
        private static RandomNumberGenerator _fastRng;
        private static RandomNumberGenerator _rng;
        private static object _rngAccess = new object();
        private int _a; //_timeLow;
        private short _b; //_timeMid;
        private short _c; //_timeHighAndVersion;
        private byte _d; //_clockSeqHiAndReserved;
        private byte _e; //_clockSeqLow;
        private byte _f; //_node0;
        private byte _g; //_node1;
        private byte _h; //_node2;
        private byte _i; //_node3;
        private byte _j; //_node4;
        private byte _k; //_node5;

        private enum Format
        {
            N, // 00000000000000000000000000000000
            D, // 00000000-0000-0000-0000-000000000000
            B, // {00000000-0000-0000-0000-000000000000}
            P, // (00000000-0000-0000-0000-000000000000)
            X, // {0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}}
        }

        #endregion Private Fields

        #region Constructors
        public Id(byte[] b)
        {
            CheckArray(b, 16);
            _a = BitConverter.ToInt32(b, 0);
            _b = BitConverter.ToInt16(b, 4);
            _c = BitConverter.ToInt16(b, 6);
            _d = b[8];
            _e = b[9];
            _f = b[10];
            _g = b[11];
            _h = b[12];
            _i = b[13];
            _j = b[14];
            _k = b[15];
        }

        //<summary>Initialize with a string Guid value</summary>
        public Id(string g)
        {
            CheckNull(g);
            g = g.Trim();
            var parser = new UniqueIdParser(g);

            Id guid;
            if (!parser.Parse(out guid))
                throw CreateFormatException(g);

            this = guid;
        }

        //<summary>Initialize with the numeric components</summary>
        public Id(int a, short b, short c, byte[] d)
        {
            CheckArray(d, 8);
            _a = (int)a;
            _b = (short)b;
            _c = (short)c;
            _d = d[0];
            _e = d[1];
            _f = d[2];
            _g = d[3];
            _h = d[4];
            _i = d[5];
            _j = d[6];
            _k = d[7];
        }

        //<summary>Initialize with the explicit numeric components</summary>
        public Id(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
        {
            _a = a;
            _b = b;
            _c = c;
            _d = d;
            _e = e;
            _f = f;
            _g = g;
            _h = h;
            _i = i;
            _j = j;
            _k = k;
        }

        //<summary>A static method that creates a new Id</summary>
        public static Id NewId()
        {
            byte[] b = new byte[16];
            // thread-safe access to the prng
            lock (_rngAccess)
            {
                if (_rng == null)
                    _rng = RandomNumberGenerator.Create();
                _rng.GetBytes(b);
            }

            Id res = new Id(b);
            res._d = (byte)((res._d & 0x3fu) | 0x80u);
            res._c = (short)((res._c & 0x0fffu) | 0x4000u);

            return res;
        }

        //<summary>A static exception helper method</summary>
        private static Exception CreateFormatException(string s)
        {
            return new FormatException(string.Format("Invalid Id format: {0}", s));
        }
        #endregion Constructors

        #region Public Fields

        //<summary>Aids in the ability to create an "empty" id</summary>
        public static readonly Id Empty = new Id(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        #endregion Public Fields

        #region Public Methods
        public static Id Create(Id cls, string name)
        {
            // Always assume UTF8
            var nameBytes = Encoding.UTF8.GetBytes(name);

            // convert the namespace UUID to network order
            var namespaceBytes = cls.ToByteArray();
            SwapByteOrder(namespaceBytes);

            // comput the hash of the name space ID concatenated with the name (step 4)
            byte[] hash;
            using (HashAlgorithm algorithm = SHA1.Create())
            {
                algorithm.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, null, 0);
                algorithm.TransformFinalBlock(nameBytes, 0, nameBytes.Length);
                hash = algorithm.Hash;
            }

            var id = new byte[16];
            Array.Copy(hash, 0, id, 0, 16);
            id[6] = (byte)((id[6] & 0x0F) | (Version << 4));
            id[8] = (byte)((id[8] & 0x3F) | 0x80);

            SwapByteOrder(id);
            return new Id(id);
        }

        //<summary>A static helper to create an idea from other Ids</summary>
        public static Id Create(Id one, Id two, Id three)
        {
            return Create(one, two, three, Id.NewId());
        }

        //<summary>A static helper to create an idea from other Ids</summary>
        public static Id Create(Id one, Id two, Id three, Id four)
        {
            const int BYTECOUNT = 16;
            byte[] destByte = new byte[BYTECOUNT];
            byte[] id1Byte = one.ToByteArray();
            byte[] id2Byte = two.ToByteArray();
            byte[] id3Byte = three.ToByteArray();
            byte[] id4Byte = four.ToByteArray();

            for (int i = 0; i < BYTECOUNT; i++)
            {
                destByte[i] = (byte)(id1Byte[i] ^ id2Byte[i] ^ id3Byte[i] ^ id4Byte[i]);
            }
            return new Id(destByte);
        }

        //<summary>A thread-safe (just in case) static helper to create a new Id</summary>
        public static Id NewGuid()
        {
            byte[] b = new byte[16];

            // thread-safe access to the prng
            lock (_rngAccess)
            {
                if (_rng == null)
                    _rng = RandomNumberGenerator.Create();
                _rng.GetBytes(b);
            }

            Id res = new Id(b);
            res._d = (byte)((res._d & 0x3fu) | 0x80u);
            res._c = (short)((res._c & 0x0fffu) | 0x4000u);

            return res;
        }

        //<summary>The not-equal operator overload</summary>
        public static bool operator !=(Id a, Id b)
        {
            return !(a.Equals(b));
        }

        //<summary>The equal operator overload</summary>
        public static bool operator ==(Id a, Id b)
        {
            return a.Equals(b);
        }

        //<summary>Provides typical parsing from string to Id</summary>
        public static Id Parse(string input)
        {
            Id guid;
            if (!TryParse(input, out guid))
                throw CreateFormatException(input);

            return guid;
        }

        //<summary>A more stringent version of Parse.  Will throw exception if parsing is not successful</summary>
        public static Id ParseExact(string input, string format)
        {
            Id guid;
            if (!TryParseExact(input, format, out guid))
                throw CreateFormatException(input);

            return guid;
        }

        //<summary>Will return false if parsing could not perform</summary>
        public static bool TryParse(string input, out Id result)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            var parser = new UniqueIdParser(input);
            return parser.Parse(out result);
        }

        //<summary>A more stringent version of Parse.  Will throw exception if parsing is not successful</summary>
        public static bool TryParseExact(string input, string format, out Id result)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (format == null)
                throw new ArgumentNullException("format");

            var parser = new UniqueIdParser(input);
            return parser.Parse(ParseFormat(format), out result);
        }

        //<summary>Equatable helper method to compare contents</summary>
        public int CompareTo(object value)
        {
            if (value == null)
                return 1;

            if (!(value is Guid))
            {
                throw new ArgumentException("value", "Argument of System.Guid.CompareTo should be a Guid.");
            }

            return CompareTo((Guid)value);
        }

        //<summary>Equatable helper method to compare contents</summary>
        public int CompareTo(Id value)
        {
            if (_a != value._a)
            {
                return Compare(_a, value._a);
            }
            else if (_b != value._b)
            {
                return Compare(_b, value._b);
            }
            else if (_c != value._c)
            {
                return Compare(_c, value._c);
            }
            else if (_d != value._d)
            {
                return Compare(_d, value._d);
            }
            else if (_e != value._e)
            {
                return Compare(_e, value._e);
            }
            else if (_f != value._f)
            {
                return Compare(_f, value._f);
            }
            else if (_g != value._g)
            {
                return Compare(_g, value._g);
            }
            else if (_h != value._h)
            {
                return Compare(_h, value._h);
            }
            else if (_i != value._i)
            {
                return Compare(_i, value._i);
            }
            else if (_j != value._j)
            {
                return Compare(_j, value._j);
            }
            else if (_k != value._k)
            {
                return Compare(_k, value._k);
            }
            return 0;
        }

        //<summary>Equatable helper method to compare contents</summary>
        public override bool Equals(object o)
        {
            if (o is Guid)
                return CompareTo((Guid)o) == 0;
            return false;
        }

        //<summary>Equatable helper method to compare contents</summary>
        public bool Equals(Guid g)
        {
            return CompareTo(g) == 0;
        }

        //<summary>Returns an int for this object's unique hash code</summary>
        public override int GetHashCode()
        {
            int res;

            res = (int)_a;
            res = res ^ ((int)_b << 16 | _c);
            res = res ^ ((int)_d << 24);
            res = res ^ ((int)_e << 16);
            res = res ^ ((int)_f << 8);
            res = res ^ ((int)_g);
            res = res ^ ((int)_h << 24);
            res = res ^ ((int)_i << 16);
            res = res ^ ((int)_j << 8);
            res = res ^ ((int)_k);

            return res;
        }

        //<summary>Converts this Id to a Byte Array</summary>
        public byte[] ToByteArray()
        {
            byte[] res = new byte[16];
            byte[] tmp;
            int d = 0;
            int s;

            tmp = BitConverter.GetBytes(_a);
            for (s = 0; s < 4; ++s)
            {
                res[d++] = tmp[s];
            }

            tmp = BitConverter.GetBytes(_b);
            for (s = 0; s < 2; ++s)
            {
                res[d++] = tmp[s];
            }

            tmp = BitConverter.GetBytes(_c);
            for (s = 0; s < 2; ++s)
            {
                res[d++] = tmp[s];
            }

            res[8] = _d;
            res[9] = _e;
            res[10] = _f;
            res[11] = _g;
            res[12] = _h;
            res[13] = _i;
            res[14] = _j;
            res[15] = _k;

            return res;
        }

        //<summary>Converts this Id to a parsable string</summary>
        public override string ToString()
        {
            return BaseToString(true, false, false);
        }

        //<summary>Converts this Id to a parsable string using a format</summary>
        public string ToString(string format)
        {
            bool h = true;
            bool p = false;
            bool b = false;

            if (format != null)
            {
                string f = format.ToLowerInvariant();

                if (f == "b")
                {
                    b = true;
                }
                else if (f == "p")
                {
                    p = true;
                }
                else if (f == "n")
                {
                    h = false;
                }
                else if (f != "d" && f != String.Empty)
                {
                    throw new FormatException("Argument to Guid.ToString(string format) should be \"b\", \"B\", \"d\", \"D\", \"n\", \"N\", \"p\" or \"P\"");
                }
            }

            return BaseToString(h, p, b);
        }

        //<summary>Converts this Id to a parsable string, using a FormatProvider (for cultural sensitivity)</summary>
        public string ToString(string format, IFormatProvider provider)
        {
            return ToString(format);
        }
        #endregion Public Methods

        #region Internal Methods
        //<summary>Used in the model builder</summary>
        internal static byte[] FastNewIdArray()
        {
            byte[] guid = new byte[16];

            // thread-safe access to the prng
            lock (_rngAccess)
            {
                if (_rng != null)
                    _fastRng = _rng;

                if (_fastRng == null)
                    _fastRng = new RNGCryptoServiceProvider();
                _fastRng.GetBytes(guid);
            }

            guid[8] = (byte)((guid[8] & 0x3f) | 0x80);
            guid[7] = (byte)((guid[7] & 0x0f) | 0x40);

            return guid;
        }
        internal static void SwapByteOrder(byte[] guid)
        {
            SwapBytes(guid, 0, 3);
            SwapBytes(guid, 1, 2);
            SwapBytes(guid, 4, 5);
            SwapBytes(guid, 6, 7);
        }
        #endregion Internal Methods

        #region Private Methods
        private static void AppendByte(StringBuilder builder, byte value)
        {
            builder.Append(ToHex((value >> 4) & 0xf));
            builder.Append(ToHex(value & 0xf));
        }
        private static void AppendInt(StringBuilder builder, int value)
        {
            builder.Append(ToHex((value >> 28) & 0xf));
            builder.Append(ToHex((value >> 24) & 0xf));
            builder.Append(ToHex((value >> 20) & 0xf));
            builder.Append(ToHex((value >> 16) & 0xf));
            builder.Append(ToHex((value >> 12) & 0xf));
            builder.Append(ToHex((value >> 8) & 0xf));
            builder.Append(ToHex((value >> 4) & 0xf));
            builder.Append(ToHex(value & 0xf));
        }
        private static void AppendShort(StringBuilder builder, short value)
        {
            builder.Append(ToHex((value >> 12) & 0xf));
            builder.Append(ToHex((value >> 8) & 0xf));
            builder.Append(ToHex((value >> 4) & 0xf));
            builder.Append(ToHex(value & 0xf));
        }
        private static void CheckArray(byte[] o, int l)
        {
            CheckNull(o);
            CheckLength(o, l);
        }
        private static void CheckLength(byte[] o, int l)
        {
            if (o.Length != l)
            {
                throw new ArgumentException(String.Format("Array should be exactly {0} bytes long.", 1));
            }
        }
        private static void CheckNull(object o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("Value cannot be null.");
            }
        }
        private static int Compare(int x, int y)
        {
            if (x < y)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
        private static Format ParseFormat(string format)
        {
            if (format.Length != 1)
                throw new ArgumentException("Wrong format");

            switch (format[0])
            {
                case 'N':
                    return Format.N;

                case 'D':
                    return Format.D;

                case 'B':
                    return Format.B;

                case 'P':
                    return Format.P;

                case 'X':
                    return Format.X;
            }

            throw new ArgumentException("Wrong format");
        }
        private static void SwapBytes(byte[] guid, int left, int right)
        {
            var temp = guid[left];
            guid[left] = guid[right];
            guid[right] = temp;
        }
        private static char ToHex(int b)
        {
            return (char)((b < 0xA) ? ('0' + b) : ('a' + b - 0xA));
        }
        private string BaseToString(bool h, bool p, bool b)
        {
            StringBuilder res = new StringBuilder(40);

            if (p)
            {
                res.Append('(');
            }
            else if (b)
            {
                res.Append('{');
            }

            AppendInt(res, _a);
            if (h)
            {
                res.Append('-');
            }
            AppendShort(res, _b);
            if (h)
            {
                res.Append('-');
            }
            AppendShort(res, _c);
            if (h)
            {
                res.Append('-');
            }

            AppendByte(res, _d);
            AppendByte(res, _e);

            if (h)
            {
                res.Append('-');
            }

            AppendByte(res, _f);
            AppendByte(res, _g);
            AppendByte(res, _h);
            AppendByte(res, _i);
            AppendByte(res, _j);
            AppendByte(res, _k);

            if (p)
            {
                res.Append(')');
            }
            else if (b)
            {
                res.Append('}');
            }

            return res.ToString();
        }
        #endregion Private Methods

        #region Private Classes

        //<summary>Internal, private class to use for aid in parsing an Id</summary>
        private class UniqueIdParser
        {
            #region Private Fields
            private int _cur;
            private int _length;
            private string _src;
            #endregion Private Fields

            #region Public Constructors
            public UniqueIdParser(string src)
            {
                _src = src;
                Reset();
            }
            #endregion Public Constructors

            #region Private Properties
            private bool Eof
            {
                get { return _cur >= _length; }
            }
            #endregion Private Properties

            #region Public Methods
            public bool Parse(Format format, out Id guid)
            {
                if (format == Format.X)
                    return TryParseX(out guid);

                return TryParseNDBP(format, out guid);
            }
            public bool Parse(out Id guid)
            {
                if (TryParseNDBP(Format.N, out guid))
                    return true;

                Reset();
                if (TryParseNDBP(Format.D, out guid))
                    return true;

                Reset();
                if (TryParseNDBP(Format.B, out guid))
                    return true;

                Reset();
                if (TryParseNDBP(Format.P, out guid))
                    return true;

                Reset();
                return TryParseX(out guid);
            }
            #endregion Public Methods

            #region Private Methods
            private static bool HasHyphen(Format format)
            {
                switch (format)
                {
                    case Format.D:
                    case Format.B:
                    case Format.P:
                        return true;

                    default:
                        return false;
                }
            }
            private bool ParseChar(char c)
            {
                if (!Eof && _src[_cur] == c)
                {
                    _cur++;
                    return true;
                }

                return false;
            }
            private bool ParseHex(int length, bool strict, out ulong res)
            {
                res = 0;

                for (int i = 0; i < length; i++)
                {
                    if (Eof)
                        return !(strict && (i + 1 != length));

                    char c = Char.ToLowerInvariant(_src[_cur]);
                    if (Char.IsDigit(c))
                    {
                        res = res * 16 + c - '0';
                        _cur++;
                    }
                    else if (c >= 'a' && c <= 'f')
                    {
                        res = res * 16 + c - 'a' + 10;
                        _cur++;
                    }
                    else
                    {
                        if (!strict)
                            return true;

                        return !(strict && (i + 1 != length));
                    }
                }

                return true;
            }
            private bool ParseHexPrefix()
            {
                if (!ParseChar('0'))
                    return false;

                return ParseChar('x');
            }
            private void Reset()
            {
                _cur = 0;
                _length = _src.Length;
            }
            private bool TryParseNDBP(Format format, out Id guid)
            {
                ulong a, b, c;
                guid = new Id();

                if (format == Format.B && !ParseChar('{'))
                    return false;

                if (format == Format.P && !ParseChar('('))
                    return false;

                if (!ParseHex(8, true, out a))
                    return false;

                var has_hyphen = HasHyphen(format);

                if (has_hyphen && !ParseChar('-'))
                    return false;

                if (!ParseHex(4, true, out b))
                    return false;

                if (has_hyphen && !ParseChar('-'))
                    return false;

                if (!ParseHex(4, true, out c))
                    return false;

                if (has_hyphen && !ParseChar('-'))
                    return false;

                var d = new byte[8];
                for (int i = 0; i < d.Length; i++)
                {
                    ulong dd;
                    if (!ParseHex(2, true, out dd))
                        return false;

                    if (i == 1 && has_hyphen && !ParseChar('-'))
                        return false;

                    d[i] = (byte)dd;
                }

                if (format == Format.B && !ParseChar('}'))
                    return false;

                if (format == Format.P && !ParseChar(')'))
                    return false;

                if (!Eof)
                    return false;

                guid = new Id((int)a, (short)b, (short)c, d);
                return true;
            }

            private bool TryParseX(out Id guid)
            {
                ulong a, b, c;
                guid = new Id();

                if (!(ParseChar('{')
                    && ParseHexPrefix()
                    && ParseHex(8, false, out a)
                    && ParseChar(',')
                    && ParseHexPrefix()
                    && ParseHex(4, false, out b)
                    && ParseChar(',')
                    && ParseHexPrefix()
                    && ParseHex(4, false, out c)
                    && ParseChar(',')
                    && ParseChar('{')))
                {
                    return false;
                }

                var d = new byte[8];
                for (int i = 0; i < d.Length; ++i)
                {
                    ulong dd;

                    if (!(ParseHexPrefix() && ParseHex(2, false, out dd)))
                        return false;

                    d[i] = (byte)dd;

                    if (i != 7 && !ParseChar(','))
                        return false;
                }

                if (!(ParseChar('}') && ParseChar('}')))
                    return false;

                if (!Eof)
                    return false;

                guid = new Id((int)a, (short)b, (short)c, d);
                return true;
            }
            #endregion Private Methods
        }

        #endregion Private Classes
    }
}
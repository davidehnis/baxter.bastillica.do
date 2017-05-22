using System;

namespace Baxter.Domain
{
    public static class ValueExtension
    {
        #region Public Methods
        public static Value Value(this object wrapped)
        {
            return new Value(wrapped);
        }
        #endregion Public Methods
    }

    //<summary>Encapsulated data/object</summary>
    public class Value : IEquatable<Value>
    {
        #region Public Properties
        public Key Key { get; protected set; }

        public virtual object Raw { get; protected set; }

        public Type Type { get; protected set; }
        #endregion Public Properties

        #region Public Constructors
        public Value()
        {
            Key = new Key(Id.NewId());
        }
        public Value(Value data)
        {
            Type = data.Type;
            Raw = data.Raw;
        }
        public Value(Node data)
        {
            Type = typeof(Node);
            Raw = data;
        }
        public Value(string data)
        {
            Raw = data;
            Type = typeof(string);
        }
        public Value(object data)
        {
            Raw = data;
            Type = data.GetType();
            Key = new Key(Id.NewId());
        }
        public Value(Key data)
        {
            Key = new Key(Id.NewId());
            Raw = data;
            Type = typeof(Key);
        }
        #endregion Public Constructors

        #region Public Methods
        public static Value Empty()
        {
            return new Value(string.Empty);
        }
        static public implicit operator int(Value value)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return 0;
            }
        }
        static public implicit operator Value(string value)
        {
            return value.Value();
        }
        static public implicit operator Value(int value)
        {
            return value.Value();
        }
        public static bool Invalid(Value value)
        {
            if (ReferenceEquals(value, null))
            {
                return true;
            }

            if (value.Raw == null)
            {
                return true;
            }

            if (value.Type == typeof(string) && value.Raw.ToString() == string.Empty)
            {
                return true;
            }

            return false;
        }
        public static bool IsNull(Value value)
        {
            return ReferenceEquals(value, null);
        }
        public static bool IsNumber(Value value)
        {
            Type type = value.Type;
            return type == typeof(decimal) ||
                   type == typeof(double) ||
                   type == typeof(ulong) ||
                   type == typeof(long) ||
                   type == typeof(short) ||
                   type == typeof(ushort) ||
                   type == typeof(int) ||
                   type == typeof(uint) ||
                   type == typeof(float);
        }
        public static bool operator !=(Value left, int right)
        {
            return !(left == right);
        }
        public static bool operator !=(Value left, object right)
        {
            return !(left == right);
        }
        public static bool operator !=(Value left, Value right)
        {
            return !(left.Raw == right.Raw);
        }
        public static Value operator +(Value left, Value right)
        {
            double result = 0;
            if (IsNumber(left) && IsNumber(right))
            {
                var cleft = Convert.ToDouble(Convert.ChangeType(left.Raw, left.Type));
                var cright = Convert.ToDouble(Convert.ChangeType(right.Raw, right.Type));
                result = cleft + cright;
            }

            return new Value(result);
        }
        public static bool operator ==(Value left, Value right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null))
            {
                return false;
            }

            return left.Equals(right);
        }
        public static bool operator ==(Value left, int right)
        {
            int result = Cast(left, 0);

            return result == right;
        }
        public static bool operator ==(Value left, object right)
        {
            int result = Cast(left, 0);

            return result == (int)right;
        }
        public bool Equals(Value obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            if (Raw.ToString() == obj.Raw.ToString())
            {
                return true;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return false;
        }
        public override int GetHashCode()
        {
            return string.Format
                ("{0}-{1}", Raw, Key.GetHashCode(), Type).GetHashCode();
        }
        public new Type GetType()
        {
            return Type;
        }
        public override string ToString()
        {
            string result = string.Empty;
            if (Type == typeof(Node))
            {
                var node = (Node)Raw;
                result = node.ToString();
            }
            else
            {
                result = Raw.ToString();
            }

            return result;
        }

        public bool Valid()
        {
            return Raw != (object)string.Empty;
        }
        #endregion Public Methods

        #region Public Methods
        public static U Cast<U>(Value s, U defaultValue)
        {
            return Cast(s.ToString(), defaultValue);
        }

        public static U Cast<U>(int s, U defaultValue)
        {
            return Cast(s.ToString(), defaultValue);
        }

        public static U Cast<U>(string s, U defaultValue)
        {
            if (string.IsNullOrEmpty(s))
                return defaultValue;
            return (U)Convert.ChangeType(s, typeof(U));
        }

        public static bool IsNumber(object num)
        {
            Type type = num.GetType();
            return type == typeof(decimal) ||
                   type == typeof(double) ||
                   type == typeof(ulong) ||
                   type == typeof(long) ||
                   type == typeof(short) ||
                   type == typeof(ushort) ||
                   type == typeof(int) ||
                   type == typeof(uint) ||
                   type == typeof(float);
        }

        public virtual TypeCode GetTypeCode()
        {
            Type type = Raw.GetType();
            return Type.GetTypeCode(type);
        }
        public bool Is(Type type)
        {
            try
            {
                var converted = Convert.ChangeType(Raw, type);
                return converted != null;
            }
            catch
            {
                return false;
            }
        }
        public bool ToBoolean(IFormatProvider provider)
        {
            return (bool)Convert.ChangeType(Raw, Type);
        }
        public bool ToBoolean()
        {
            return (bool)Convert.ChangeType(Raw, Type);
        }
        public byte ToByte(IFormatProvider provider)
        {
            return (byte)Convert.ChangeType(Raw, Type);
        }
        public byte ToByte()
        {
            return (byte)Convert.ChangeType(Raw, Type);
        }
        public char ToChar(IFormatProvider provider)
        {
            return (char)Convert.ChangeType(Raw, Type);
        }
        public char ToChar()
        {
            return (char)Convert.ChangeType(Raw, Type);
        }
        public DateTime ToDateTime(IFormatProvider provider)
        {
            return (DateTime)Convert.ChangeType(Raw, Type);
        }
        public DateTime ToDateTime()
        {
            return (DateTime)Convert.ChangeType(Raw, Type);
        }
        public decimal ToDecimal(IFormatProvider provider)
        {
            if (IsNumber(Raw))
            {
                return (decimal)Convert.ChangeType(Raw, typeof(decimal));
            }

            return 0;
        }
        public decimal ToDecimal()
        {
            if (IsNumber(Raw))
            {
                return (decimal)Convert.ChangeType(Raw, typeof(decimal));
            }

            return 0;
        }
        public double ToDouble(IFormatProvider provider)
        {
            if (IsNumber(Raw))
            {
                return (double)Convert.ChangeType(Raw, typeof(double));
            }

            return 0;
        }
        public double ToDouble()
        {
            if (IsNumber(Raw))
            {
                return (double)Convert.ChangeType(Raw, typeof(double));
            }

            return 0;
        }
        public short ToInt16(IFormatProvider provider)
        {
            if (IsNumber(Raw))
            {
                return (short)Convert.ChangeType(Raw, typeof(short));
            }

            return 0;
        }
        public short ToInt16()
        {
            if (IsNumber(Raw))
            {
                return (short)Convert.ChangeType(Raw, typeof(short));
            }

            return 0;
        }
        public int ToInt32(IFormatProvider provider)
        {
            if (IsNumber(Raw))
            {
                return (int)Convert.ChangeType(Raw, typeof(int));
            }

            return 0;
        }
        public int ToInt32()
        {
            if (IsNumber(Raw))
            {
                return (int)Convert.ChangeType(Raw, typeof(int));
            }

            return 0;
        }
        public long ToInt64(IFormatProvider provider)
        {
            if (IsNumber(Raw))
            {
                return (long)Convert.ChangeType(Raw, typeof(long));
            }

            return 0;
        }
        public long ToInt64()
        {
            if (IsNumber(Raw))
            {
                return (long)Convert.ChangeType(Raw, typeof(long));
            }

            return 0;
        }
        public object ToObject()
        {
            return Raw;
        }
        public sbyte ToSByte(IFormatProvider provider)
        {
            return (sbyte)Convert.ChangeType(Raw, Type);
        }
        public sbyte ToSByte()
        {
            return (sbyte)Convert.ChangeType(Raw, Type);
        }
        public float ToSingle(IFormatProvider provider)
        {
            if (IsNumber(Raw))
            {
                return (float)Convert.ChangeType(Raw, typeof(float));
            }

            return 0;
        }
        public float ToSingle()
        {
            if (IsNumber(Raw))
            {
                return (float)Convert.ChangeType(Raw, typeof(float));
            }

            return 0;
        }

        public string ToString(IFormatProvider provider)
        {
            return (string)Convert.ChangeType(Raw, Type);
        }
        public ushort ToUInt16(IFormatProvider provider)
        {
            if (IsNumber(Raw))
            {
                return (ushort)Convert.ChangeType(Raw, typeof(ushort));
            }

            return 0;
        }
        public ushort ToUInt16()
        {
            if (IsNumber(Raw))
            {
                return (ushort)Convert.ChangeType(Raw, typeof(ushort));
            }

            return 0;
        }
        public uint ToUInt32(IFormatProvider provider)
        {
            if (IsNumber(Raw))
            {
                return (uint)Convert.ChangeType(Raw, typeof(uint));
            }

            return 0;
        }
        public uint ToUInt32()
        {
            if (IsNumber(Raw))
            {
                return (uint)Convert.ChangeType(Raw, typeof(uint));
            }

            return 0;
        }
        public ulong ToUInt64(IFormatProvider provider)
        {
            if (IsNumber(Raw))
            {
                return (ulong)Convert.ChangeType(Raw, typeof(ulong));
            }

            return 0;
        }
        public ulong ToUInt64()
        {
            if (IsNumber(Raw))
            {
                return (ulong)Convert.ChangeType(Raw, typeof(ulong));
            }

            return 0;
        }
        #endregion Public Methods
    }
}
using System.Collections.Generic;

namespace Baxter.Domain
{
    /// <summary>A static Locale helper class</summary>
    public static class LocaleExtension
    {
        #region Public Methods
        /// <summary>Convert a string to a new Locale object</summary>
        public static Locale Locale(this string msg)
        {
            return new Locale(msg);
        }

        /// <summary>Convert a list of string values to locale values</summary>
        public static List<Locale> ToLocales(this IEnumerable<string> values)
        {
            var list = new List<Locale>();
            foreach (var item in values)
            {
                list.Add(new Locale(item));
            }

            return list;
        }

        /// <summary>Convert a list of Locale objects to a list of strings</summary>
        public static List<string> ToStrings(this List<Locale> values)
        {
            var list = new List<string>();
            foreach (var item in values)
            {
                list.Add(item.ToString());
            }

            return list;
        }
        #endregion Public Methods
    }

    /// <summary>A class that provides culturally sensitive strings</summary>
    public class Locale
    {
        #region Protected Fields
        protected string _value = string.Empty;
        #endregion Protected Fields

        #region Public Constructors
        public Locale(string value)
        {
            _value = value;
        }
        #endregion Public Constructors

        #region Public Methods
        public static bool operator !=(Locale loc, string value)
        {
            return !(loc._value == value);
        }

        public static bool operator !=(Locale left, Locale right)
        {
            return !(left == right);
        }

        public static bool operator ==(Locale loc, string value)
        {
            return loc._value == value;
        }

        public static bool operator ==(Locale left, Locale right)
        {
            return left._value == right._value;
        }

        public override bool Equals(object obj)
        {
            return _value == obj.ToString();
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value;
        }
        #endregion Public Methods
    }

    public class Locales : List<Locale>
    {
    }
}
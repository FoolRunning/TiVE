using System;

namespace ProdigalSoftware.TiVE.Settings
{
    internal abstract class Setting : IConvertible
    {
        public static readonly Setting Null = new NullSetting();

        public abstract string SaveAsString();

        protected abstract bool AreEqual(Setting setting);

        #region Overrides of Object
        public override bool Equals(object obj)
        {
            Setting other = obj as Setting;
            return other != null && AreEqual(other);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException("Setting can not be the key in a hash table");
        }

        public override string ToString()
        {
            return SaveAsString();
        }
        #endregion

        #region Operator overloads
        public static bool operator ==(Setting s1, Setting s2)
        {
            return Equals(s1, s2);
        }

        public static bool operator !=(Setting s1, Setting s2)
        {
            return !Equals(s1, s2);
        }

        public static implicit operator bool(Setting setting)
        {
            return setting.ToBoolean(null);
        }

        public static implicit operator int(Setting setting)
        {
            return setting.ToInt32(null);
        }

        public static implicit operator float(Setting setting)
        {
            return setting.ToSingle(null);
        }

        public static implicit operator string(Setting setting)
        {
            return setting.ToString(null);
        }
        #endregion

        #region Implementation of IConvertible
        public abstract TypeCode GetTypeCode();

        public virtual bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException(GetType() + " can not be cast to a boolean");
        }

        public virtual char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException(GetType() + " can not be cast to a char");
        }

        public virtual sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException(GetType() + " can not be cast to a sbyte");
        }

        public virtual byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException(GetType() + " can not be cast to a byte");
        }

        public virtual short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException(GetType() + " can not be cast to a short");
        }

        public virtual ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException(GetType() + " can not be cast to a ushort");
        }

        public virtual int ToInt32(IFormatProvider provider)
        {
            return ToInt16(provider);
        }

        public virtual uint ToUInt32(IFormatProvider provider)
        {
            return ToUInt16(provider);
        }

        public virtual long ToInt64(IFormatProvider provider)
        {
            return ToInt32(provider);
        }

        public virtual ulong ToUInt64(IFormatProvider provider)
        {
            return ToUInt32(provider);
        }

        public virtual float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException(GetType() + " can not be cast to a float");
        }

        public virtual double ToDouble(IFormatProvider provider)
        {
            return ToSingle(provider);
        }

        public virtual decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException(GetType() + " can not be cast to a decimal");
        }

        public virtual DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException(GetType() + " can not be cast to a DateTime");
        }

        public virtual string ToString(IFormatProvider provider)
        {
            return SaveAsString();
        }

        public virtual object ToType(Type conversionType, IFormatProvider provider)
        {
            throw new NotImplementedException(GetType() + " can not be cast to type " + conversionType);
        }
        #endregion
    }

    #region NullSetting class
    internal sealed class NullSetting : Setting
    {
        public override string SaveAsString()
        {
            return "";
        }

        protected override bool AreEqual(Setting setting)
        {
            return setting is NullSetting;
        }

        public override TypeCode GetTypeCode()
        {
            return TypeCode.Empty;
        }
    }
    #endregion

    #region BoolSetting class
    internal sealed class BoolSetting : Setting
    {
        private readonly bool value;
        
        public BoolSetting(bool value)
        {
            this.value = value;
        }

        public override string SaveAsString()
        {
            return value.ToString();
        }

        protected override bool AreEqual(Setting setting)
        {
            BoolSetting other = setting as BoolSetting;
            return other != null && other.value == value;
        }

        public override TypeCode GetTypeCode()
        {
            return TypeCode.Boolean;
        }

        public override bool ToBoolean(IFormatProvider provider)
        {
            return value;
        }
    }
    #endregion

    #region IntSetting class
    internal sealed class IntSetting : Setting
    {
        private readonly int value;

        public IntSetting(int value)
        {
            this.value = value;
        }

        public override string SaveAsString()
        {
            return value.ToString();
        }

        protected override bool AreEqual(Setting setting)
        {
            IntSetting other = setting as IntSetting;
            return other != null && other.value == value;
        }

        public override TypeCode GetTypeCode()
        {
            return TypeCode.Int32;
        }

        public override int ToInt32(IFormatProvider provider)
        {
            return value;
        }

        public override float ToSingle(IFormatProvider provider)
        {
            return value;
        }
    }
    #endregion

    #region StringSetting class
    internal sealed class StringSetting : Setting
    {
        private readonly string value;

        public StringSetting(string value)
        {
            this.value = value;
        }

        public override string SaveAsString()
        {
            return value;
        }

        protected override bool AreEqual(Setting setting)
        {
            StringSetting other = setting as StringSetting;
            return other != null && other.value == value;
        }

        public override TypeCode GetTypeCode()
        {
            return TypeCode.String;
        }
    }
    #endregion

    #region EnumSetting class
    internal sealed class EnumSetting<T> : Setting where T : struct, IConvertible
    {
        private readonly T value;

        public EnumSetting(T value)
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enum");

            this.value = value;
        }

        public override string SaveAsString()
        {
            return value.ToString();
        }

        protected override bool AreEqual(Setting setting)
        {
            EnumSetting<T> other = setting as EnumSetting<T>;
            return other != null && Equals(other.value, value);
        }

        public override TypeCode GetTypeCode()
        {
            return TypeCode.Int32;
        }

        public override int ToInt32(IFormatProvider provider)
        {
            return value.ToInt32(provider);
        }
    }
    #endregion

    internal sealed class ResolutionSetting : Setting
    {
        private readonly int value;
        
        public ResolutionSetting(int value)
        {
            this.value = value;
        }

        public override string SaveAsString()
        {
            return value.ToString();
        }

        protected override bool AreEqual(Setting setting)
        {
            return false;
        }

        public override TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }
    }
}

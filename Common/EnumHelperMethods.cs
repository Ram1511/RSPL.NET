using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace RSPL.Common
{
    public static class EnumHelperMethods
    {
        //public static string GetDescription(this Enum value)
        //{
        //    var type = value.GetType();
        //    var name = Enum.GetName(type, value);
        //    if (name == null)
        //        return null;
        //    var field = type.GetField(name);
        //    if (field == null)
        //        return null;
        //    var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
        //    return attr?.Description;
        //}
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            if (fi == null)
                return "";

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public static string GetEnumDescription<T>(int intValue)
        {
            Type enumType = typeof(T);
            var enumValue = (Enum)Enum.ToObject(enumType, intValue);
            return GetEnumDescription(enumValue);
        }

        public static T GetEnumByName<T>(string str) where T : struct, IConvertible
        {
            Type enumType = typeof(T);
            //////if (!enumType.IsEnum)
            //////{
            //////    throw new Exception("T must be an Enumeration type.");
            //////}
            T val;
            return Enum.TryParse<T>(str, true, out val) ? val : default(T);
        }

        public static T GetEnumByValue<T>(int intValue) where T : struct, IConvertible
        {
            Type enumType = typeof(T);
            //////if (!enumType.IsEnum)
            //////{
            //////    throw new Exception("T must be an Enumeration type.");
            //////}

            return (T)Enum.ToObject(enumType, intValue);
        }

        public static string GetEnumName<T>(int intValue) where T : struct, IConvertible
        {
            string result = "";
            try
            {
                Type enumType = typeof(T);
                //////if (!enumType.IsEnum)
                //////{
                //////    throw new Exception("T must be an Enumeration type.");
                //////}

                result = Enum.GetName(enumType, intValue);
            }
            catch (Exception)
            {
            }
            return result;
        }

        public static T GetEnumFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException("Not found.", nameof(description));
            // or return default(T);
        }

        //public static List<Core.Models.LookupItem> GetEnumLookup<T, TKey>() where T : struct
        //{
        //    var result = new List<Core.Models.LookupItem>();
        //    Type enumType = typeof(T);
        //    T enumMemberValue;
        //    string enumMemberDescription;
        //    Core.Models.LookupItem item;


        //    foreach (TKey enumMember in Enum.GetValues(typeof(T)))
        //    {
        //        enumMemberValue = (T)Enum.ToObject(enumType, enumMember);
        //        enumMemberDescription = GetEnumDescription(enumMemberValue as Enum);
        //        item = new Core.Models.LookupItem(enumMemberDescription, enumMember.ToString());

        //        result.Add(item);
        //    }

        //    return result;
        //}
        public static bool EnumContainValue<T>(string name)
        {
            return Enum.IsDefined(typeof(T), name);
        }

        public static bool EnumContainValue<T>(Int32 intValue)
        {
            return Enum.IsDefined(typeof(T), intValue);
        }

        public static bool EnumContainValue<T>(T value)
        {
            return Enum.IsDefined(typeof(T), value);
        }

        public static bool EnumContainValue<T>(Byte byteValue)
        {
            return Enum.IsDefined(typeof(T), byteValue);
        }
    }
}

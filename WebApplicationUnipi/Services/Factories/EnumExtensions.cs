using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
namespace WebApplicationUnipi.Services.Factories
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Custom extension method for enums
        ///
        /// Used to display different names for enums on front end
        /// Check source comments
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetDisplayAttributeFrom(this Enum enumValue) //for each enum you can use this as enum.thisFunc()
        {
            var enumType = enumValue.GetType(); // get enum type
            string displayName;
            MemberInfo info = enumType.GetMember(enumValue.ToString()).First();
            //MemberInfo => Obtains information about the attributes of a member and provides access to member metadata.
            // get an enum members
            // if not null it exists
            if (info != null && info.CustomAttributes.Any())
            {
                DisplayAttribute nameAttr = info.GetCustomAttribute<DisplayAttribute>();
                displayName = nameAttr != null ? nameAttr.Name + (nameAttr.Description != null ? " | " + nameAttr.Description : "") : enumValue.ToString();
            }
            else
            {
                displayName = enumValue.ToString();
            }
            return displayName;
        }

        public static string GetDisplayAttributeFrom(this Type enumType, int enumValue) //for each enum you can use this as enum.thisFunc()
        {
            string displayName;
            MemberInfo info = enumType.GetMember(Enum.GetName(enumType,enumValue)).First();
            //MemberInfo => Obtains information about the attributes of a member and provides access to member metadata.
            // get an enum members
            // if not null it exists
            if (info != null && info.CustomAttributes.Any())
            {
                DisplayAttribute nameAttr = info.GetCustomAttribute<DisplayAttribute>();
                displayName = nameAttr != null ? nameAttr.Name : Enum.GetName(enumType, enumValue);
            }
            else
            {
                displayName = Enum.GetName(enumType, enumValue);
            }
            return displayName;
        }
    }
}
﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ReactivePlot.Common
{
    /// <summary>
    /// https://stackoverflow.com/questions/4943817/mapping-object-to-dictionary-and-vice-versa/4944547#4944547
    /// </summary>
    public static class StringHelper
    {
        public static T ToObject<T>(this IDictionary<string, object> source)
            where T : class, new()
        {
            var someObject = new T();
            var someObjectType = someObject.GetType();

            foreach (var item in source)
            {
                someObjectType
                    .GetProperty(item.Key)
                        .SetValue(someObject, item.Value, null);
            }

            return someObject;
        }

        public static IDictionary<string, object> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );
        }

        public static string AsString(IDictionary<string, object> keyValuePairs)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var kvp in keyValuePairs)
            {
                stringBuilder.AppendLine($"{kvp.Key} : {kvp.Value}");
            }
            return stringBuilder.ToString();
        }
    }
}
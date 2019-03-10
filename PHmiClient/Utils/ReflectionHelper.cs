using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PHmiClient.Utils {
    public static class ReflectionHelper {
        public static object GetValue(object obj, string memberName) {
            Type type = obj.GetType();
            return type.InvokeMember(
                memberName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.GetField
                | BindingFlags.InvokeMethod,
                null, obj, null);
        }

        #region GetDisplayName

        public static string GetDisplayName(object obj, string property) {
            Type type = obj.GetType();
            return GetDisplayName(type, property);
        }

        public static string GetDisplayName(Type type, string property) {
            PropertyInfo prop = type.GetProperty(property);
            if (prop == null)
                return property;
            DisplayNameAttribute displayName = prop
                .GetCustomAttributes(typeof(DisplayNameAttribute), true)
                .Cast<DisplayNameAttribute>()
                .FirstOrDefault();
            if (displayName != null)
                return displayName.DisplayName;
            var metprops = ((MetadataTypeAttribute[]) type
                    .GetCustomAttributes(typeof(MetadataTypeAttribute), true))
                .Select(a => a.MetadataClassType.GetProperty(property))
                .Where(p => p != null)
                .ToArray();
            foreach (PropertyInfo p in metprops) {
                displayName = p.GetCustomAttributes(typeof(DisplayNameAttribute), true)
                    .Cast<DisplayNameAttribute>().FirstOrDefault();
                if (displayName != null) return displayName.DisplayName;
            }

            return property;
        }

        public static string GetDisplayName<T>(Expression<Func<T, object>> getPropertyExpression) {
            string property = PropertyHelper.GetPropertyName(getPropertyExpression);
            return GetDisplayName(typeof(T), property);
        }

        public static string GetDisplayName<T>(T obj, Expression<Func<T, object>> getPropertyExpression) {
            return GetDisplayName(getPropertyExpression);
        }

        #endregion GetDisplayName
    }
}
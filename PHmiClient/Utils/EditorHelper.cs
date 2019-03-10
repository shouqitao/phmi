using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace PHmiClient.Utils {
    public class EditorHelper : IEditorHelper {
        public object Clone(object obj) {
            Type type = obj.GetType();
            MetadataTypeAttribute metadataTypeAttribute =
                ((MetadataTypeAttribute[]) type.GetCustomAttributes(typeof(MetadataTypeAttribute), true))
                .FirstOrDefault();
            if (metadataTypeAttribute == null)
                throw new ArgumentException("obj must have MetadataTypeAttribute applied to it");
            Type metaType = metadataTypeAttribute.MetadataClassType;
            ConstructorInfo metaConstructor = metaType.GetConstructor(new Type[0]);
            if (metaConstructor == null)
                throw new ArgumentException("obj metadata type must have a parameterless constructor");
            object meta = metaConstructor.Invoke(new object[0]);
            var metaProps = metaType.GetProperties(
                BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty |
                BindingFlags.Instance);
            var props = type.GetProperties(
                BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty |
                BindingFlags.Instance);
            foreach (PropertyInfo p in metaProps.Where(p => p.CanRead && p.CanWrite)) {
                string propertyName = p.Name;
                PropertyInfo objProperty = props.First(pr => pr.Name == propertyName);
                object objValue = objProperty.GetValue(obj, null);
                if (p.PropertyType.GetInterfaces().Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))
                    && !typeof(Array).IsAssignableFrom(p.PropertyType)) {
                    if (objValue != null) {
                        ConstructorInfo pConstructor = p.PropertyType.GetConstructor(new Type[0]);
                        if (pConstructor != null) {
                            object collection = pConstructor.Invoke(new object[0]);
                            MethodInfo addMethod = p.PropertyType.GetMethod("Add");
                            foreach (object o in (IEnumerable) objValue)
                                addMethod.Invoke(collection, new[] {o});
                            p.SetValue(meta, collection, null);
                        }
                    }
                } else {
                    p.SetValue(meta, objValue, null);
                }
            }

            return meta;
        }

        public void Update(object source, object target) {
            Type sourceType = source.GetType();
            var sourceProps = sourceType.GetProperties(
                BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty |
                BindingFlags.Instance);
            Type targetType = target.GetType();
            var targetProps = targetType.GetProperties(
                BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty |
                BindingFlags.Instance);
            foreach (PropertyInfo p in sourceProps.Where(p => p.CanRead && p.CanWrite)) {
                string propertyName = p.Name;
                PropertyInfo targetProperty = targetProps.First(pr => pr.Name == propertyName);
                object sourceValue = p.GetValue(source, null);
                object targetValue = targetProperty.GetValue(target, null);
                if (p.PropertyType.GetInterfaces().Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))
                    && !typeof(Array).IsAssignableFrom(p.PropertyType)) {
                    if (sourceValue != null && targetValue != null) {
                        var metaEnumerable = (IEnumerable) sourceValue;
                        var targetEnumerable = (IEnumerable) targetValue;
                        var toRemove = (from object o in targetEnumerable
                            where metaEnumerable.Cast<object>().All(m => o != m)
                            select o).ToArray();
                        MethodInfo removeMethod = targetProperty.PropertyType.GetMethod("Remove");
                        foreach (object o in toRemove) removeMethod.Invoke(targetValue, new[] {o});
                        var toAdd = (from object o in (IEnumerable) sourceValue
                            where targetEnumerable.Cast<object>().All(t => o != t)
                            select o).ToArray();
                        MethodInfo addMethod = targetProperty.PropertyType.GetMethod("Add");
                        foreach (object o in toAdd) addMethod.Invoke(targetValue, new[] {o});
                    }
                } else {
                    if (sourceValue == null) {
                        if (targetValue != null)
                            targetProperty.SetValue(target, null, null);
                    } else if (!sourceValue.Equals(targetValue)) {
                        var sourceArray = sourceValue as Array;
                        var targetArray = targetValue as Array;
                        if (sourceArray != null && targetArray != null) {
                            if (!ArraysEqual(sourceArray, targetArray))
                                targetProperty.SetValue(target, sourceValue, null);
                        } else {
                            targetProperty.SetValue(target, sourceValue, null);
                        }
                    }
                }
            }
        }

        private static bool ArraysEqual(Array array1, Array array2) {
            if (array1.Length != array2.Length) return false;
            return !array1.Cast<object>().Where((t, i) => !AreEqual(array1.GetValue(i), array2.GetValue(i)))
                .Any();
        }

        private static bool AreEqual(object obj1, object obj2) {
            if (obj1 == null) return obj2 == null;
            return obj1.Equals(obj2);
        }
    }
}
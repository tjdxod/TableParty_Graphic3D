#if !ODIN_INSPECTOR

#if UNITY_EDITOR
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#endif

using System;
using UnityEngine;

namespace Dive.Avatar.Meta
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal class LabelTextAttribute : PropertyAttribute
    {
        #region Public Properties
        
        public string Label { get; private set; }
        
        #endregion
        
        #region Public Methods
        
        public LabelTextAttribute(string label)
        {
            Label = label;
        }
        
        #endregion
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LabelTextAttribute), true)]
    internal class LabelTextAttributeDrawer : PropertyDrawer
    {
        #region Public Methods
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, GetLabel(property));
        }
        
        #endregion
        
        #region Private Methods
        
        private static GUIContent GetLabel(SerializedProperty property)
        {
            LabelTextAttribute labelTextAttribute = GetAttribute<LabelTextAttribute>(property);
            string labelText = (labelTextAttribute == null)
                ? property.displayName
                : labelTextAttribute.Label;
            
            GUIContent label = new GUIContent(labelText);
            return label;
        }
        
        private static T GetAttribute<T>(SerializedProperty property) where T : class
        {
            T[] attributes = GetAttributes<T>(property);
            return (attributes.Length > 0) ? attributes[0] : null;
        }

        private static T[] GetAttributes<T>(SerializedProperty property) where T : class
        {
            var propertyInfo = GetProperty(GetTargetObjectWithProperty(property), property.name);
            var fieldInfo = GetField(GetTargetObjectWithProperty(property), property.name);            
            
            if (propertyInfo != null)
            {
                return (T[])propertyInfo.GetCustomAttributes(typeof(T), true);
            }            
            
            if (fieldInfo != null)
            {
                return (T[])fieldInfo.GetCustomAttributes(typeof(T), true);
            }
            
            return new T[] { };
        }

        private static FieldInfo GetField(object target, string fieldName)
        {
            return GetAllFields(target, f => f.Name.Equals(fieldName, StringComparison.Ordinal)).FirstOrDefault();
        }

        private static PropertyInfo GetProperty(object target, string propertyName)
        {
            return GetAllProperties(target, p => p.Name.Equals(propertyName,
                StringComparison.InvariantCulture)).FirstOrDefault();
        }
        
        private static IEnumerable<FieldInfo> GetAllFields(object target, Func<FieldInfo, bool> predicate)
        {
            if (target == null)
            {
                Debug.LogError("The target object is null. Check for missing scripts.");
                yield break;
            }

            List<Type> types = GetSelfAndBaseTypes(target);

            for (int i = types.Count - 1; i >= 0; i--)
            {
                IEnumerable<FieldInfo> fieldInfos = types[i]
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(predicate);

                foreach (var fieldInfo in fieldInfos)
                {
                    yield return fieldInfo;
                }
            }
        }

        private static IEnumerable<PropertyInfo> GetAllProperties(object target, Func<PropertyInfo, bool> predicate)
        {
            var types = new List<Type>
            {
                target.GetType()
            };

            while (types.Last().BaseType != null)
            {
                types.Add(types.Last().BaseType);
            }

            for (var i = types.Count - 1; i >= 0; i--)
            {
                var propertyInfos = types[i]
                    .GetProperties(BindingFlags.Instance | BindingFlags.Static |
                                   BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(predicate);

                foreach (var propertyInfo in propertyInfos)
                {
                    yield return propertyInfo;
                }
            }
        }

        
        private static object GetTargetObjectWithProperty(SerializedProperty property)
        {
            string path = property.propertyPath.Replace(".Array.data[", "[");
            object obj = property.serializedObject.targetObject;
            string[] elements = path.Split('.');

            for (int i = 0; i < elements.Length - 1; i++)
            {
                string element = elements[i];
                if (element.Contains("["))
                {
                    string elementName = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
                    int index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal)).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }

            return obj;
        }

        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
            {
                return null;
            }

            Type type = source.GetType();

            while (type != null)
            {
                FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    return field.GetValue(source);
                }

                PropertyInfo property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    return property.GetValue(source, null);
                }

                type = type.BaseType;
            }

            return null;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            IEnumerable enumerable = GetValue_Imp(source, name) as IEnumerable;
            if (enumerable == null)
            {
                return null;
            }

            IEnumerator enumerator = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++)
            {
                if (!enumerator.MoveNext())
                {
                    return null;
                }
            }

            return enumerator.Current;
        }

        private static List<Type> GetSelfAndBaseTypes(object target)
        {
            List<Type> types = new List<Type>()
            {
                target.GetType()
            };

            while (types.Last().BaseType != null)
            {
                types.Add(types.Last().BaseType);
            }

            return types;
        }        
        
        #endregion
    }
#endif
}

#endif
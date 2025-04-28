#if !ODIN_INSPECTOR

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
#endif

using System;
using UnityEngine;

namespace Dive.VRModule
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal class ShowIfAttribute : PropertyAttribute
    {
        #region Public Properties

        public string Condition { get; private set; }
        public int Comparison { get; private set; }

        #endregion

        #region Public Methods

        public ShowIfAttribute(string condition, bool isValue)
        {
            Condition = condition;
            Comparison = isValue ? 1 : 0;
        }

        public ShowIfAttribute(string condition, object objectValue)
        {
            Condition = condition;
            Comparison = Convert.ToInt32(objectValue);
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Field)]
    internal class EnableIfAttribute : PropertyAttribute
    {
        #region Public Properties

        public string Condition { get; private set; }
        public int Comparison { get; private set; }

        #endregion

        #region Public Methods

        public EnableIfAttribute(string condition, bool isValue)
        {
            Condition = condition;
            Comparison = isValue ? 1 : 0;
        }

        public EnableIfAttribute(string condition, object objectValue)
        {
            Condition = condition;
            Comparison = Convert.ToInt32(objectValue);
        }

        #endregion
    }

#if UNITY_EDITOR
    //참고
    //https://stackoverflow.com/questions/58441744/how-to-enable-disable-a-list-in-unity-inspector-using-a-bool
    [CustomPropertyDrawer(typeof(ShowIfAttribute), true)]
    [CustomPropertyDrawer(typeof(EnableIfAttribute), true)]
    internal class ShowIfAttributeDrawer : PropertyDrawer
    {
        #region Public Methods

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 속성 높이를 계산하고 조건을 충족하지 않고 그리기 모드가 DontDraw이면 높이는 0이됩니다..
            var meetsCondition = MeetsConditions(property);
            var showIfAttribute = attribute is ShowIfAttribute;

            if (!meetsCondition && showIfAttribute)
                return 0;
            return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var meetsCondition = MeetsConditions(property);
            if (meetsCondition)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            var showIfAttribute = attribute is ShowIfAttribute;

            if (!showIfAttribute)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.PropertyField(position, property, label, true);
                EditorGUI.EndDisabledGroup();
            }
        }

        #endregion

        #region Private Methods

        private static FieldInfo GetField(object target, string fieldName)
        {
            return GetAllFields(target, f => f.Name.Equals(fieldName,
                StringComparison.InvariantCulture)).FirstOrDefault();
        }

        private static MethodInfo GetMethod(object target, string methodName)
        {
            return GetAllMethods(target, m => m.Name.Equals(methodName,
                StringComparison.InvariantCulture)).FirstOrDefault();
        }

        private static PropertyInfo GetProperty(object target, string propertyName)
        {
            return GetAllProperties(target, p => p.Name.Equals(propertyName,
                StringComparison.InvariantCulture)).FirstOrDefault();
        }

        private static IEnumerable<FieldInfo> GetAllFields(object target, Func<FieldInfo, bool> predicate)
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
                var fieldInfos = types[i]
                    .GetFields(BindingFlags.Instance | BindingFlags.Static |
                               BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(predicate);

                foreach (var fieldInfo in fieldInfos)
                {
                    yield return fieldInfo;
                }
            }
        }

        private static IEnumerable<MethodInfo> GetAllMethods(object target, Func<MethodInfo, bool> predicate)
        {
            var methodInfos = target.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Static |
                            BindingFlags.NonPublic | BindingFlags.Public)
                .Where(predicate);

            return methodInfos;
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

        private bool MeetsConditions(SerializedProperty property)
        {
            var target = property.serializedObject.targetObject;
            string condition;
            int comparison;

            switch (attribute)
            {
                case ShowIfAttribute showIfAttribute:
                    condition = showIfAttribute.Condition;
                    comparison = showIfAttribute.Comparison;
                    break;
                case EnableIfAttribute enableIfAttribute:
                    condition = enableIfAttribute.Condition;
                    comparison = enableIfAttribute.Comparison;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return OpMeetsConditions(target, condition, comparison);
        }

        private bool OpMeetsConditions(object target, string condition, int comparison)
        {
            try
            {
                var conditionField = GetField(target, condition);
                if (conditionField != null)
                {
                    if (conditionField.FieldType == typeof(bool))
                    {
                        var isValue = (bool)conditionField.GetValue(target);
                        return comparison == (isValue ? 1 : 0);
                    }

                    var enumValue = conditionField.GetValue(target);

                    return comparison == Convert.ToInt32(enumValue);
                }

                var conditionProperty = GetProperty(target, condition);
                if (conditionProperty != null)
                {
                    if (conditionProperty.PropertyType == typeof(bool))
                    {
                        var isValue = (bool)conditionProperty.GetValue(target);
                        return comparison == (isValue ? 1 : 0);
                    }

                    var enumValue = conditionProperty.GetValue(target);

                    return comparison == Convert.ToInt32(enumValue);
                }

                var conditionMethod = GetMethod(target, condition);
                if (conditionMethod != null && conditionMethod.GetParameters().Length == 0)
                {
                    if (conditionMethod.ReturnType == typeof(bool))
                    {
                        var isValue = (bool)conditionMethod.Invoke(target, null);
                        return comparison == (isValue ? 1 : 0);
                    }

                    var enumValue = conditionMethod.Invoke(target, null);

                    return comparison == Convert.ToInt32(enumValue);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return true;
        }

        #endregion
    }
#endif
}

#endif
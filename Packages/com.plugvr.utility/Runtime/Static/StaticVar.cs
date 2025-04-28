using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dive.Utility
{
    /// <summary>
    /// Unity의 No domain reload에 대응하기 위한 Static 초기화 클래스
    /// 플레이 버튼을 누를 시 StaticVar로 선언한 Static 변수를 초기 값으로 변경
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StaticVar<T> : IStaticVar
    {
        public T Value;
        private readonly T defaultValue;

        public StaticVar(T value)
        {
            Value = value;
            defaultValue = value;

#if UNITY_EDITOR
            StaticStorage.Register(this);
#endif
        }

        public static implicit operator T(StaticVar<T> v)
        {
            if (v == null)
                throw new NullReferenceException("StaticVar is null");

            return v.Value;
        }

        public static bool operator ==(StaticVar<T> a, T b)
        {
            if (a is null)
            {
                return b is null;
            }

            if (b is null)
            {
                return false;
            }

            var isCast = b is StaticVar<T>;

            return isCast && a.Value.Equals(b);
        }

        public static bool operator !=(StaticVar<T> a, T b)
        {
            if (a is null)
            {
                return b is not null;
            }

            if (b is null)
            {
                return true;
            }

            var isCast = b is StaticVar<T>;

            return !isCast || !a.Value.Equals(b);
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && obj.GetType() == GetType() && Equals((StaticVar<T>)obj);
        }

        private bool Equals(StaticVar<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Value, other.Value) && EqualityComparer<T>.Default.Equals(defaultValue, other.defaultValue);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, defaultValue);
        }

        public void Reset()
        {
            Value = defaultValue;
        }
    }
}
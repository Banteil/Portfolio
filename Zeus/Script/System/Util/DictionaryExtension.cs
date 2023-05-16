using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

namespace Zeus
{
    public static class DictionaryExtension
    {
        public static TValue Upsert<TKey, TValue>(this Dictionary<TKey, TValue> _dic, TKey _key, TValue _value)
        {
            if (_dic.ContainsKey(_key))
                _dic[_key] = _value;
            else
                _dic.Add(_key, _value);

            return _dic[_key];
        }
    }

    public static class ListExtension
    {
        public static List<TValue> Copy<TValue>(this List<TValue> _list)
        {
            var a = _list.ToArray();
            var list = new List<TValue>();
            foreach (var item in a)
            {
                list.Add(item);
            }

            return list;
        }
    }

    public static partial class GameObjectExtension
    {
        public static T ComponentAdd<T>(this GameObject _ob) where T : MonoBehaviour
        {
            var component = _ob.GetComponent<T>();
            if (component == null)
                component = _ob.AddComponent<T>();

            return component;
        }

        public static T CopyComponent<T>(this GameObject destination, T original) where T : MonoBehaviour
        {
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy as T;
        }
    }

    public static class FloatExtentions
    {
        public static bool CompaerEpsilon(this float _source, float _dest, float _epsilon)
        {
            return Mathf.Abs(_source - _dest) < _epsilon;
        }
    }

    public static class IntExtentions
    {
        //0~max까지 순자가 나온다.
        public static int NextBounderyNumber(this int _source, bool _next, int max)
        {
            var counter = _source;
            if (_next)
                ++counter;
            else
                --counter;

            if (counter > max)
            {
                counter = 0;
            }
            else if (counter < 0)
            {
                counter = max;
            }

            return counter;
        }
    }

    public static class Vector3Extentions
    {
        public static bool CompaerEpsilon(this Vector3 _source, Vector3 _dest, float _epsilon)
        {
            return Mathf.Abs(_source.x - _dest.x) < _epsilon && Mathf.Abs(_source.y - _dest.y) < _epsilon && Mathf.Abs(_source.z - _dest.z) < _epsilon;
        }
    }

    //public static class MemoryStreamExtensions
    //{
    //    public static void Append(this MemoryStream stream, byte value)
    //    {
    //        stream.Append(new[] { value });
    //    }

    //    public static void Append(this MemoryStream stream, byte[] values)
    //    {
    //        stream.Write(values, 0, values.Length);
    //    }

    //    public static void Append(this MemoryStream stream, byte[] values, int _length)
    //    {
    //        stream.Write(values, 0, _length);
    //    }
    //}

    public static class NetworkStreamExtension
    {
        public static int Readn(this NetworkStream stream, byte[] buffer, int offset, int size)
        {
            int totalRead = 0;
            int expected = size;
            try
            {
                do
                {
                    var read = stream.Read(buffer, offset, expected);
                    if (read > 0)
                    {
                        //Add to the read buffer
                        totalRead += read;
                        expected -= read;
                    }
                    else
                        return 0;
                    //something went wrong
                } while (totalRead < expected);
            }
            catch (IOException e)
            {
                // Extract some information from this exception, and then   
                // throw it to the parent method.  
                if (e.Source != null)
                    Debug.Log("IOException source: {0}" + e.Message);
                throw;
            }

            return totalRead;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
    public static void Clear(this Transform transform)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(transform.GetChild(i).gameObject);
        }
    }
}

public static class StringExtensions
{
    public static byte[] ToEncodedByteArray(this string str, System.Text.Encoding encoding)
    {
        return encoding.GetBytes(str);
    }
}


// copy right
// https://zenn.dev/nogi_awn/articles/76440270429ad0

using System;
using System.Reflection;
using UnityEngine;

public static class ComponentUtility
{
    public static T CopyFrom<T>(this T self, T other) where T : Component
    {
        Type type = typeof(T);

        FieldInfo[] fields = type.GetFields();
        foreach (var field in fields)
        {
            if (field.IsStatic) continue;
            field.SetValue(self, field.GetValue(other));
        }

        PropertyInfo[] props = type.GetProperties();
        foreach (var prop in props)
        {
            if (!prop.CanWrite || !prop.CanRead || prop.Name == "name") continue;
            prop.SetValue(self, prop.GetValue(other));
        }

        return self as T;
    }
    public static T AddComponent<T>(this GameObject self, T other) where T : Component
    {
        return self.AddComponent<T>().CopyFrom(other);
    }
}
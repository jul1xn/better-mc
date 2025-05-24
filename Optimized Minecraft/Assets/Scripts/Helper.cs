using TMPro;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

public static class Helper
{
    public static string GetBoolText(bool value)
    {
        if (value)
        {
            return "ON";
        }
        else
        {
            return "OFF";
        }
    }

    public static string ConvertToValidFilename(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return "";
        }

        input = input.ToLower();
        input = input.Replace(' ', '_');
        input = Regex.Replace(input, @"[^a-z0-9_]", "");
        input = input.Trim('_');

        return input;
    }

    public static string Texture2DToByteString(Texture2D texture)
    {
        if (texture == null) return null;

        byte[] textureBytes = texture.EncodeToPNG();
        return Convert.ToBase64String(textureBytes);
    }

    public static Texture2D ByteStringToTexture2D(string base64String)
    {
        if (string.IsNullOrEmpty(base64String)) return null;

        byte[] textureBytes = Convert.FromBase64String(base64String);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(textureBytes);
        return texture;
    }

    public static string ConvertVector2ToString(Vector2 position)
    {
        return $"{position.x};{position.y}";
    }

    public static string ConvertVector3ToString(Vector3 position)
    {
        return $"{position.x};{position.y};{position.z}";
    }

    public static Vector3 ConvertStringToVector3(string value)
    {
        string[] lines = value.Split(';');
        return new Vector3(float.Parse(lines[0]), float.Parse(lines[1]), float.Parse(lines[2]));
    }

    public static Vector2 ConvertStringToVector2(string value)
    {
        string[] lines = value.Split(';');
        return new Vector2(float.Parse(lines[0]), float.Parse(lines[1]));
    }

    public static short[] Vector3ToShortList(Vector3 value)
    {
        return new short[] { (short)value.x, (short)value.y, (short)value.z };
    }

    public static Vector3 ShortListToVector3(short[] value)
    {
        return new Vector3(value[0], value[1], value[2]);
    }
}
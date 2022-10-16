using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public static class Convert
{
    public static Vector2 GetVector2(byte[] _data, int _beginPoint = 0, bool _x = true, bool _y = true)
    {
        Vector2 vect = Vector2.zero;
        if (_data.Length < sizeof(float) * 2) return vect;
        if (_x) vect.x = BitConverter.ToSingle(_data, _beginPoint + 0 * sizeof(float));
        if (_y) vect.y = BitConverter.ToSingle(_data, _beginPoint + 1 * sizeof(float));
        return vect;

    }
    public static byte[] GetByteVector2(Vector2 _vector, bool _x = true, bool _y = true)
    {
        byte[] vect = new byte[sizeof(float) * 2];
        if (_x) Buffer.BlockCopy(BitConverter.GetBytes(_vector.x), 0, vect, 0 * sizeof(float), sizeof(float));
        if (_y) Buffer.BlockCopy(BitConverter.GetBytes(_vector.y), 0, vect, 1 * sizeof(float), sizeof(float));

        return vect;
    }
    public static byte[] GetByteVector2(Vector3 _vector, bool _x = true, bool _y = true)
    {
        byte[] vect = new byte[sizeof(float) * 2];
        if (_x) Buffer.BlockCopy(BitConverter.GetBytes(_vector.x), 0, vect, 0 * sizeof(float), sizeof(float));
        if (_y) Buffer.BlockCopy(BitConverter.GetBytes(_vector.y), 0, vect, 1 * sizeof(float), sizeof(float));

        return vect;
    }

    public static Vector3 GetVector3(byte[] _data, int _beginPoint = 0, bool _x = true, bool _y = true, bool _z = true)
    {
        Vector3 vect = Vector3.zero;
        if (_data.Length < sizeof(float) * 3) return vect;
        if (_x) vect.x = BitConverter.ToSingle(_data, _beginPoint + 0 * sizeof(float));
        if (_y) vect.y = BitConverter.ToSingle(_data, _beginPoint + 1 * sizeof(float));
        if (_z) vect.z = BitConverter.ToSingle(_data, _beginPoint + 2 * sizeof(float));
        return vect;
    }
    public static byte[] GetByteVector3(Vector3 _vector, bool _x = true, bool _y = true, bool _z = true)
    {
        byte[] vect = new byte[sizeof(float) * 3];
        if (_x) Buffer.BlockCopy(BitConverter.GetBytes(_vector.x), 0, vect, 0 * sizeof(float), sizeof(float));
        if (_y) Buffer.BlockCopy(BitConverter.GetBytes(_vector.y), 0, vect, 1 * sizeof(float), sizeof(float));
        if (_z) Buffer.BlockCopy(BitConverter.GetBytes(_vector.z), 0, vect, 2 * sizeof(float), sizeof(float));

        return vect;
    }

    public static byte[] Conversion(bool _data)
    {
        return BitConverter.GetBytes(_data);
    }

    public static byte[] ToArrayByte(int _data)
    {
        return BitConverter.GetBytes(_data);
    }

    public static byte[] ToArrayByte(float _data)
    {
        return BitConverter.GetBytes(_data);
    }

    public static byte[] ToArrayByte(double _data)
    {
        return BitConverter.GetBytes(_data);
    }
    public static byte[] ToStringUTF8Byte(string _data)
    {
        return System.Text.Encoding.UTF8.GetBytes(_data);
    }


    public static float FloatConversion(byte[] _data, int _index)
    {
        return BitConverter.ToSingle(_data, _index);
    }
    public static int IntConversion(byte[] _data, int _index)
    {
        return BitConverter.ToInt32(_data, _index);
    }
    public static bool BoolConversion(byte[] _data, int _index)
    {
        return BitConverter.ToBoolean(_data, _index);
    }

    public static string StringUTF8ByteConversion(byte[] _data, int _index)
    {
        byte[] encData = new byte[_data.Length - _index];
        Array.Copy(_data, _index, encData, 0, encData.Length);
        return System.Text.Encoding.UTF8.GetString(encData);
    }
}


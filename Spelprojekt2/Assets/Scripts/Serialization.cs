using Unity.Collections;
using UnityEngine;
using System;
using System.Text;


public static class Serialization
{
  public static byte[] SerializeArray(uint[] a)
  {
    byte[] buffer = new byte[a.Length * 4];
    Buffer.BlockCopy(a, 0, buffer, 0, buffer.Length);

    return buffer;
  }

  public static byte[] SerializeArray(NativeArray<float> a)
  {
    return SerializeArray(a.ToArray());
  }

  public static byte[] SerializeArray(int[] a)
  {
    byte[] buffer = new byte[a.Length * 4];
    Buffer.BlockCopy(a, 0, buffer, 0, buffer.Length);

    return buffer;
  }

  public static byte[] SerializeArray(float[] a)
  {
    byte[] buffer = new byte[a.Length * 4];
    Buffer.BlockCopy(a, 0, buffer, 0, buffer.Length);

    return buffer;
  }



  public static int memcpy(ref byte[] dest, byte[] src, int offset)
  {
    for (int i = 0; i < src.Length; i++)
    {
      dest[i + offset] = src[i];
    }
    return offset + src.Length;
  }

  public static byte[] SerializeArray<T>(T[] src, int tSize)
  {
    byte[] dest = new byte[src.Length * tSize];
    Buffer.BlockCopy(src, 0, dest, 0, dest.Length);
    return dest;
  }

  public static int SerializeArray<T>(ref byte[] dest, T[] src, int offset, int tSize = 4)
  {
      byte[] src_bytes = SerializeArray<T>(src, tSize);
      return memcpy(ref dest, src_bytes, offset);
  }

  public static int SerializeScalar<T>(ref byte[] dest, T src, int offset, int tSize = 4)
  {
    T[] s = new T[1] { src };
    Buffer.BlockCopy(s, 0, dest, offset, tSize);
    return offset + tSize;
  }

  public static int SerializeVector3(ref byte[] dest, Vector3 src, int offset)
  {
      float[] p = new float[3]
      {
          src.x,
          src.y,
          src.z,
      };
      return SerializeArray<float>(ref dest, p, offset);
  }

  public static int SerializeQuaternion(ref byte[] dest, Quaternion src, int offset)
  {
      float[] p = new float[4]
      {
          src.x,
          src.y,
          src.z,
          src.w,
      };
      return SerializeArray<float>(ref dest, p, offset);
  }

  public static int SerializeString(string str, byte[] buffer, int offset, int max_size)
  {
      if(str.Length > max_size)
      {
          Debug.LogError($" Error serializing string, str is longer then max {str.Length} vs {max_size}");
      }
      int size    = str.Length > max_size ? max_size : str.Length;
      int padding = max_size - size;

      byte[] str_bytes = Encoding.UTF8.GetBytes(str);
      offset = memcpy(ref buffer, str_bytes, offset);
      offset += padding;
      return offset;
  }

  public static int DeserializeScalar<T>(ref T dest, byte[] src, int offset, int tSize = 4)
  {
    T[] d = new T[1];
    Buffer.BlockCopy(src, offset, d, 0, tSize);
    dest = d[0];
    return offset + tSize;
  }

  public static int DeserializeString(ref string str, byte[] buffer, int offset, int max_size)
  {
    byte[] str_buf = new byte[max_size];
    offset         = DeserializeArray<byte>(ref str_buf, buffer, offset, 1);
    int string_len = Array.IndexOf(str_buf, 0);
    string_len     = string_len < 0 ? max_size : string_len;
    str            = Encoding.UTF8.GetString(str_buf, 0, string_len);
    return offset;
  }


  public static int DeserializeVector3(ref Vector3 dest, byte[] src, int offset)
  {
    float[] p = new float[3];
    offset = DeserializeArray<float>(ref p, src, offset);
    dest = new(p[0], p[1], p[2]);
    return offset;

  }
  public static int DeserializeQuaternion(ref Quaternion dest, byte[] src, int offset)
  {
    float[] p = new float[4];
    offset = DeserializeArray<float>(ref p, src, offset);
    dest = new(p[0], p[1], p[2], p[3]);
    return offset;
  }

  public static int DeserializeArray<T>(ref T[] dest, byte[] src, int offset, int tSize = 4)
  {
    int to_read = dest.Length * tSize;
    Buffer.BlockCopy(src, offset, dest, 0, to_read);
    return offset + to_read;
  }
}

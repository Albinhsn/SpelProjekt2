using Unity.Collections;
using System;


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

  public static int SerializeScalar<T>(ref byte[] dest, T src, int offset, int tSize = 4)
  {
    T[] s = new T[1] { src };
    Buffer.BlockCopy(s, 0, dest, offset, tSize);
    return offset + tSize;
  }

  public static int DeserializeScalar<T>(ref T dest, byte[] src, int offset, int tSize = 4)
  {
    T[] d = new T[1];
    Buffer.BlockCopy(src, offset, d, 0, tSize);
    dest = d[0];
    return offset + tSize;
  }

  public static int DeserializeFloat(ref float dest, byte[] src, int offset)
  {
    float[] d = new float[1];
    Buffer.BlockCopy(src, offset, d, 0, sizeof(float));
    dest = d[0];
    return offset + 4;
  }

  public static int DeserializeArray<T>(ref T[] dest, byte[] src, int offset, int tSize = 4)
  {
    int to_read = dest.Length * tSize;
    Buffer.BlockCopy(src, offset, dest, 0, to_read);
    return offset + to_read;
  }
}

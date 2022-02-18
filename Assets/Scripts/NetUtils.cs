using System.Collections;

public static class NetUtils
{
    //Array of 8 booleans
    public static byte ToByte(this bool[] bools)
    {
        byte[] boolsByte = new byte[1];
        if (bools.Length == 8)
        {
            BitArray a = new BitArray(bools);
            a.CopyTo(boolsByte, 0);
        }

        return boolsByte[0];
    }

    //bitNumber -> index of the starting boolean array
    public static bool GetBoolean(this byte b, int bitNumber)
    {
        return (b & (1 << bitNumber)) != 0;
    }
}
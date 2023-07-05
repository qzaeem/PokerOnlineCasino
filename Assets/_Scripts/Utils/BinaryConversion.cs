using System;

public static class BinaryByteConversion
{
    public static byte ConvertBoolArrayToByte(bool[] bitArray)
    {
        string bitString = "";

        for(int i = 0; i < bitArray.Length; i++)
        {
            bitString += bitArray[i] ? "1" : "0";
        }

        return Convert.ToByte(bitString, 2);
    }

    public static bool[] ConvertByteToBoolArray(byte num)
    {
        string bitString = Convert.ToString(num, 2);

        if(bitString.Length < 6)
        {
            string temp = "";

            for(int i = 0; i < 6 - bitString.Length; i++)
            {
                temp += "0";
            }

            bitString = temp + bitString;
        }

        bool[] bitArray = new bool[bitString.Length];

        for(int i = 0; i < bitString.Length; i++)
        {
            bitArray[i] = bitString[i] == '1' ? true : false;
        }

        return bitArray;
    }
}

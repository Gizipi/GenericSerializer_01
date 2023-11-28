using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buffer 
{
    static int intBSize = 4;
    static int boolBSize = 1;
    static int charBSize = 2;
    static int longBSize = 8;


    //Conveert Value to Byte 
    public static byte[] ConvertToByte(object value)
    {

        byte[] storer = new byte[intBSize];

        for (int i = 0; i < intBSize; i++)
        {
            //storer[i] = ((byte)(value >> (8 * i)));
        }

        return storer;
    }

    //Conveert Value to Byte 
    public static void ConvertToByte(int value, List<byte> bytes, int writeIndex)
    {

        for (int i = writeIndex; i < writeIndex + intBSize; i++)
        {
            bytes[i] =((byte)(value >> (8 * i)));
        }
    }

    public static byte[] ConvertToByte(bool value)
    {

        byte[] storer = new byte[boolBSize];

        storer[0] = ((byte)(value ? 1 : 0));

        return storer;
    }

    public static byte[] ConvertToByte(char value)
    {

        byte[] storer = new byte[charBSize];

        for (int i = 0; i < charBSize; i++)
        {
            storer[i] = ((byte)(value >> (8 * i)));
        }

        return storer;
    }

    public static byte[] ConvertToByte(long value)
    {

        byte[] storer = new byte[longBSize];

        for (int i = 0; i < longBSize; i++)
        {
            storer[i] = (byte)(value >> (8 * i));
        }

        return storer;
    }
}

public static class ConvertByteToValue
{
    //convert byte to value
    public static void ConvertByteToInt(out int value, byte[] bytes, int readIndex)
    {

        value = bytes[readIndex] | (bytes[readIndex + 1] << 8) | (bytes[readIndex + 2] << 16) | (bytes[readIndex + 3] << 24);
    }

    public static bool ConvertBytetoBool(byte[] bytes)
    {
        if (bytes[0] == 0)
            return false;
        else
            return true;

        //return Convert.ToBoolean(bytes[0]);
    }

    public static char ConvertByteToChar(byte[] bytes)
    {

        char result = (char)(bytes[0] | (bytes[1] << 8));

        //Console.WriteLine($"Read Char: {result}");

        return result;
    }

    public static long ConvertByteToLong(byte[] bytes)
    {

        long result = bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24) | (bytes[4] << 48) | (bytes[5] << 96) | (bytes[6] << 192) | (bytes[7] << 384);

        //Console.WriteLine($"Read Long: {result}");

        return result;
    }
}


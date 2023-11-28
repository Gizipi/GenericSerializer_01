using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using UnityEngine;
using UnityEngine.XR;

public static class GenericSaving
{
    private static Dictionary<int, object> objectClasses_read = new Dictionary<int, object>();
    private static List<object> objectClasses_write = new List<object>();

    //public const int byteLength = 8;
    //public const int intByteLength = 4;
    //public const int charByteLength = 1;
    //public const int boolByteLength = 1;

    public static void Serialize<t>(t obj, Stream stream)
    {
        Debug.Log("Begin Serialize");

        if (stream.isRead)
            Reading(obj, stream);
        else
            Writing(obj, stream);
    }

    private static void Reading(object obj, Stream stream)
    {
        ClassHeader header = new ClassHeader();

        Assembly b = obj.GetType().Assembly;

        header.classNameLength = b.FullName.Length;
        header.className = b.FullName;
        header.classPointer = -1;
        Debug.Log("Read class header");
        stream.ReadClassHeader(ref header);

        Debug.Log($"Class length after read: {header.classLength}");


        if (stream.isRead)
        {
            //stream.Serialize(ref serializeInttarget);
            //Debug.Log($"Read value: {serializeInttarget}");
            //field.SetValue(obj, serializeInttarget);
        }
    }

    private static void WriteExistingClass(int key, Stream stream)
    {
        int serializeInttarget = 0;

        Assembly b = objectClasses_write[key].GetType().Assembly;
        string name = b.GetName().Name;

        ValueHeader header = new ValueHeader();
        header.valueNameLength = name.Length;
        header.valueName = name;
        header.valueLength = 4;
        header.valueType = ValueType.pointer;
        Serialize(header, stream);

        stream.Serialize(ref key);
    }

    private static void Writing<t>(t obj, Stream stream)
    {
        Assembly b = GetAssemblyOfType<t>();

        if (objectClasses_write.Contains(obj))
        {
            WriteExistingClass(objectClasses_write.IndexOf(obj), stream);
            return;
        }

        objectClasses_write.Add(obj);

        if (stream.currentClass.classLength == 0)
            stream.CreateClassHeader(objectClasses_write.IndexOf(obj), b);

        Type characterType = typeof(t);

        FieldInfo[] characterFields = characterType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        ValueHeader header = new ValueHeader();

        foreach (var field in characterFields)
        {

            Debug.Log($"Reading Field: {field.Name}");

            switch (field.FieldType.Name)
            {
                case "Int32":

                    header.valueLength = 4;
                    int intValue = 0;

                    UpdateValueHeader(ref header, ValueType.Int, field);
                    Serialize(header, stream);
                    intValue = (Int32)field.GetValue(obj);
                    // Debug.Log($"Serialize int: {intValue}");
                    stream.Serialize(ref intValue);
                    break;
                case "Bool":

                    header.valueLength = 1;
                    bool boolvalue = false;

                    UpdateValueHeader(ref header, ValueType.Bool, field);
                    Serialize(header, stream);
                    boolvalue = (bool)field.GetValue(obj);
                    //Debug.Log($"Serialize bool: {boolvalue}");
                    stream.Serialize(ref boolvalue);
                    break;
                case "string":
                    string stringValue = (string)field.GetValue(obj);
                    char[] chars = stringValue.ToCharArray();
                    header.valueLength = 4 + chars.Length; //int byte size plus total length of chars in string

                    UpdateValueHeader(ref header, ValueType.String, field);
                    Serialize(header, stream);
                    //Debug.Log($"Serialize string: {stringValue}");
                    stream.Serialize(ref stringValue);
                    break;
                case "Char":

                    header.valueLength = 1;
                    char charValue = 'a';

                    UpdateValueHeader(ref header, ValueType.Char, field);
                    Serialize(header, stream);
                    charValue = (char)field.GetValue(obj);
                    //Debug.Log($"Serialize char: {charValue}");
                    stream.Serialize(ref charValue);
                    break;
                default:
                    Serialize(field.GetValue(obj), stream);
                    break;
                    //case "string":
                    //    ByteArrayToFile(field.Name, ConvertValueToByte.ConvertToByte((string)(object)obj));
                    //    break;
            }
        }

        if (!stream.isRead)
            stream.SaveBytes();


    }

    //private static void HandleFieldInfo<f>(f obj, ValueHeader header, FieldInfo field, Stream stream)
    //{
    //    header.valueLength = sizeof(f);
    //    f serializeChartarget = default;

    //    header = CreateHeader(ValueType.Char, field);
    //    Serialize(header, stream);
    //    serializeChartarget = (f)field.GetValue(obj);
    //    stream.Serialize(ref serializeChartarget);
    //}

    private static void Serialize(ValueHeader header, Stream stream)
    {
        //Debug.Log($"Header name length: {header.valueNameLength}");
        stream.Serialize(ref header.valueNameLength);
        //Debug.Log($"Header name: {header.valueName}");
        Serialize(header.valueName, stream);
        char valueType = (char)header.valueType;
        //Debug.Log($"Header value type: {valueType}");
        stream.Serialize(ref valueType);
        //Debug.Log($"Header value length: {header.valueLength}");
        stream.Serialize(ref header.valueLength);
    }

    private static void Serialize(string str, Stream stream)
    {
        foreach (char c in str)
        {
            //stream.Serialize(ref c);
        }
    }

    private static void UpdateValueHeader(ref ValueHeader header, ValueType type, FieldInfo field)
    {
        header.valueNameLength = field.Name.Length;
        header.valueName = (string)field.Name;
        header.valueType = type;
    }

    static Assembly GetAssemblyOfType<T>() => typeof(T).Assembly;
}

public class Stream
{
    public bool isRead;
    public ClassHeader currentClass = new ClassHeader();

    public Stream(string fileName) { }
    public virtual void CreateClassHeader(int key, Assembly field) { }
    public virtual void ReadClassHeader(ref ClassHeader header) { }
    public virtual void ReadValueHeader(ref ValueHeader header) { }
    public virtual void Serialize(ref string value) { }
    public virtual void Serialize(ref int value) { }
    public virtual void Serialize(ref char value) { }
    public virtual void Serialize(ref bool value) { }
    public virtual void SaveBytes() { }
}


class WriteStream : Stream
{
    private const bool IsRead = false;

    private BitWriter m_writer;



    public WriteStream(string fileName) : base(fileName)
    {
        isRead = IsRead;
        m_writer = new BitWriter(fileName);
    }

    public override void CreateClassHeader(int key, Assembly field)
    {
        currentClass.classNameLength = field.FullName.Length;
        currentClass.className = field.FullName;
        currentClass.classPointer = key;
        currentClass.classLength = 0;
    }

    public override void Serialize(ref string value)
    {
        char[] chars = value.ToCharArray();
        int charsLength = chars.Length;
        Serialize(ref charsLength);
        for (int i = 0; i < chars.Length; i++)
            Serialize(ref chars[i]);
    }

    public override void Serialize(ref int value)
    {
        currentClass.classLength += 4;
        m_writer.WriteBits(value, 4);
    }
    public override void Serialize(ref char value)
    {
        currentClass.classLength += 1;
        m_writer.WriteBits(value, 1);
    }
    public override void Serialize(ref bool value)
    {
        currentClass.classLength += 1;
        m_writer.WriteBits(value, 1);
    }

    public override void SaveBytes()
    {
        m_writer.SaveBytes(currentClass);
    }
}

class ReadStream : Stream
{
    private const bool IsRead = true;

    private BitReader m_reader;

    public ReadStream(string fileName) : base(fileName)
    {
        isRead = IsRead;
        m_reader = new BitReader(fileName);
    }

    public override void ReadClassHeader(ref ClassHeader header)
    {

        ClassHeader currentHeader = new ClassHeader();
        int count = 0;
        char collectedChar = default;
        while (count < m_reader.CollectBufferLength())
        {
            currentHeader.classNameLength = (int)m_reader.ReadBits(4 * 8);
            for (int i = 0; i < currentHeader.classNameLength; i++)
            {
                collectedChar = (char)m_reader.ReadBits(1 * 8);
                currentHeader.className = currentHeader.className + collectedChar;
            }
            currentHeader.classPointer = (int)m_reader.ReadBits(4 * 8);
            currentHeader.classLength = (int)m_reader.ReadBits(4 * 8);

            if (currentHeader.className == header.className || currentHeader.classPointer == header.classPointer)
            {
                header = currentHeader;
                return;
            }
            else
            {
                count += currentHeader.classLength;
                m_reader.ChangeByteIndexToStartOfNextClassHeader(currentHeader.classLength);
            }
        }

        Debug.Log($"There is no save data for this object: {header.className}");

        //Serialize(ref header.valueNameLength);
    }
    public override void Serialize(ref int value)
    {
        value = (int)m_reader.ReadBits(4 * 8);
    }

    public override void Serialize(ref char value)
    {
        value = (char)m_reader.ReadBits(1 * 8);
    }

    public override void Serialize(ref bool value)
    {
        int collectedInt = (int)m_reader.ReadBits(1 * 8);
        if (collectedInt == 0)
            value = false;
        else
            value = true;
    }
}


class BitWriter
{
    //int bytes;
    int byteIndex = 0;
    int bitIndex;

    //int writeIndex;
    List<byte> buffer;

    string fileLocation;
    bool saving = false;

    public BitWriter(string filename)
    {
        fileLocation = filename;

        buffer = new List<byte>();
        //writeIndex = 0;
        byteIndex = 0;
        bitIndex = 0;
        //Debug.Log("Contructor called");
    }

    public void WriteBits(int numBytes)
    {
        //Debug.Log($"Byte index: {byteIndex}");
        bitIndex = 0;
        ExpandList(numBytes * 8);
    }
    public void WriteBits(int value, int numBytes)
    {
        WriteBits(numBytes);
        for (int i = 0; i < (numBytes * 8); i++)
        {
            int bit = value & (1 << i);
            WriteBit(bit);
        }
    }
    public void WriteBits(char value, int numBytes)
    {
        WriteBits(numBytes);
        for (int i = 0; i < (numBytes * 8); i++)
        {
            int bit = value & (1 << i);
            WriteBit(bit);
        }
    }
    public void WriteBits(bool value, int numBytes)
    {
        WriteBits(numBytes);
        for (int i = 0; i < (numBytes * 8); i++)
        {
            if (value == true)
                WriteBit(1);
            else
                WriteBit(0);
        }
    }

    private void WriteBit(int bit)
    {
        int index = (byteIndex * 8) + (7 - bitIndex);
        if (index >= buffer.Count)
            throw new InvalidOperationException($"Buffer overflow, index: {index}, buffer.count: {buffer.Count}");

        if (bit != 0)
            buffer[index] |= 1; // (byte)(1 >> byteIndex);

        bitIndex++;
        if (bitIndex >= 8)
        {
            byteIndex++;
            bitIndex = 0;
            //Debug.Log($"ByteIndex should increase: {byteIndex}");
        }
    }

    private void ExpandList(int targetSize)
    {
        for (int i = 0; i < targetSize; i++)
        {
            if (saving)
                buffer.Insert((byteIndex * 8), 0);
            else
                buffer.Add(0);
        }
    }

    public void SaveBytes(ClassHeader header)
    {

        saving = true;

        byteIndex = byteIndex - header.classLength;
        WriteBits(header.classNameLength, 4);
        char[] chars = header.className.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
            WriteBits(chars[i], 1);
        WriteBits(header.classPointer, 4);
        WriteBits(header.classLength, 4);
        byteIndex = byteIndex + header.classLength;

        Debug.Log($"Saving, header name: {header.className}, byteIndex: {byteIndex}, length: {buffer.Count}, values: {buffer.ToArray().ToString()}");

        DebugBuffer();

        using (var fs = new FileStream(fileLocation, FileMode.OpenOrCreate, FileAccess.Write))
        {
            fs.Write(buffer.ToArray(), 0, buffer.Count);
        }
    }

    private void DebugBuffer()
    {
        string debugTXT = "";
        int bit = 0;
        int byt = 0;
        foreach (byte b in buffer)
        {
            bit++;
            debugTXT += b;

            if (bit != 8)
                continue;
            byt++;
            bit = 0;
            debugTXT += " ";
            if (byt != 4)
                continue;
            byt = 0;
            debugTXT += " || ";
        }
        Debug.Log(debugTXT);
    }
}

class BitReader
{
    int byteIndex;
    int bitIndex;

    byte[] buffer;

    string fileLocation;

    public BitReader(string filename)
    {
        fileLocation = filename;

        buffer = new byte[32 * 9];

        byteIndex = 0;
        bitIndex = 0;

        ReadFile();
    }

    private void ReadFile()
    {
        Debug.Log("Read file");
        using (var fs = new FileStream(fileLocation, FileMode.OpenOrCreate, FileAccess.Read))
        {
            buffer = new byte[(int)fs.Length];
            fs.Read(buffer);
        }

        Debug.Log($"buffer length: {buffer.Length}, value 29: {buffer[29]}, value 30: {buffer[30]}, value 31: {buffer[31]}");
    }
    public bool WouldReadPastEnd(int numBits)
    {
        int bitsRemaining = (byteIndex * 8) + bitIndex;
        return numBits > bitsRemaining;
    }

    public int CollectBufferLength()
    {
        return buffer.Length;
    }
    public void ChangeByteIndexToStartOfNextClassHeader(int numBits)
    {
        byteIndex += numBits / 8;
    }

    public uint ReadBits(int numBits)
    {
        if (WouldReadPastEnd(numBits))
            throw new InvalidOperationException("Attempted to read past the end of the buffer");

        uint result = 0;
        string bits = "";
        for (int i = 0; i < numBits; i++)
        {
            result <<= 1;
            int bit = buffer[(byteIndex * 8) + bitIndex];// (buffer[bitIndex] >> bitIndex) & 1;
            bits += bit;

            result |= (uint)bit; // (uint)(bit << i);

            bitIndex++;
            if (bitIndex >= 8)
            {
                bitIndex = 0;
                byteIndex++;
            }
        }

        Debug.Log($"Result: {result}, bits: {bits}");
        return result;
    }
}

public struct ClassHeader
{
    public int classNameLength;
    public string className;
    public int classPointer;
    public int classLength;
}

public struct ValueHeader
{
    public int valueNameLength;
    public string valueName;
    public ValueType valueType;
    public int valueLength;
}

public enum ValueType
{
    Int = 'a',
    UInt = 'b',
    Float = 'c',
    Bool = 'd',
    Char = 'e',
    String = 'f',
    Long = 'g',
    ULong = 'h',
    Short = 'i',
    UShort = 'j',
    Double = 'k',
    pointer = 'l',
}

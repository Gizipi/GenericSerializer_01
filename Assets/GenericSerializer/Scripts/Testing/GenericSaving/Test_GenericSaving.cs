using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;


[AttributeUsage(AttributeTargets.All)]
public class SerializableVariable : Attribute
{

}

public enum byteLengths
{
    interger = 4,
    character = 2,
    boolean = 2,
}


public static class Test_GenericSaving
{
    private static Dictionary<object, int> write_objectClasses = new Dictionary<object, int>();
    private static Dictionary<int, (object obj, string name)> read_objectClasses = new Dictionary<int, (object obj, string name)>();
    private static List<int> objectBacklog = new List<int>();
    //private static List<object> objectClasses_write = new List<object>();

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

    private static void Reading(object obj, Stream stream, string objectName = "")
    {
        ClassHeader classHeader = new ClassHeader();
        Dictionary<string, FieldInfo> classValues = new Dictionary<string, FieldInfo>();

        Type type = obj.GetType();

        Assembly b = type.Assembly;

        if (objectName == "")
            objectName = type.Name;

        classHeader.classNameLength = objectName.Length;
        classHeader.className = objectName;
        classHeader.classPointer = -1;
        if (stream.ReadClassHeader(ref classHeader))
        {
            Debug.Log($"Read class header, pointer = {classHeader.classPointer}");
            if (!read_objectClasses.ContainsKey(classHeader.classPointer))
                read_objectClasses.Add(classHeader.classPointer, (classHeader.classPointer, classHeader.className));
            bool reading = true;
            FieldInfo[] characterFields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            ValueHeader header = new ValueHeader();

            while (reading)
            {
                stream.ReadValueHeader(ref header);
                Debug.Log($"Value header name: {header.valueName}");

                foreach (FieldInfo field in characterFields)
                {
                    if (field.GetCustomAttribute<SerializableVariable>() == null)
                        continue;

                    if (field.Name == header.valueName)
                    {
                        if (header.valueType == ValueType.Int)
                        {
                            int value = 0;
                            stream.ReadBytes(ref value, header.valueLength);
                            field.SetValue(obj, value);
                        }
                        else if (header.valueType == ValueType.Char)
                        {
                            char value = 'a';
                            stream.Serialize(ref value);
                            field.SetValue(obj, value);
                        }
                        else if (header.valueType == ValueType.pointer)
                        {
                            int value = 0;
                            stream.ReadBytes(ref value, header.valueLength);
                            if (!read_objectClasses.ContainsKey(value))
                            {
                                objectBacklog.Add(value);
                                read_objectClasses.Add(value, (field.GetValue(obj), field.Name));
                            }
                            field.SetValue(obj, read_objectClasses[value].obj);
                        }
                    }
                }
                reading = stream.IsStillReading(classHeader);
            }
        }

        if (objectBacklog.Count > 0)
        {
            Debug.Log("Going through backLog");
            int readNum = objectBacklog[0];
            objectBacklog.RemoveAt(0);
            Reading(read_objectClasses[readNum].obj, stream, read_objectClasses[readNum].name);
        }
        Debug.Log($"Class length after read: {classHeader.classLength}");
    }

    private static int WriteExistingClass(object key, Stream stream, string objectName = "")
    {
        int serializeInttarget = 0;
        int pointer = write_objectClasses[key];

        Assembly b = key.GetType().Assembly;
        if (objectName == "")
            objectName = b.GetName().Name;

        ValueHeader header = new ValueHeader();
        header.valueNameLength = objectName.Length;
        header.valueName = objectName;
        header.valueLength = 4;
        header.valueType = ValueType.pointer;
        Serialize(header, stream);

        stream.Serialize(ref pointer);

        return pointer;
    }

    private static void Writing<t>(t obj, Stream stream, string objectName = "")
    {
        Type type = obj.GetType();

        List<(object obj, string name)> caughtObject = new List<(object, string)>();

        int key = write_objectClasses.Count;

        if (write_objectClasses.ContainsKey(obj))
            key = write_objectClasses[obj];
        else 
            write_objectClasses.Add(obj, key);

        stream.CreateClassHeader(key, obj, objectName);

        Debug.Log($"Writing class: {stream.currentClass.className}");


        Type characterType = typeof(t);

        FieldInfo[] characterFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        ValueHeader header = new ValueHeader();

        Debug.Log($"number of fields in class: {characterFields.Length}");

        foreach (var field in characterFields)
        {

            if (field.GetCustomAttribute<SerializableVariable>() == null)
                continue;

            Debug.Log($"Reading Field: {field.Name}");

            switch (field.FieldType.Name)
            {
                case "Int32":

                    header.valueLength = 4;
                    int intValue = 0;

                    UpdateValueHeader(ref header, ValueType.Int, field);
                    Serialize(header, stream);
                    intValue = (int)field.GetValue(obj);
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
                    header.valueLength = (int)byteLengths.character + chars.Length; //int byte size plus total length of chars in string

                    UpdateValueHeader(ref header, ValueType.String, field);
                    Serialize(header, stream);
                    //Debug.Log($"Serialize string: {stringValue}");
                    stream.Serialize(ref stringValue);
                    break;
                case "Char":

                    header.valueLength = (int)byteLengths.character;
                    char charValue = 'a';

                    UpdateValueHeader(ref header, ValueType.Char, field);
                    Serialize(header, stream);
                    charValue = (char)field.GetValue(obj);
                    //Debug.Log($"Serialize char: {charValue}");
                    stream.Serialize(ref charValue);
                    break;
                default:

                    header.valueLength = 4;

                    int childKey;
                    if (!write_objectClasses.ContainsKey(field.GetValue(obj)))
                    {
                        childKey = write_objectClasses.Count;
                        write_objectClasses.Add(field.GetValue(obj), childKey);
                        caughtObject.Add((field.GetValue(obj), field.Name));
                    }
                    else
                        childKey = write_objectClasses[field.GetValue(obj)];

                    UpdateValueHeader(ref header, ValueType.pointer, field);
                    Serialize(header, stream); ;

                    stream.Serialize(ref childKey);
                    break;
                    //case "string":
                    //    ByteArrayToFile(field.Name, ConvertValueToByte.ConvertToByte((string)(object)obj));
                    //    break;
            }
        }


        stream.WriteDownClass();

        foreach ((object obj, string name) item in caughtObject)
        {
            Writing(item.obj, stream, item.name);
        }

        if (objectName == "")
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
        Debug.Log($"serializing Header name: {header.valueName}");
        stream.Serialize(ref header.valueNameLength);
        stream.Serialize(ref header.valueName);
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
        header.valueName = field.Name;
        header.valueType = type;
    }

    static Assembly GetAssemblyOfType<T>() => typeof(T).Assembly;
}

public class Stream
{
    public bool isRead;
    public ClassHeader currentClass = new ClassHeader();

    public Stream(string fileName) { }
    public virtual void CreateClassHeader(int key, object field, string objectName) { }
    public virtual bool ReadClassHeader(ref ClassHeader header)
    {
        Debug.Log("ReadClassHeader was not overwritten");
        return false;
    }
    public virtual void ReadValueHeader(ref ValueHeader header) { }
    public virtual void ReadBytes(ref int value, int numBytes) { }
    public virtual bool IsStillReading(ClassHeader header) { return false; }
    public virtual void Serialize(ref string value) { }
    public virtual void Serialize(ref int value) { }
    public virtual void Serialize(ref char value) { }
    public virtual void Serialize(ref bool value) { }
    public virtual void WriteDownClass() { }
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

    public override void CreateClassHeader(int key, object obj, string objectName)
    {
        currentClass.classNameLength = obj.GetType().Name.Length;
        currentClass.className = obj.GetType().Name;
        currentClass.classPointer = key;
        currentClass.classLength = 0;

        if (objectName != "")
        {//clean plz
            currentClass.classNameLength = objectName.Length;
            currentClass.className = objectName;
        }
    }

    public override void Serialize(ref string value)
    {
        char[] chars = value.ToCharArray();
        int charsLength = chars.Length;
        Debug.Log($"Serialize string, string length: {charsLength}, string: {value}");
        //Serialize(ref charsLength);
        for (int i = 0; i < chars.Length; i++)
            Serialize(ref chars[i]);
    }

    public override void Serialize(ref int value)
    {
        currentClass.classLength += (int)byteLengths.interger;
        m_writer.WriteBits(value);
    }
    public override void Serialize(ref char value)
    {
        currentClass.classLength += (int)byteLengths.character;
        Debug.Log("Writing: " + (char)value);
        m_writer.WriteBits(value);
    }
    public override void Serialize(ref bool value)
    {
        currentClass.classLength += 1;
        m_writer.WriteBits(value);
    }

    public override void WriteDownClass()
    {
        m_writer.WriteDownClass(currentClass);
    }

    public override void SaveBytes()
    {
        m_writer.SaveBytes();
    }
}

class ReadStream : Stream
{
    private const bool IsRead = true;

    private BitReader m_reader;

    private int byteCount = 0;

    private int previousClassLengths = 0;

    public ReadStream(string fileName) : base(fileName)
    {
        isRead = IsRead;
        byteCount = 0;
        m_reader = new BitReader(fileName);
    }

    public override bool ReadClassHeader(ref ClassHeader header)
    {

        ClassHeader currentHeader = new ClassHeader();
        int count = 0;
        char collectedChar = default;
        while (count < m_reader.CollectBufferLength())
        {
            currentHeader.classNameLength = (int)m_reader.ReadBits((int)byteLengths.interger * 8);
            Debug.Log($"Class name length: {currentHeader.classNameLength}");
            for (int i = 0; i < currentHeader.classNameLength; i++)
            {
                collectedChar = (char)m_reader.ReadBits((int)byteLengths.character * 8);
                currentHeader.className = currentHeader.className + collectedChar;
            }
            Debug.Log($"Class name: {currentHeader.className}, compared to: {header.className}");
            currentHeader.classPointer = (int)m_reader.ReadBits(4 * 8);
            Debug.Log($"Class pointer: {currentHeader.classPointer}");
            currentHeader.classLength = (int)m_reader.ReadBits(4 * 8);
            Debug.Log($"Class length: {currentHeader.classLength}");

            byteCount += 4 + ((int)byteLengths.character * header.classNameLength) + 4 + 4;

            if (currentHeader.className == header.className)
            {
                Debug.Log("Class name is the same");
                header = currentHeader;
                previousClassLengths += currentHeader.classLength + (4 + (currentHeader.className.Length * (int)byteLengths.character) + 4 + 4);
                return true;
            }
            else
            {
                count += currentHeader.classLength * 8;
                byteCount += currentHeader.classLength;
                previousClassLengths += currentHeader.classLength + (4 + (currentHeader.className.Length * (int)byteLengths.character) + 4 + 4);
                m_reader.ChangeByteIndexToStartOfNextClassHeader(currentHeader.classLength);
            }
        }

        Debug.Log($"There is no save data for this object: {header.className}");

        return false;

        //Serialize(ref header.valueNameLength);
    }

    public override void ReadValueHeader(ref ValueHeader vHeader)
    {
        Debug.Log($"Read ValueHeader");
        char collectedChar = default;
        vHeader.valueName = "";
        vHeader.valueNameLength = (int)m_reader.ReadBits(4 * 8);
        Debug.Log("Name length: " + vHeader.valueNameLength);
        for (int i = 0; i < vHeader.valueNameLength; i++)
        {
            collectedChar = (char)m_reader.ReadBits((int)byteLengths.character * 8);
            vHeader.valueName = vHeader.valueName + collectedChar;
        }
        vHeader.valueType = (ValueType)m_reader.ReadBits((int)byteLengths.character * 8);
        vHeader.valueLength = (int)m_reader.ReadBits(4 * 8);
        byteCount += 4 + ((int)byteLengths.character * vHeader.valueNameLength) + (int)byteLengths.character + 4;
    }

    public override void ReadBytes(ref int value, int numBytes)
    {
        byteCount += numBytes;
        value = (int)m_reader.ReadBits(numBytes * 8);
    }

    public override void Serialize(ref int value)
    {
        byteCount += (int)byteLengths.interger;
        value = (int)m_reader.ReadBits((int)byteLengths.interger * 8);
    }

    public override void Serialize(ref char value)
    {
        byteCount += (int)byteLengths.character;
        value = (char)m_reader.ReadBits((int)byteLengths.character * 8);
    }

    public override void Serialize(ref bool value)
    {
        byteCount += 1;
        int collectedInt = (int)m_reader.ReadBits(1 * 8);
        if (collectedInt == 0)
            value = false;
        else
            value = true;
    }
    public override bool IsStillReading(ClassHeader header)
    {
        Debug.Log($"Count length: {byteCount}, class length: {header.classLength}");
        if (byteCount >= previousClassLengths)
            return false;
        else
            return true;
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
        byteIndex = -1;
        bitIndex = 0;
        //ReadFile();
        byteIndex = Mathf.Max((buffer.Count / 8), 0);
    }

    private void ReadFile()
    {
        byte[] bytes = null;
        Debug.Log("Read file");
        using (var fs = new FileStream(fileLocation, FileMode.OpenOrCreate, FileAccess.Read))
        {
            bytes = new byte[(int)fs.Length];
            fs.Read(bytes);
        }
        foreach (byte b in bytes)
            buffer.Add(b);
       
        Debug.Log($"buffer length: {buffer.Count}");
    }

    public void Extend(int numBytes)
    {
        //Debug.Log($"Byte index: {byteIndex}");
        bitIndex = 0;
        ExpandList(numBytes * 8);
        byteIndex += numBytes;
    }
    public void WriteBits(int value, int numBytes = 4)
    {
        Extend(numBytes);
        for (int i = 0; i < (numBytes * 8); i++)
        {
            int bit = value & (1 << i);
            WriteBit(bit);
        }
        byteIndex += numBytes;
    }
    public void WriteBits(char value, int numBytes = (int)byteLengths.character)
    {
        //Debug.Log($"byteIndex: {byteIndex}");
        Extend(numBytes);
        for (int i = 0; i < (numBytes * 8); i++)
        {
            int bit = value & (1 << i);
            WriteBit(bit);
        }
        byteIndex += numBytes;
    }
    public void WriteBits(bool value, int numBytes = 1)
    {
        Extend(numBytes);
        for (int i = 0; i < (numBytes * 8); i++)
        {
            if (value == true)
                WriteBit(1);
            else
                WriteBit(0);
        }
        byteIndex += numBytes;
    }

    private void WriteBit(int bit)
    {
        int index = ((byteIndex - 1) * 8) + (7 - bitIndex);
        if (index >= buffer.Count)
            throw new InvalidOperationException($"Buffer overflow, index: {index}, buffer.count: {buffer.Count}");

        if (bit != 0)
            buffer[index] |= 1; // (byte)(1 >> byteIndex);

        bitIndex++;
        if (bitIndex >= 8)
        {
            byteIndex--;
            bitIndex = 0;
            //Debug.Log($"ByteIndex should increase: {byteIndex}");
        }
    }

    private void ExpandList(int targetSize)
    {
        Debug.Log("Inserting at: " + byteIndex * 8);
        for (int i = 0; i < targetSize; i++)
        {            
            if (saving)
                buffer.Insert((byteIndex * 8), 0);
            else
                buffer.Add(0);
        }
    }

    public void WriteDownClass(ClassHeader header)
    {

        saving = true;

        Debug.Log($"Header info: class name length: {header.classNameLength}, header name: {header.className}, header pointer: {header.classPointer}, header length: {header.classLength}");

        byteIndex = byteIndex - header.classLength;
        WriteBits(header.classNameLength);
        char[] chars = header.className.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
            WriteBits(chars[i]);
        WriteBits(header.classPointer);
        WriteBits(header.classLength);
        byteIndex = byteIndex + header.classLength;

        Debug.Log($"Saving, header name: {header.className}, byteIndex: {byteIndex}, buffer length: {buffer.Count}, values: {buffer.ToArray().ToString()}");

        DebugBuffer();

        saving = false;
    }

    public void SaveBytes()
    {
        using (var fs = new FileStream(fileLocation, FileMode.Create, FileAccess.Write))
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

        buffer = new byte[0];

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

        Debug.Log($"buffer length: {buffer.Length}");
    }
    public bool WouldReadPastEnd(int numBits)
    {
        int bitsRead = (byteIndex * 8) + bitIndex;
        return buffer.Length < bitsRead + numBits;
    }

    public int CollectBufferLength()
    {
        return buffer.Length;
    }
    public void ChangeByteIndexToStartOfNextClassHeader(int numBits)
    {
        byteIndex += numBits;
    }

    public uint ReadBits(int numBits)
    {
        if (WouldReadPastEnd(numBits))
            throw new InvalidOperationException($"Attempted to read past the end of the buffer, read bits: {(byteIndex * 8) + bitIndex}, buffer length: {buffer.Length}, numBits: {numBits}");

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

        //Debug.Log($"ByteIndex: {byteIndex}, Result: {result}, bits: {bits}");
        return result;
    }

    public void ReadBits(ref object value, int numBits)
    {
        if (WouldReadPastEnd(numBits))
            throw new InvalidOperationException($"Attempted to read past the end of the buffer, read bits: {(byteIndex * 8) + bitIndex}, numBits: {numBits}");

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

        Debug.Log($"ByteIndex: {byteIndex}, Result: {result}, bits: {bits}");
        value = result;
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

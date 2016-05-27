/* ****************************************************************
 =========================================================================
DireenTech Inc. (www.DireenTech.com)

  File Name:      ByteArrayReaderWriter.cs
  Created:        Aug. 30, 2012
  Author:		  Harry Direen, DireenTech
  Description:    
 * 
 * Desc: ByteArrayReaderWriter
 * Two class ByteArrayReader and ByteArrayWriter are definded. 
 * These classes are used for packing or unpacking data into a 
 * byte array.  The endianness of the packing order can be defined.
 * 	
=========================================================================
               Copyright (c) 2012 - 2015 DireenTech Inc.
All or Portions of This Software are Copyrighted by DireenTech Inc.
                        Company Proprietary
========================================================================= 
 *******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Rabit.Utils
{
    public enum EndianOrder_e
    {
        little,
        big
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct FloatUIntStruct
    {
        [FieldOffset(0)]
        public float floatValue;
        [FieldOffset(0)]
        public UInt32 uintValue;
    }

    public class NumberConverter
    {

        /// <summary>
        /// Convert a 16-bit float (passed as a UInt16) to 
        /// a float.  The 16-bit float has a 9-bit mantissa, a 6-bit expontent
        /// and a sign bit.  The format is similar to the IEEE-754 but in 16-bit
        /// form.  The exponent uses a bias of 31 insead of 127.  The mantissa
        /// is the upper 9-bits of a IEEE-754 32-bit float mantissa.
        /// </summary>
        /// <param name="float16">The float16 in and unsigned 16-bit format.</param>
        /// <returns>a 32-bit float.</returns>
        public static float float16ToFloat32(UInt16 float16)
        {
            FloatUIntStruct fltIntVal = new FloatUIntStruct();
            fltIntVal.uintValue = 0;
            UInt32 mant = (UInt32)float16 & 0x01ff;
            Int32 exp = (Int32)((float16 & 0x7E00)) >> 9;
            if (exp == 0x2F)        //Max value
            {
                fltIntVal.floatValue = (float)1.0e32;
                if ((float16 & 0x8000) != 0)
                    fltIntVal.uintValue |= 0x80000000;
                return (float)1.0e32;  //Max value  
            }
            else if (exp == 0)
            {
                if (mant == 0)
                {
                    fltIntVal.floatValue = (float)0.0;
                    return fltIntVal.floatValue;
                }
                else
                {
                    //we are at our smallest 16-bit float value...
                    //re-adjust mantissa and exp for 32-bit float;
                    exp = -31;
                    for (int i = 0; i < 9; ++i)
                    {
                        mant = mant << 1;
                        --exp;
                        if ((mant & 0x0200) != 0)
                            break;
                    }
                }
            }
            else
            {
                exp = exp - 31;     //bias of 31.
            }

            mant = mant & 0x1FF;
            mant = mant << 14;   //IEEE-754 mantisa
            exp = exp + 127;    //IEEE-754 exp bias.
            fltIntVal.uintValue = ((UInt32)exp << 23) + mant;
            if ((float16 & 0x8000) != 0)
                fltIntVal.uintValue |= 0x80000000;

            return fltIntVal.floatValue;
        }

        /// <summary>
        /// Convert a 16-bit float (passed as a UInt16) to 
        /// a float.  The 16-bit float has a 9-bit mantissa, a 6-bit expontent
        /// and a sign bit.  The format is similar to the IEEE-754 but in 16-bit
        /// form.  The exponent uses a bias of 31 insead of 127.  The mantissa
        /// is the upper 9-bits of a IEEE-754 32-bit float mantissa.
        /// </summary>
        /// <param name="float16">The float16 in and unsigned 16-bit format.</param>
        /// <returns>a 32-bit float.</returns>
        public static UInt16 float32ToFloat16(float value)
        {
            UInt32 float16 = 0;
            FloatUIntStruct fltIntVal = new FloatUIntStruct();
            fltIntVal.floatValue = value;
            UInt32 mant = fltIntVal.uintValue & 0x007fffff;
            Int32 exp = (Int32)(fltIntVal.uintValue >> 23) & 0xff;
            if (exp == 0)        //Max value
            {
                return 0;  //Max value  
            }
            exp -= 127;
            if (exp > 31)
                return (UInt16)0x7E00;  //Max positive value
            else if (exp < -31)
                return (UInt16)0xFE00;  //Max negative value
            else
            {
                mant = mant >> 14;
                exp = (exp + 31) & 0x3F;
                float16 = mant + (UInt32)(exp << 9);
                if ((fltIntVal.uintValue & 0x80000000) != 0)
                    float16 = float16 | 0x8000;
            }
            return (UInt16)float16;
        }
    }

    public class ByteArrayReader
    {
        private EndianOrder_e endianOrder;
        private EndianOrder_e systemEndianness;
        private readonly byte[] byteArray;
        private int idx;          //Current index into the byte array

        //To avoid constanly creating and distroying common 
        //temporary byte arrays, create them once here.
        private byte[] ba2;
        private byte[] ba4;
        private byte[] ba8;

        public EndianOrder_e EndianOrder
        {
            get { return endianOrder; }
            set { endianOrder = value; }
        }
        public int Idx
        {
            get { return idx; }
            set { idx = value < 0 ? 0 : value >= byteArray.Length ? byteArray.Length - 1 : value; }
        }
        public byte[] ByteArray
        {
            get { return byteArray; }
        }
        public int ByteArraySize
        {
            get { return ByteArray.Length; }
        }

        public ByteArrayReader(byte[] array, EndianOrder_e endianness)
        {
            byteArray = array;
            endianOrder = endianness;
            systemEndianness = BitConverter.IsLittleEndian ? EndianOrder_e.little : EndianOrder_e.big;
            ba2 = new byte[2];
            ba4 = new byte[4];
            ba8 = new byte[8];
        }

        private byte[] getBytes(int NoBytes)
        {
            byte[] valArray = NoBytes == 2 ? ba2 : NoBytes == 3 || NoBytes == 4 ? ba4 : NoBytes <= 8 ? ba8 : new byte[NoBytes];
            try
            {
                if (idx + NoBytes <= byteArray.Length)
                {
                    if (endianOrder == systemEndianness)
                    {
                        for (int i = 0; i < NoBytes; ++i)
                            valArray[i] = byteArray[idx++];
                    }
                    else
                    {
                        for (int i = NoBytes - 1; i >= 0; --i)
                            valArray[i] = byteArray[idx++];
                    }
                }
            }
            catch (Exception ex)
            {
				Console.WriteLine( "ByteArrayReader:getBytes. Err: " + ex.Message);
            }
            return valArray;
        }

        /// <summary>
        /// Read NoBytes from the reader into the bArray, starting
        /// at the bArray offset of startIdx.
        /// </summary>
        /// <param name="bArray">The byte array to return value in.</param>
        /// <param name="startIdx">The start index into the byte array.</param>
        /// <param name="NoBytes">The no bytes to be read.</param>
        /// <returns>The number of bytes read</returns>
        public int readBytes(byte[] bArray, int startIdx, int NoBytes)
        {
            int n1 = byteArray.Length - Idx;
            int n2 = bArray.Length - startIdx;
            NoBytes = NoBytes > n1 ? n1 : NoBytes;
            NoBytes = NoBytes > n2 ? n2 : NoBytes;
            for (int i = 0; i < NoBytes; i++)
                bArray[startIdx++] = byteArray[Idx++];

            return NoBytes;
        }

        public byte readUInt8()
        {
            byte value = 0;
            if( idx < byteArray.Length )
                value = byteArray[idx++];
            return value;
        }

        public Int16 readInt16()
        {
            byte[] valArray = getBytes(2);
            Int16 value = 0;
            try
            {
                value = BitConverter.ToInt16(valArray, 0);
            }
            catch (Exception ex)
            {
				Console.WriteLine( "ByteArrayReader:readInt16. Err: " + ex.Message);
            }
            return value;
        }

        public UInt16 readUInt16()
        {
            byte[] valArray = getBytes(2);
            UInt16 value = 0;
            try
            {
                value = BitConverter.ToUInt16(valArray, 0);
            }
            catch (Exception ex)
            {
				Console.WriteLine( "ByteArrayReader:readUInt16. Err: " + ex.Message);
            }
            return value;
        }


        public Int32 readInt24()
        {
            UInt32 value = 0;
            try
            {
                if (idx + 3 <= byteArray.Length)
                {
                    if (endianOrder == EndianOrder_e.big)
                    {
                        for (int i = 2; i >= 0; --i)
                            value += (UInt32)byteArray[idx++] << (i * 8);
                    }
                    else
                    {
                        for (int i = 0; i < 3; ++i)
                            value += (UInt32)byteArray[idx++] << (i * 8);
                    }
                    if( (value & 0x00800000) != 0 )
                        value |= 0xFF000000;        //Extend sign
                }
            }
            catch (Exception ex)
            {
				Console.WriteLine( "ByteArrayReader:ToInt32. Err: " + ex.Message);
            }
            return (Int32)value;
        }

        public UInt32 readUInt24()
        {
            UInt32 value = 0;
            try
            {
                if (idx + 3 <= byteArray.Length)
                {
                    if (endianOrder == EndianOrder_e.big)
                    {
                        for (int i = 2; i >= 0; --i)
                            value += (UInt32)byteArray[idx++] << (i * 8);
                    }
                    else
                    {
                        for (int i = 0; i < 3; ++i)
                            value += (UInt32)byteArray[idx++] << (i * 8);
                    }
                }
            }
            catch (Exception ex)
            {
				Console.WriteLine( "ByteArrayReader:ToUInt32. Err: " + ex.Message);
            }
            return value;
        }

        public Int32 readInt32()
        {
            byte[] valArray = getBytes(4);
            Int32 value = 0;
            try
            {
                value = BitConverter.ToInt32(valArray, 0);
            }
            catch (Exception ex)
            {
				Console.WriteLine( "ByteArrayReader:ToInt32. Err: " + ex.Message);
            }
            return value;
        }

        public UInt32 readUInt32()
        {
            byte[] valArray = getBytes(4);
            UInt32 value = 0;
            try
            {
                value = BitConverter.ToUInt32(valArray, 0);
            }
            catch (Exception ex)
            {
				Console.WriteLine( "ByteArrayReader:ToUInt32. Err: " + ex.Message);
            }
            return value;
        }

        public Int64 readInt64()
        {
            byte[] valArray = getBytes(8);
            Int64 value = 0;
            try
            {
                value = BitConverter.ToInt64(valArray, 0);
            }
            catch (Exception ex)
            {
				Console.WriteLine( "ByteArrayReader:ToInt64. Err: " + ex.Message);
            }
            return value;
        }

        public UInt64 readUInt64()
        {
            byte[] valArray = getBytes(8);
            UInt64 value = 0;
            try
            {
                value = BitConverter.ToUInt64(valArray, 0);
            }
            catch (Exception ex)
            {
				Console.WriteLine( "ByteArrayReader:ToUInt64. Err: " + ex.Message);
            }
            return value;
        }

        public double readDouble()
        {
            byte[] valArray = getBytes(8);
            double value = 0;
            try
            {
                value = BitConverter.ToDouble(valArray, 0);
            }
            catch (Exception ex)
            {
				Console.WriteLine( "ByteArrayReader:ToDouble. Err: " + ex.Message);
            }
            return value;
        }

        public float readFloat()
        {
            byte[] valArray = getBytes(4);
            float value = 0;
            try
            {
                value = BitConverter.ToSingle(valArray, 0);
            }
            catch (Exception ex)
            {
				Console.WriteLine( "ByteArrayReader:ToSingle. Err: " + ex.Message);
            }
            return value;
        }


        public float read16BitFloat()
        {
            UInt16 value = readUInt16();
            return NumberConverter.float16ToFloat32(value);
        }

        public string readString(int strSize)
        {
            string strValue = "";
            try
            {
                ASCIIEncoding ascii = new ASCIIEncoding();
                byte[] asciiBytes = new byte[strSize + 1];
                int maxIdx = byteArray.Length - idx;
                maxIdx = maxIdx < strSize ? maxIdx : strSize;
                if (maxIdx < 1)
                    return null;

                int end = 0;
                for (int i = 0; i < maxIdx; ++i)
                {
                    asciiBytes[i] = byteArray[idx++];
                    if (end == 0 && asciiBytes[i] == 0)
                        end = i;
                }
                if (end == 0) end = maxIdx;

                strValue = ascii.GetString(asciiBytes, 0, end);
            }
            catch (Exception ex)
            {
				Console.WriteLine( "ByteArrayReader:readString. Err: " + ex.Message);
            }
            return strValue;
        }

    }


    public class ByteArrayWriter
    {
        private EndianOrder_e endianOrder;
        private EndianOrder_e systemEndianness;
        private readonly byte[] byteArray;
        private int idx;          //Current index into the byte array

        public EndianOrder_e EndianOrder
        {
            get { return endianOrder; }
            set { endianOrder = value; }
        }
        public int Idx
        {
            get { return idx; }
            set { idx = value < 0 ? 0 : value >= byteArray.Length ? byteArray.Length - 1 : value; }
        }
        public byte[] ByteArray
        {
            get { return byteArray; }
        }
        public int ByteArraySize
        {
            get { return ByteArray.Length; }
        }

        public ByteArrayWriter(byte[] array, EndianOrder_e endianness)
        {
            byteArray = array;
            endianOrder = endianness;
            systemEndianness = BitConverter.IsLittleEndian ? EndianOrder_e.little : EndianOrder_e.big;
        }

        public ByteArrayWriter(int arraySize, EndianOrder_e endianness)
        {
            byteArray = new byte[arraySize];
            endianOrder = endianness;
            systemEndianness = BitConverter.IsLittleEndian ? EndianOrder_e.little : EndianOrder_e.big;
        }

        public void Clear()
        {
            for(int i = 0; i < ByteArraySize; i++)
                byteArray[i] = 0;

            idx = 0;
        }

        private bool addToArray(byte[] valArray)
        {
            if (idx + valArray.Length > byteArray.Length)
                return true;    //No room in the Inn.

            if (endianOrder == systemEndianness)
            {
                for (int i = 0; i < valArray.Length; ++i)
                    byteArray[idx++] = valArray[i];
            }
            else
            {
                for (int i = valArray.Length - 1; i >= 0; --i)
                    byteArray[idx++] = valArray[i];
            }
            return false;
        }


        /// <summary>
        /// Write NoBytes from the bArray into the writer, starting
        /// at the bArray offset of startIdx.
        /// </summary>
        /// <param name="bArray">The byte array with values.</param>
        /// <param name="startIdx">The start index into the byte array.</param>
        /// <param name="NoBytes">The no bytes to write.</param>
        /// <returns>The number of bytes read</returns>
        public int writeBytes(byte[] bArray, int startIdx, int NoBytes)
        {
            int n1 = byteArray.Length - Idx;
            int n2 = bArray.Length - startIdx;
            NoBytes = NoBytes > n1 ? n1 : NoBytes;
            NoBytes = NoBytes > n2 ? n2 : NoBytes;
            for (int i = 0; i < NoBytes; i++)
                byteArray[Idx++] = bArray[startIdx++];

            return NoBytes;
        }

        public bool writeUInt8(byte value)
        {
            byteArray[idx++] = value;
            return false;
        }

        public bool writeInt16(Int16 value)
        {
            return addToArray(BitConverter.GetBytes(value));
        }

        public bool writeUInt16(UInt16 value)
        {
            return addToArray(BitConverter.GetBytes(value));
        }

        public bool writeInt24(Int32 value)
        {
            Int32 tmp;
            if (idx + 3 > byteArray.Length)
                return true;    //No room in the Inn.

            if (endianOrder == EndianOrder_e.big )
            {
                for (int i = 2; i >= 0; --i)
                {
                    tmp = value >> (8 * i);
                    byteArray[idx++] = (byte)(tmp & 0xff);
                }
            }
            else
            {
                for (int i = 0; i < 3; ++i)
                {
                    tmp = value >> (8 * i);
                    byteArray[idx++] = (byte)(tmp & 0xff);
                }
            }
            return false;
        }

        public bool writeUInt24(UInt32 value)
        {
            UInt32 tmp;
            if (idx + 3 > byteArray.Length)
                return true;    //No room in the Inn.

            if (endianOrder == EndianOrder_e.big)
            {
                for (int i = 2; i >= 0; --i)
                {
                    tmp = value >> (8 * i);
                    byteArray[idx++] = (byte)(tmp & 0xff);
                }
            }
            else
            {
                for (int i = 0; i < 3; ++i)
                {
                    tmp = value >> (8 * i);
                    byteArray[idx++] = (byte)(tmp & 0xff);
                }
            }
            return false;
        }

        public bool writeInt32(Int32 value)
        {
            return addToArray(BitConverter.GetBytes(value));
        }

        public bool writeUInt32(UInt32 value)
        {
            return addToArray(BitConverter.GetBytes(value));
        }

        public bool writeInt64(Int64 value)
        {
            return addToArray(BitConverter.GetBytes(value));
        }

        public bool writeUInt64(UInt64 value)
        {
            return addToArray(BitConverter.GetBytes(value));
        }

        public bool writeDouble(double value)
        {
            return addToArray(BitConverter.GetBytes(value));
        }

        public bool writeFloat(float value)
        {
            return addToArray(BitConverter.GetBytes(value));
        }

        public bool writeFloat16(float value)
        {
            UInt16 float16 = NumberConverter.float32ToFloat16(value);
            return addToArray(BitConverter.GetBytes(float16));
        }

        public bool writeString(string str, int maxSize)
        {
            int i = 0;
            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] asciiBytes = ascii.GetBytes(str);
            maxSize = maxSize > 0 ? maxSize : asciiBytes.Length;
            if( idx + maxSize > byteArray.Length)
                return true;    //No room in the Inn.

            int N = asciiBytes.Length >= maxSize ? maxSize : asciiBytes.Length;
            while (i < N)
            {
                byteArray[idx++] = asciiBytes[i++];
            }
            while (i < maxSize)
            {
                byteArray[idx++] = 0;
                ++i;
            }
            return false;
        }
    }

}

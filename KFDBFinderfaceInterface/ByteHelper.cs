using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace KFDBFinder.Extensions
{
    public class ByteHelper
    {
        /// <summary>
        /// 获取取第index位 
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static int GetBit(byte b, int index)
        {
            return ((b & (1 << index)) > 0) ? 1 : 0;
        }
        /// <summary>
        /// 将第index位设为1 
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static byte SetBit(byte b, int index)
        {
            return (byte)(b | (1 << index));
        }
        /// <summary>
        /// 将第index位设为0 
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static byte ClearBit(byte b, int index)
        {
            return (byte)(b & (byte.MaxValue - (1 << index)));
        }
        /// <summary>
        /// 将第index位取反 
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static byte ReverseBit(byte b, int index)
        {
            return (byte)(b ^ (byte)(1 << index));
        }
        /// <summary>
        /// 将字节A写入目标字节B的指定位 
        /// </summary>
        /// <param name="Value">字节B 超过8位时低位数组索引小于高位</param>
        /// <param name="Index">起始位置</param>
        /// <param name="Leng">占位长度</param>
        /// <param name="OriginalValue">A字节</param>
        /// <returns></returns>
        public static byte[] BitProcessing(byte[] Value, int Index, int Leng, byte OriginalValue)
        {
            bool Med;
            for (int index = 1; index <= Leng; index++)
            {
                byte Weight = (byte)Math.Pow(2, index - 1);
                Med = ((OriginalValue & Weight) == Weight);
                int Cursor = Index + index - 1;
                Value[Cursor / 8] = set_bit(Value[Cursor / 8], Cursor % 8, Med);
            }
            return Value;
        }
        /// <summary>
        /// 设置字节任意位
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        private static byte set_bit(byte data, int index, bool flag)
        {
            ++index;
            if (index > 8 || index < 1)
                throw new ArgumentOutOfRangeException();
            int v = index < 2 ? index : (2 << (index - 2));
            return flag ? (byte)(data | v) : (byte)(data & ~v);
        }

        public static byte[] StructToBytes(object structObj)
        {

            //返回类的非托管大小（以字节为单位）  
            int size = Marshal.SizeOf(structObj);

            //分配大小  
            byte[] bytes = new byte[size];

            //从进程的非托管堆中分配内存给structPtr  
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            try
            {
                //将数据从托管对象structObj封送到非托管内存块structPtr  
                Marshal.StructureToPtr(structObj, structPtr, false);

                //Marshal.StructureToPtr(structObj, structPtr, true);  
                //将数据从非托管内存指针复制到托管 8 位无符号整数数组  
                Marshal.Copy(structPtr, bytes, 0, size);

                return bytes;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                //释放以前使用 AllocHGlobal 从进程的非托管内存中分配的内存  
                Marshal.FreeHGlobal(structPtr);
            }
        }
        public static object BytesToStruct(byte[] bytes, Type strType)
        {
            //获取结构体的大小（以字节为单位）  
            int size = Marshal.SizeOf(strType);
            //简单的判断（可以去掉）  
            if (size > bytes.Length)
            {
                return null;
            }

            //从进程的非托管堆中分配内存给structPtr  
            IntPtr strPtr = Marshal.AllocHGlobal(size);
            try
            {

                //将数据从一维托管数组bytes复制到非托管内存指针strPtr  
                Marshal.Copy(bytes, 0, strPtr, size);

                //将数据从非托管内存块封送到新分配的指定类型的托管对象  
                //将内存空间转换为目标结构体  
                object obj = Marshal.PtrToStructure(strPtr, strType);

                return obj;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                //释放以前使用 AllocHGlobal 从进程的非托管内存中分配的内存 
                Marshal.FreeHGlobal(strPtr);
            }
        }

        public static byte[] ObjectToBytes(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                return ms.GetBuffer();
            }
        }
    }
}

using DigitaPlatform.DeviceAccess.Base;
using DigitaPlatform.DeviceAccess.Transfer;
using DigitaPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DigitaPlatform.DeviceAccess.Execute
{
    internal class Mitsubishi_3E: MitsubishiBase
    {
        /// <summary>
        /// 参数匹配设置
        /// </summary>
        /// <param name="props"></param>
        /// <param name="tos"></param>
        /// <returns></returns>
        internal override Result Match(List<DevicePropItemEntity> props, List<TransferObject> tos)
        {
            var pr = props.Where(x => x.PropName == "Port" || x.PropName == "Ip").Select(x => x.PropValue).ToList();
            return this.Match(props, tos, pr, "SocketUnit");
        }
            
        /// <summary>
        /// 错误字典
        /// </summary>
        Dictionary<int, string> ErrCode = new Dictionary<int, string>()
        {
            {0x55,"将运行中写入设置为不允许时，通过对象设备向 CPU 模块发出了运行中数据写入请求" },
            {0xC050,"在 “通信数据代码设置”中，设置了 ASCII 代码通信时，接收了不能转换为二进制代码的 ASCII 代码" },
            {0xC051,"写入或读取点数超出了允许范围" },
            {0xC052,"写入或读取点数超出了允许范围" },
            {0xC053,"写入或读取点数超出了允许范围" },
            {0xC054,"写入或读取点数超出了允许范围" },
            {0XC056,"写入及读取请求超出了最大地址" },
            {0XC058,"ASCII- 二进制转换后的请求数据长与字符部分 ( 文本的一部分 ) 的数据数不相符" },
            {0XC059,"指令、子指令的指定有误/是在 CPU 模块中禁止使用的指令、子指令" },
            {0XC05B," CPU 模块不能对指定软元件进行写入及读取" },
            {0XC05C,"请求内容中存在有错误。( 以位为单位对字软元件进行了写入及读取等)" },
            {0XC05D,"未进行监视登录" },
            {0XC05F,"是不能对对象 CPU 模块执行的请求" },
            {0XC060,"请求内容中存在有错误。( 对位软元件进行的数据指定中存在有错误等 ) " },
            {0XC061,"请求数据长与字符部分 ( 文本的一部分 ) 的数据数不相符" },
            {0XC06F,"“通信数据代码设置”为二进制时，接收了 ASCII 的请求报文。此外，设置为 ASCII 时，接收了二进制的请求报文" },
            {0XC070,"不能对对象站进行软元件存储器的扩展指定" },
            {0XC0B5,"指定了在 CPU 模块中不能使用的数据" },
            {0XC200,"远程口令有错误" },
            {0XC201,"通信所使用的端口处于远程口令的锁定状态/或者 “通信数据代码设置”为 ASCII 代码时，由于处于远程口令的锁定状态，因此无法将子指令以后转换为二进制代码" },
            {0XC204,"与发出了远程口令解锁处理请求的对象设备不相同" },
        };



        #region 读
        public override Result Read(List<CommAddress> variables)
        {
            //计算地址
            List<MitsublshiAddress> address = new List<MitsublshiAddress>();
            foreach (MitsublshiAddress addrs in variables)
            {
                address.Add(ConvetAddress_3E(addrs).Data);
                ReadAddressData(ConvetAddress_3E(addrs).Data);
            }
            /*可以先获取里面的数据集合是否有不一样的，再选择不一样的指令
             */
            List<byte> sendByte = new List<byte>()
            {
                0X50, 0X00 ,0X00 ,0XFF ,0XFF ,0X03 ,0X00,//一般固定不变

                0x0C,0x00,// 剩余字节长度
                0x0A,0x00,//监控时间设置

                0x01,0x04,// 成批读出指令
                address[0].IsByte,0X00,//读取字类型数据    00是字读取   01是位读取
            };
            //确认数据类型
            sendByte.Add((byte)(address[0].AreaAddress % 256));//长度
            sendByte.Add((byte)(address[0].AreaAddress / 256 % 256));
            sendByte.Add((byte)(address[0].AreaAddress / 256 / 256 % 256));

            sendByte.Add((byte)address[0].AreaType);//数据类型
            var readnumberword = address[0].Length * (Marshal.SizeOf(SetVariableType(address[0].DataType)) / 2);
            if (address[0].VariableType == typeof(bool)) readnumberword = address[0].Length;

            sendByte.Add((byte)((readnumberword) % 256));//读取长度
            sendByte.Add((byte)((readnumberword) / 256 % 256));


            var send_data = TransferObject.SendAndReceived(sendByte.ToList(), 9, 5000, CalcDataLength);
            if (!send_data.Status) return new Result(false, "读取失败");

            int errcord = BitConverter.ToInt32(new byte[4] { send_data.Data[9], send_data.Data[10], 0x00, 0x00 });

            if (errcord != 0) //错误值返回
                return new Result(false, ErrCode[errcord]);

            byte[] data = new byte[send_data.Data.Count - 11];
            //获取值
            Array.Copy(send_data.Data.ToArray(), 11, data, 0, data.Length);
            //再根据获取的值，进行数据转换
            //部分的数据会有辣么的缺少如读double的时候，则只会出现6字个字节的数据
            var changerdata = ConvertType(data, address[0].VariableType);

            return base.Read(variables);
        }

        //读取，单个地址读取
        private Result<List<object>> ReadAddressData(MitsublshiAddress address)
        {
            List<object> result = new List<object>();

            //后期可考虑吓，如果读取的长度过长，可分批读取

            List<byte> sendByte = new List<byte>()
            {
                0X50, 0X00 ,0X00 ,0XFF ,0XFF ,0X03 ,0X00,//一般固定不变

                0x0C,0x00,// 剩余字节长度
                0x0A,0x00,//监控时间设置

                0x01,0x04,// 成批读出指令
                address.IsByte,0X00,//读取字类型数据    00是字读取   01是位读取
            };
            int typeLen = 1;
            //确认数据类型
            sendByte.Add((byte)(address.AreaAddress % 256));//长度
            sendByte.Add((byte)(address.AreaAddress / 256 % 256));
            sendByte.Add((byte)(address.AreaAddress / 256 / 256 % 256));

            sendByte.Add((byte)address.AreaType);//数据类型
            
            if (address.VariableType != typeof(bool))
                typeLen = (Marshal.SizeOf(address.VariableType) / 2);

            sendByte.Add((byte)((typeLen* address.Length) % 256));//读取长度
            sendByte.Add((byte)((typeLen * address.Length) / 256 % 256));

            var send_data = TransferObject.SendAndReceived(sendByte.ToList(), 9, 5000, CalcDataLength);
            if (!send_data.Status) return new Result<List<object>>(false, "读取失败");

            int errcord = BitConverter.ToInt32(new byte[4] { send_data.Data[9], send_data.Data[10], 0x00, 0x00 });

            if (errcord != 0) //错误值返回
                return new Result<List<object>>(false, ErrCode[errcord]);

            //获取值
            List<byte> data = send_data.Data.GetRange(11, send_data.Data.Count - 11);
            if (address.VariableType == typeof(bool))
            {
                // 每个字节有两个状态    高4位一个状态   0001    True    0000 False       低4位状态同上
                int index = 0;
                for (int i = 0; i < address.Length; i++)
                {
                    dynamic bValue = false;
                    if (i % 2 == 0)
                    {
                        bValue = (data[index] & (1 << 4)) != 0;
                    }
                    else
                    {
                        bValue = (data[index] & 1) != 0;
                        index++;
                    }
                    result.Add(bValue);
                }
            }
            else
            {
                int len = typeLen * 2;// 当前数据类型所需的字节数
                for (int i = 0; i < data.Count; i+=len) 
                {
                    List<byte> vBytes = data.GetRange(i, len);//获取字节
                    result.Add(ConvertType(vBytes.ToArray(), address.VariableType));
                }
            }

          return new Result<List<object>>() { Data = result };
        }

        #endregion

        #region 写
        /// <summary>
        /// Q系列的不支持该功能，只在iq系列才支持
        /// </summary>
        /// <param name="Prameter"></param>
        /// <returns></returns>
        public override Result MultiRead(List<CommAddress> Prameter)
        {
            var parameters = MultiReadAddressChanger(Prameter);
            ushort num = (ushort)(parameters.Count * 6 + 8);//8指超时时间，指令，子指令，字软元件块，位软件块
            List<byte> bytes = new List<byte>
            {
                0x50,0x00,
                0x00,
                0xFF,
                0xFF,0x03,
                0x00
            };
            // 后续字节数
            bytes.AddRange(BitConverter.GetBytes(num));
            // PLC处理超时时间
            bytes.AddRange(new byte[] { 0x0A, 0x00 });
            // 指令
            bytes.AddRange(new byte[] { 0x06, 0x04 });
            // 子指令
            bytes.AddRange(new byte[] { 0x00, 0x00 });

            int wordParameter = parameters.Where(x => (x.IsByte == 0x00)).ToList().Count;
            int byteParameter = parameters.Where(x => (x.IsByte != 0x00)).ToList().Count;

            // 计算两种类型的处理块数
            bytes.Add(BitConverter.GetBytes(wordParameter)[0]);// 拼接字点数
            bytes.Add(BitConverter.GetBytes(byteParameter)[0]);// 拼接位点数 
                                                               //填充字节

            ///字可以说排序下去，
            /*
             * 如果填充的数量太大，但其实内部有部分是不需要的，那么会有很多
             * 使用是需要在填充到这条指令前，把读取的数量都区分出来
             */

            //排序
            var ParameterList = parameters.OrderByDescending(x => x.IsByte == 0).ToList();
            foreach (MitsublshiAddress parameter in ParameterList)
            {
                List<byte> startBytes = new List<byte>();
                bytes.Add((byte)(parameter.AreaAddress % 256));//位置起始地址
                bytes.Add((byte)(parameter.AreaAddress / 256 % 256));
                bytes.Add((byte)(parameter.AreaAddress / 256 / 256 % 256));

                bytes.Add((byte)parameter.AreaType);//读取区域代码号

                ushort readnumberword = (ushort)(parameter.Length * (Marshal.SizeOf(SetVariableType(parameter.DataType)) / 2));
                if (parameter.IsByte == 1) readnumberword = (ushort)parameter.Length;

                bytes.AddRange(BitConverter.GetBytes(readnumberword));// 区分D  字区   X  位区   不需要
            }
            var send_data = TransferObject.SendAndReceived(bytes.ToList(), 9, 5000, CalcDataLength);
            if (!send_data.Status) return new Result(false, "读取失败");

            int errcord = BitConverter.ToInt32(new byte[4] { send_data.Data[9], send_data.Data[10], 0x00, 0x00 });

            if (errcord != 0) //错误值返回
                return new Result(false, ErrCode[errcord]);

            byte[] data = new byte[send_data.Data.Count - 11];
            //获取值
            Array.Copy(send_data.Data.ToArray(), 11, data, 0, data.Length);


            return new Result();
        }

        /// <summary>
        /// 写指令
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Prameter"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public override Result Write<T>(CommAddress Prameter, List<T> values)
        {

            List<byte> bytes = new List<byte>()
            {
               0X50, 0X00 ,0X00 ,0XFF ,0XFF ,0X03 ,0X00,//一般固定不变
            };
            List<byte> LenBytes = new List<byte>();
            var Convetdata = ConvetAddress_3E(Prameter).Data;
            int typeLen = 1;
            if (typeof(T) != typeof(bool))
            {
                typeLen = Marshal.SizeOf<T>() / 2;   // 当前类型下，每个数据需要多个地址的寄存器
            }
            LenBytes.AddRange(new byte[2] { 0x0A, 0x00 });//监控时间设置
            LenBytes.AddRange(new byte[2] { 0x01, 0x14 });//成批写入指令
            LenBytes.AddRange(new byte[2] { Convetdata.IsByte, 0X00 });//类型数据    00是字   01是位
            if (Convetdata.VariableType != typeof(T))
            {
                return new Result(false, "数量类型不一致");
            }
            LenBytes.Add((byte)(Convetdata.AreaAddress % 256));//位置起始地址
            LenBytes.Add((byte)(Convetdata.AreaAddress / 256 % 256));
            LenBytes.Add((byte)(Convetdata.AreaAddress / 256 / 256 % 256));

            //LenBytes.AddRange(BitConverter.GetBytes((ushort)(Convetdata.AreaAddress)));//起始写入地址
            LenBytes.Add((byte)Convetdata.AreaType);//区域代码号
            LenBytes.AddRange(BitConverter.GetBytes((short)(typeLen * values.Count)));//写入寄存器数量
            if (Convetdata.IsByte == 0x01)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        LenBytes.Add(0x00);
                    }
                    bool state = bool.Parse(values[i].ToString());

                    if (state)
                    {
                        byte arg = 0x01;
                        if (i % 2 == 0)
                            arg *= 16;      // 按位或  0001 0000
                        LenBytes[bytes.Count - 1] |= arg;
                    }
                }

            }
            else
            {

                LenBytes.AddRange(ConvetWriteddata<T>(values));
                //foreach (dynamic item in values)
                //{
                //    LenBytes.Add(BitConverter.GetBytes(item));
                //}
            }
            bytes.AddRange(BitConverter.GetBytes((short)LenBytes.Count));
            bytes.AddRange(LenBytes);
            var send_data = TransferObject.SendAndReceived(bytes.ToList(), 9, 5000, CalcDataLength);
            if (!send_data.Status) return new Result(false, "读取失败");

            int errcord = BitConverter.ToInt32(new byte[4] { send_data.Data[9], send_data.Data[10], 0x00, 0x00 });

            if (errcord != 0) //错误值返回
                return new Result(false, ErrCode[errcord]);

            return new Result();
        }
        #endregion

        #region 辅助函数


        /// <summary>
        /// 写数据将写入的数据转换成字节
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        private byte[] ConvetWriteddata<T>(List<T> values)
        {
            List<byte> redata = new List<byte>();
            var type = typeof(T);
            switch (type.Name)
            {
                case "Int32":
                    foreach (var item in values)
                    {
                        redata.AddRange(BitConverter.GetBytes(Convert.ToInt32(item)));
                    }
                    break;
                case "Int16":
                    foreach (var item in values)
                    {
                        redata.AddRange(BitConverter.GetBytes(Convert.ToInt16(item)));
                    }
                    break;
                case "UInt32":
                    foreach (var item in values)
                    {
                        redata.AddRange(BitConverter.GetBytes(Convert.ToUInt32(item)));
                    }
                    break;
                case "UInt16":
                    foreach (var item in values)
                    {
                        redata.AddRange(BitConverter.GetBytes(Convert.ToUInt16(item)));
                    }
                    break;
                case "Double":
                    foreach (var item in values)
                    {
                        redata.AddRange(BitConverter.GetBytes(Convert.ToDouble(item)));
                    }
                    break;
                case "Single":
                    foreach (var item in values)
                    {
                        redata.AddRange(BitConverter.GetBytes(Convert.ToSingle(item)));
                    }
                    break;
                case "String":
                    foreach (var item in values)
                    {
                        redata.AddRange(Encoding.Unicode.GetBytes(Convert.ToString(item)));
                    }
                    break;
                default:
                    break;
            }
            return redata.ToArray();
        }
        /// <summary>
        /// 批量读取地址转换（多块）
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<MitsublshiAddress> MultiReadAddressChanger(List<CommAddress> parameters)
        {
            List<MitsublshiAddress> Listaddresses = new List<MitsublshiAddress>();
            foreach (CommAddress address in parameters) 
            {
                Listaddresses.Add(ConvetAddress_3E(address).Data);
            }
            return Listaddresses;
        }

        //计算数据长度
        private int CalcDataLength(byte[] data)
        {
            int length = 0;
            if (data != null && data.Length > 0)
            {
                length = BitConverter.ToInt16(new byte[] {data[7], data[8],0, 0});
            }
            return length;
        }


        /// <summary>
        /// 根据数据类型进行转换
        /// </summary>
        /// <param name="valueBytes">字节数</param>
        /// <param name="type">数据类型</param>
        /// <returns></returns>
        public Result<object> ConvertType(byte[] valueBytes, Type type)
        {
            Result<object> result = new Result<object>();

            try
            {
                if (type == typeof(bool))
                {
                    result.Data = valueBytes[0] == 0x01;
                }
                else if (type == typeof(string))
                {
                    result.Data = Encoding.UTF8.GetString(valueBytes);
                }
                else
                {
                    // 这里不需要字节调整
                    Type tBitConverter = typeof(BitConverter);
                    MethodInfo method = tBitConverter.GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .FirstOrDefault(mi => mi.ReturnType == type && mi.GetParameters().Length == 2) as MethodInfo;
                    if (method == null)
                        throw new Exception("未找到匹配的数据类型转换方法");

                    result.Data = method?.Invoke(tBitConverter, new object[] { valueBytes.ToArray(), 0 });
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
            }
            return result;
        }
        #endregion
    }
}

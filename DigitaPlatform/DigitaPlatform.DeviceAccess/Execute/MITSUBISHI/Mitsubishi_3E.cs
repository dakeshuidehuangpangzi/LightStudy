using DigitaPlatform.DeviceAccess.Base;
using DigitaPlatform.DeviceAccess.Transfer;
using DigitaPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public override Result Read(List<CommAddress> variables)
        {
            //List<List<MitsublshiAddress>> grudess = new List<List<MitsublshiAddress>>();

           // var readtable = variables.Where(x => x is MitsublshiAddress).ToList();//找到所有的
            //计算地址
            List<MitsublshiAddress> address = new List<MitsublshiAddress>();
            foreach (MitsublshiAddress addrs in variables)
            {
                address.Add(ConvetAddress_3E(addrs).Data);
            }

            /*可以先获取里面的数据集合是否有不一样的，再选择不一样的指令
             */
            List<byte> sendByte=new List<byte>()
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
            if (address[0].VariableType == typeof(bool)) readnumberword= address[0].Length;

            sendByte.Add((byte)(( readnumberword) % 256));//读取长度
            sendByte.Add((byte)((readnumberword) / 256 % 256));

           var send_data  = TransferObject.SendAndReceived(sendByte.ToList(),9, 5000 , CalcDataLength);
           if (!send_data.Status) return new Result(false, "读取失败");

            int errcord = BitConverter.ToInt32(new byte[4] { send_data.Data[9], send_data.Data[10], 0x00, 0x00 });
            
            if (errcord!=0) //错误值返回
                return new Result(false, ErrCode[errcord]);

            byte[] data = new byte[send_data.Data.Count-11];
            //获取值
            Array.Copy(send_data.Data.ToArray(), 11, data, 0, data.Length);
            //再根据获取的值，进行数据转换
            //部分的数据会有辣么的缺少如读double的时候，则只会出现6字个字节的数据
            var changerdata = ConvertType(data, address[0].VariableType);


           return base.Read(variables);
        }


        public Result MultiRead(List<CommAddress> parameters)
        {
            int num = parameters.Count * 6 + 8;//8指超时时间，指令，子指令，字软元件块，位软件块
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
            bytes.AddRange(new byte[] { 0x06, 0x04 });

            int wordParameter= parameters.Select(x=>(x.IsByte == 0)).ToList().Count;
            int byteParameter = parameters.Select(x => (x.IsByte == 1)).ToList().Count;

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
            var ParameterList=parameters.OrderBy(x=>x.IsByte==0).ToList();
            foreach(MitsublshiAddress parameter in ParameterList) 
            {
                List<byte> startBytes = new List<byte>();
                string str = parameter.AreaAddress.ToString().PadLeft(6, '0');
                bytes.AddRange(Encoding.Default.GetBytes(str));//地址位置
                bytes.Add((byte)parameter.AreaType);
                var readnumberword = parameter.Length * (Marshal.SizeOf(SetVariableType(parameter.DataType)) / 2);
                if (parameter.VariableType == typeof(bool)) readnumberword = parameter.Length;
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

    }
}

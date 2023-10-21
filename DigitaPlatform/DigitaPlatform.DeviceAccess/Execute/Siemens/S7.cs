using DigitaPlatform.DeviceAccess.Base;
using DigitaPlatform.DeviceAccess.Transfer;
using DigitaPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DigitaPlatform.DeviceAccess.Execute.S7
{
    internal class S7:ExecuteObject
    {
        private int _pduSize = 240;

        internal override Result Match(List<DevicePropItemEntity> props, List<TransferObject> tos)
        {
            var pr = props.Where(x => x.PropName == "Port" || x.PropName == "Ip").Select(x => x.PropValue).ToList();
            return this.Match(props, tos, pr, "SocketUnit");//连接
        }

        Dictionary<byte, string> HeaderErrors = new Dictionary<byte, string>()
        {
            {0x00,"无错误" },
            {0x81,"应用程序关系错误" },
            {0x82,"对象定义错误" },
            {0x83,"无资源可用错误" },
            {0x84,"服务处理错误" },
            {0x85,"请求错误" },
            {0x87,"访问错误" }
        };

        Dictionary<byte, string> DataItemReturnCodes = new Dictionary<byte, string>()
        {
            { 0xff,"请求成功"},
            { 0x01,"硬件错误"},
            { 0x03,"对象不允许访问"},
            { 0x05,"地址越界，所需的地址超出此PLC的极限"},
            { 0x06,"请求的数据类型与存储类型不一致"},
            { 0x07,"日期类型不一致"},
            { 0x0a,"对象不存在"}
        };

        Dictionary<string, SiemensAreaTypes> AreaTypeDic = new Dictionary<string, SiemensAreaTypes>()
        {
            {"I",SiemensAreaTypes.INPUT },
            {"Q",SiemensAreaTypes.OUTPUT },
            {"M",SiemensAreaTypes.MERKER },
            {"V",SiemensAreaTypes.DATABLOCK }
        };

        /// <summary>
        /// 地址解析
        /// </summary>
        /// <param name="address">输入地址</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private SiemensAddress AnalysisAddress(CommAddress address)
        {
            SiemensAddress siemensAddress = new SiemensAddress();
            siemensAddress.DBNumber = 0;
            string str = address.VariableName.Substring(0, 2).ToUpper();//获取头部两个字符串
            if (str=="DB")
            {
                string[] strArrays = address.VariableName.Split('.');//DB1.100

                // 区域类型  DB
                siemensAddress.AreaType = SiemensAreaTypes.DATABLOCK;
                //DB块号
                siemensAddress.DBNumber = int.Parse(strArrays[0].Substring(2));
                //位置
                siemensAddress.ByteAddress = int.Parse(strArrays[1]);
                //数据类型转换的长度
                siemensAddress.ByteCount = Marshal.SizeOf(address.VariableType);
                //位DB块
                if (strArrays.Length == 3 && int.TryParse(strArrays[2], out int bitValue))
                {
                    if (bitValue > 7)
                        throw new Exception("Bit地址设置错误，只允许在0-7范围内");
                    siemensAddress.BitAddress = (byte)bitValue;

                    siemensAddress.ByteCount = 1;
                }

            }
            else if (new string[] { "I", "Q", "M", "V" }.Contains(address.VariableName.Substring(0, 1).ToUpper()))
            {
                if (str[0].ToString() == "V")
                    siemensAddress.DBNumber = 1;

                if (AreaTypeDic.ContainsKey(str[0].ToString()))
                {
                    siemensAddress.AreaType = AreaTypeDic[str[0].ToString()];//地址类型
                }
                siemensAddress.ByteCount = Marshal.SizeOf(address.VariableType);//需要字节数

                string[] addres = address.VariableName.Split('.');
                siemensAddress.ByteAddress = int.Parse(addres[0].Substring(1));
                if (address.Length == 2 && int.TryParse(addres[1], out int bitValue))//判断 是否存在bit类型  I0.0  I10.1这些
                {
                    if (bitValue > 7)
                        throw new Exception("Bit地址设置错误，只允许在0-7范围内");
                    siemensAddress.BitAddress = (byte)bitValue;

                    siemensAddress.ByteCount = 1;
                }
            }
            if (address.VariableType == typeof(bool))
                siemensAddress.ByteCount = 1;

            return siemensAddress;

        }

        /// <summary>
        /// 组地址
        /// </summary>
        /// <param name="variables"></param>
        /// <returns></returns>
        public Result<List<CommAddress>> GroupAddress(List<CommAddress> variables)
        {
            Result<List<CommAddress>> result=new Result<List<CommAddress>>();

            SiemensAddress sa = new SiemensAddress();
            SiemensAddress saLast = null;
            foreach (var variable in variables) 
            {
                SiemensAddress saAddress=this.AnalysisAddress(variable);

                if (saAddress.AreaType != sa.AreaType)
                {
                    saLast = saAddress;
                    sa = new SiemensAddress(saAddress); // 当前组的第一个
                    result.Data.Add(sa);
                }
                else
                {
                    if (saAddress.ByteAddress - sa.ByteAddress + saAddress.ByteCount - saLast.ByteCount > 120)//字节的长度
                    {
                        saLast = saAddress;
                        sa = new SiemensAddress(saAddress); // 当前组的第一个
                        result.Data.Add(sa);
                    }
                    else
                    {
                        // 没有超越的情况下，把地址长度增加
                        // item:当前这个地址
                        // lastMa:上一次的地址信息
                        sa.ByteCount += saAddress.ByteAddress - saLast.ByteAddress + saAddress.ByteCount - saLast.ByteCount;
                    }
                }
                sa.Variables.Add(saAddress);

                saLast = saAddress;
            }
            return result;
        }



        public List<byte> GetParameterItemByte(SiemensAddress sa)
        {
            List<byte> result = new List<byte>() { 0X12,0X0A,0X10};
            result.Add(0X02);
            paramBytes.Add((byte)(sa.ByteCount / 256 % 256));
            paramBytes.Add((byte)(sa.ByteCount % 256));
            // DB块编号   200Smart V区  DB1
            paramBytes.Add((byte)(sa.DBNumber / 256 % 256));
            paramBytes.Add((byte)(sa.DBNumber % 256));
            // 数据区域
            paramBytes.Add((byte)sa.AreaType);  //81 I   82  Q   83  M   84DB
                                                // 地址
                                                // Byte:100   Bit:0

            //int address = startAddr * 8 + bitAddr;
            int addr = (sa.ByteAddress << 3);
            paramBytes.Add((byte)(addr / 256 / 256 % 256));
            paramBytes.Add((byte)(addr / 256 % 256));
            paramBytes.Add((byte)(addr % 256));

            return paramBytes;

        }
        #region 握手通讯


        private  Result connect()
        {
            Result result = new Result();
            try
            {
                if (this.TransferObject == null) throw new Exception("通讯组件不可用");
                //连接请求
                var prop = Props.FirstOrDefault(x => x.PropName == "TryCount");//重试次数
                int tryCount = 30;
                if (prop != null) int.TryParse(prop.PropValue, out tryCount);
                Result connectState= this.TransferObject.Connect(tryCount);//第一次握手发送
                if (!connectState.Status) return connectState;

                //第1次握手  COTP报文    需要卡槽号和机架号
                prop = this.Props.FirstOrDefault(p => p.PropName == "Rack");//获取机架号
                int rack = 0;
                if (prop != null)
                    int.TryParse(prop.PropValue, out rack);
                prop = this.Props.FirstOrDefault(p => p.PropName == "Slot");//获取卡槽号
                int slot = 0;
                if (prop != null)
                    int.TryParse(prop.PropValue, out slot);

                Result cotpState = COTPConnection(rack, slot);
                if (!cotpState.Status) return cotpState;

                //第2次  SetupCommunication报文
                Result setupState = SetupCommunication();
                if (!setupState.Status) return setupState;


            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
            }
            return result;

        }
        private Result COTPConnection(int rack, int slot)
        {
            Result result = new Result();

            List<byte> cotpBytes = new List<byte> {
                // TPKT
                0x03,0x00,
                0x00,0x16,//请求字节
                // COTP
                0x11,//当前字节以后的字节数
                0xe0,//PDU Type，
                0x00,0x00,//DST  目标引用
                0x00,0x00,//SRC   源引用
                0x00,//扩展格式/流看着

              /*Parameter的排序可以任意的   ，主要看参数代码是判别
               */

                #region Parameter1
                 // Parameter-code  tpdu-size
                0xc0,//参数代码  TPDU-SIZE
                0x01,//参数长度
                0x0a,//TPDU大小
                #endregion

                #region Parameter2
                 // Parameter-code  src-tsap   指定上位机端
                0xc1,//参数代码   SRC  
                0x02,//参数长度
                0x10,//Source TSAP:01->PG;02->OP;03->S7单边（服务器模式）;0x10->S7双边通信
                0x00,//默认为0
	            #endregion

                #region Parameter3
                // Parameter-code  dst-tsap
                0xc2,//参数代码  dst
                0x02,//参数长度
                0x03,//Source TSAP:01->PG;02->OP;03->S7单边（服务器模式）;0x10->S7双边通信
                (byte)(rack*32+slot),//设定机架号和卡槽号
	            #endregion
               
            };

            try
            {
                var prop = this.Props.FirstOrDefault(p => p.PropName == "Timeout");
                int timeout = 5000;
                if (prop != null)
                    int.TryParse(prop.PropValue, out timeout);


                Result<List<byte>> resp = this.TransferObject.SendAndReceived(cotpBytes, 4, timeout, this.CalcDataLength);
                //解析响应报文
                //byte[] respBytes = new byte[22];
                //int count = socket.Receive(respBytes, 0, 22, SocketFlags.None);
                if (resp.Data[5] != 0xd0)
                {
                    throw new Exception("COTP连接响应异常");
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = "COTP连接未建立！" + ex.Message;
            }

            return result;
        }

        private Result SetupCommunication()
        {
            Result result = new Result();
            List<byte> setupBytes = new List<byte> {
                // TPKT
                0x03,0x00,0x00,0x19,
                // COTP
                0x02,0xf0,0x80,
                // Header
                0x32,0x01,
                0x00,0x00,0x00,0x00,
                // Parameter Length
                0x00,0x08,
                // Data Length
                0x00,0x00,
                // Parameter
                0xf0,//Function:Setup communication[
                0x00,//Reserved
                0x00,0x03,0x00,0x03,
                0x03,0xc0//PDU length   
            };
            try
            {
                var prop = this.Props.FirstOrDefault(p => p.PropName == "Timeout");
                int timeout = 5000;
                if (prop != null)
                    int.TryParse(prop.PropValue, out timeout);

                Result<List<byte>> setp_result = this.TransferObject.SendAndReceived(setupBytes, 4, timeout, this.CalcDataLength);
                //socket.Send(setupBytes);
                //byte[] respBytes = new byte[27];
                //int count = socket.Receive(respBytes);
                // 拿到PDU长度   后续进行报文组装和接收的时候可以参考
                byte[] pdu_size = new byte[2];
                pdu_size[0] = setp_result.Data[26];
                pdu_size[1] = setp_result.Data[25];

                this._pduSize = BitConverter.ToInt16(pdu_size);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = "Setup通信未建立！" + ex.Message;
            }
            return result;
        }
        #endregion

        private int CalcDataLength(byte[] data)
        {
            int length = 0;
            if (data != null && data.Length > 0)
            {
                length = BitConverter.ToUInt16(new byte[] { data[3], data[2] }) - 4;
            }
            return length;
        }
    }
}

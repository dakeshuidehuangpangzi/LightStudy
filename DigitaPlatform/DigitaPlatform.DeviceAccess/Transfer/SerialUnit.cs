using DigitaPlatform.DeviceAccess.Base;
using DigitaPlatform.Entities;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DigitaPlatform.DeviceAccess.Transfer
{
    internal class SerialUnit:TransferObject
    {
        private  readonly object trans_lock = new object();

        SerialPort serialPort;
        public SerialUnit()
        {
            serialPort = new SerialPort();
            this.TUnit = serialPort;
        }

        internal override Result Config(List<DevicePropItemEntity> props)
        {
            Result result = new Result();
            try
            {
                /*
                 * 
                 * 不能使用ForEach，如果内部出错后，会直接报多线程异常，不会具体到相应信息，最后还是使用
                 * foreach  当遍历到出问题，将立刻跳出来
                 props.ForEach(p => 
                {
                    object v = null;//用作转换完成后存放的临时变量
                    PropertyInfo pi = serialPort.GetType().GetProperty(p.PropName.Trim(), BindingFlags.Public | BindingFlags.Instance);
                    if (pi == null) return;

                    Type propType = pi.PropertyType;
                    if (propType.IsEnum)
                    {
                        v = Enum.Parse(propType, p.PropValue.Trim());//转换相应的枚举
                    }
                    else
                    {
                        v = Convert.ChangeType(p.PropValue.Trim(), propType);
                    }
                    pi.SetValue(serialPort, v);//找到变量值与串口对象相同属性名称，赋予值
                });

                 */
                foreach (var item in props)
                {
                    object v = null;//用作转换完成后存放的临时变量
                    PropertyInfo pi = serialPort.GetType().GetProperty(item.PropName.Trim(), BindingFlags.Public | BindingFlags.Instance);
                    if (pi == null) continue;

                    Type propType = pi.PropertyType;
                    if (propType.IsEnum)
                    {
                        v= Enum.Parse(propType, item.PropValue.Trim());//转换相应的枚举
                    }
                    else 
                    {
                        v = Convert.ChangeType(item.PropValue.Trim(), propType);
                    }
                    pi.SetValue(serialPort, v);//找到变量值与串口对象相同属性名称，赋予值
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
            }
            return result;
        }


        internal override Result Connect(int trycount = 30)
        {
            lock (trans_lock)
            {
                Result result = new Result();
                try
                {
                    int count = 0;
                    while (count < trycount)
                    {
                        if (serialPort.IsOpen)
                            break;

                        try
                        {
                            serialPort.Open();
                            break;
                        }
                        catch (IOException)
                        {
                            Task.Delay(1).GetAwaiter().GetResult();
                            count++;
                        }
                    }
                    if (serialPort == null || !serialPort.IsOpen)
                        throw new Exception("串口打开失败");

                    ConnectState = true;
                }
                catch (Exception e)
                {
                    result.Status = false;
                    result.Message = e.Message;
                }
                return result;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req">发送报文</param>
        /// <param name="receiveLen">读取长度</param>
        /// <param name="errorLen">错误长度</param>
        /// <param name="timeout">超时</param>
        /// <returns></returns>
        internal override Result<List<byte>> SendAndReceived(List<byte> req, int receiveLen, int errorLen, int timeout)
        {
            lock (trans_lock)
            {
                Result<List<byte>> result = new Result<List<byte>>();
                // 发送
                serialPort.Write(req.ToArray(), 0, req.Count);

                List<byte> respBytes = new List<byte>();
                try
                {
                    serialPort.ReadTimeout = timeout;
                    while (respBytes.Count < Math.Max(receiveLen, errorLen))
                    {
                        byte data = (byte)serialPort.ReadByte();
                        respBytes.Add(data);
                    }
                }
                catch (TimeoutException)
                {
                    if (respBytes.Count != errorLen && respBytes.Count != receiveLen)
                    {
                        result.Status = false;
                        result.Message = "接收报文超时";
                    }
                }
                catch (Exception e)
                {
                    result.Status = false;
                    result.Message = e.Message;
                }
                finally
                {
                    result.Data = respBytes;
                }
                return result;
            }
        }

        internal override Result Close()
        {
            if (serialPort != null)
                serialPort.Close();

            return base.Close();
        }


    }
}

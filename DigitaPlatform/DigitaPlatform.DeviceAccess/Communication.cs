using DigitaPlatform.DeviceAccess.Base;
using DigitaPlatform.DeviceAccess.Execute;
using DigitaPlatform.DeviceAccess.Transfer;
using DigitaPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DigitaPlatform.DeviceAccess
{
    public class Communication
    {
        private static Communication _instace;
        private static object _lock = new object();
        private Communication() { }

        public static Communication Create()
        {
            if (_instace == null)
            {
                lock (_lock)
                {
                    if (_instace == null)
                        _instace = new Communication();
                }
            }
            return _instace;
        }


        private  List<TransferObject> TransferList = new List<TransferObject>();

        public  Result<ExecuteObject> GetExecuteObject(List<DevicePropItemEntity> props)
        {
            Result<ExecuteObject> result = new Result<ExecuteObject>();

            try
            {
                // 创建执行单元
                var protocol = props.FirstOrDefault(p => p.PropName == "Protocol");
                if (protocol == null)
                {
                    throw new Exception("协议信息未知");
                }

                Type type = Assembly.Load("DigitaPlatform.DeviceAccess")
                           .GetType("DigitaPlatform.DeviceAccess.Execute." + protocol.PropValue);
                if (type == null)
                {
                    // 返回异常
                    throw new Exception("执行对象类型无效");
                }
                ExecuteObject eo = Activator.CreateInstance(type) as ExecuteObject;
                if (eo == null)
                {
                    // 返回异常
                    throw new Exception("执行对象创建失败");
                }

                var r1 = eo.Match(props, TransferList);//设置参数
                if (!r1.Status)
                {
                    result.Status = false;
                    result.Message = r1.Message;
                }
                else
                    result.Data = eo;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
            }
            return result;
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

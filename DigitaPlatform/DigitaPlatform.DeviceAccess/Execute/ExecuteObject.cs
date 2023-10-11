using DigitaPlatform.Common;
using DigitaPlatform.DeviceAccess.Base;
using DigitaPlatform.DeviceAccess.Transfer;
using DigitaPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitaPlatform.DeviceAccess.Execute
{
    public abstract class ExecuteObject
    {
        /// <summary>字节排序</summary>
        public EndianType EndianType { get; set; } = EndianType.ABCD;
        /// <summary>通讯方式</summary>

        internal TransferObject TransferObject {  get; set; }


        internal List<DevicePropItemEntity> Props { get; set; }

        /// <summary>
        /// 匹配创建通讯方式
        /// </summary>
        /// <param name="props">设置参数</param>
        /// <param name="tos">实例的对象</param>
        /// <param name="conditions">找到的串口或网口号的集合</param>
        /// <param name="protocol">创建的通讯协议名称</param>
        /// <returns></returns>
        internal Result Match(List<DevicePropItemEntity> props, List<TransferObject> tos, List<string> conditions, string protocol)
        {
            Result result = new Result();

            try
            {
                this.Props = props;

                var prop = props.FirstOrDefault(p => p.PropName == "Endian");//找到字节排序的变量
                if (prop != null)
                    this.EndianType = (EndianType)Enum.Parse(typeof(EndianType), prop.PropValue);

                // 从tos列表中找到PortName值一样的对象
                this.TransferObject = tos.FirstOrDefault(
                    to =>
                    to.GetType().Name == protocol //实例类的名称与创建的通讯协议名称要一致
                    && conditions.All(s => to.Conditions.Any(c => c == s))  // 匹配两上集合是否一致（串口需要匹配串口号，网口就得匹配IP和端口号）
                    );

                if (this.TransferObject == null)
                {
                    //使用反射找到要创建的对象

                    Type type = this.GetType().Assembly.GetType("DigitaPlatform.DeviceAccess.Transfer." + protocol);

                    this.TransferObject = (TransferObject)Activator.CreateInstance(type);//创建对象

                    this.TransferObject.Conditions = conditions;//创建的局部对象赋予当前对象

                    tos.Add(this.TransferObject);//添加到参数内的集合

                    // 初始化相关属性   
                    Result result_config = this.TransferObject.Config(props);
                    if (!result_config.Status)//参数填写失败
                        return result_config;//返回填写失败信息回去
                }

            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 参数匹配设置
        /// </summary>
        /// <param name="props"></param>
        /// <param name="tos"></param>
        /// <returns></returns>

        internal virtual Result Match(List<DevicePropItemEntity> props, List<TransferObject> tos) => new Result();

        public virtual void Connect() { }
        public virtual Result Read(List<CommAddress> variables) => new Result();
        public virtual void ReadAsync() { }
        //public virtual Result Write(List<CommAddress> addresses) => new Result();
        public virtual void WriteAsync() { }

        public virtual Result Dispose()
        {
            if (this.TransferObject == null) return new Result();
            try
            {
                this.TransferObject?.Close();
                return new Result();
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message);
            }
        }
        /// <summary>
        /// 表示将一个数据字节进行指定字节序的调整
        /// </summary>
        /// <param name="bytes">接收待转换的设备中返回的字节数组</param>
        /// <returns>返回调整完成的字节数组</returns>
        public List<byte> SwitchEndianType(List<byte> bytes)
        {
            // 不管是什么字节序，这个Switch里返回的是ABCD这个顺序
            List<byte> temp = new List<byte>();
            switch (EndianType)  // alt+enter
            {
                case EndianType.ABCD:
                    temp = bytes;
                    break;
                case EndianType.DCBA:
                    for (int i = bytes.Count - 1; i >= 0; i--)
                    {
                        temp.Add(bytes[i]);
                    }
                    break;
                case EndianType.CDAB:
                    temp = new List<byte> { bytes[2], bytes[3], bytes[0], bytes[1] };
                    break;
                case EndianType.BADC:
                    temp = new List<byte> { bytes[1], bytes[0], bytes[3], bytes[2] };
                    break;
            }
            if (BitConverter.IsLittleEndian)
                temp.Reverse();

            return temp;
        }

        public virtual Type SetVariableType(string DataType)
        {
            Type retype = null;
            string[] type = new string[8] 
            {
                "string","float","double","int","uint","bool","short","ushort"
            };

            if (!type.Contains(DataType)) throw new Exception("输入的数据类型不存在，请检查");
           
            switch (DataType) 
            {
                case "string": retype = typeof(string);break;
                case "double": retype = typeof(double); break;
                case "int": retype = typeof(int); break;
                case "float": retype = typeof(float); break;
                case "uint": retype = typeof(uint); break;
                case "short": retype = typeof(short); break;
                case "ushort": retype = typeof(ushort); break;
                case "bool": retype = typeof(bool); break;
            }
            return retype;
        }
    }
}

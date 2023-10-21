using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitaPlatform.DeviceAccess.Base
{
    public class CommAddress
    {
        /// <summary>变量名称</summary>
        public string VariableName { get; set; }
        /// <summary>变量数据类型</summary>
        public Type VariableType { get; set; }
        /// <summary>变量字节数据</summary>
        public byte[] ValueBytes { get; set; }
        /// <summary>变量获取值</summary>
        public object Value { get; set; }
        /// <summary>数据类型</summary>
        public string DataType { get ; set; }
        /// <summary>长度</summary>
        public int Length { get; set; }
        /// <summary>读取的值</summary>
        public int ReadNumber { get; set; }
        /// <summary>数据的类型，0代表按字，1代表按位</summary>
        public byte IsByte { get; set; }
        //变量集合
        public List<CommAddress> Variables { get; set; } = new List<CommAddress>();

    }
}

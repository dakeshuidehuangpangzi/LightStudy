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
        //变量集合
        List<CommAddress> Variables { get; set; } = new List<CommAddress>();

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitaPlatform.DeviceAccess.Base
{
    public class ModbusAddress : CommAddress
    {
        public ModbusAddress() { }
        public ModbusAddress(ModbusAddress address)
        {
            this.FuncCode = address.FuncCode;
            this.StartAddress = address.StartAddress;
            this.Length = address.Length;
        }
        /// <summary>功能码 </summary>
        public int FuncCode { get; set; }
        /// <summary>起始地址</summary>
        public int StartAddress { get; set; }
        /// <summary>长度</summary>
        public int Length { get; set; }
    }
}

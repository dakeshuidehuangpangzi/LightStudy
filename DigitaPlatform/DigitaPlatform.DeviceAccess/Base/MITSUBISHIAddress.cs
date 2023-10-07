using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitaPlatform.DeviceAccess.Base
{
    internal class MITSUBISHIAddress:CommAddress
    {
        public MITSUBISHIAddress()
        {
                
        }
        public MITSUBISHIAddress(MITSUBISHIAddress mi)
        {
            AreaType = mi.AreaType;
            AreaAddress = mi.AreaAddress;
            Length = mi.Length;
        }
        /// <summary>读取区域</summary>
        public MITSUBISHIArea AreaType { get; set; }
        /// <summary>地址位置</summary>
        public int AreaAddress { get; set; }
        /// <summary>长度</summary>
        public int Length { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitaPlatform.DeviceAccess.Base
{
    internal class SiemensAddress:CommAddress
    {
        public SiemensAddress() { }
        public SiemensAddress(SiemensAddress sa)
        {
            this.AreaType = sa.AreaType;
            this.DBNumber = sa.DBNumber;
            this.ByteAddress = sa.ByteAddress;
            this.BitAddress = sa.BitAddress;
            this.ByteCount = sa.ByteCount;
        }
        /// <summary>
        /// 数据访问区
        /// </summary>
        public SiemensAreaEnums AreaType { get; set; }
        public int DBNumber { get; set; } = 0;
        /// <summary>
        /// 地址分为两部分（Byte   Bit）
        /// </summary>
        public int ByteAddress { get; set; }
        public byte BitAddress { get; set; } = 0;
        /// <summary>
        /// 需要的字节个数
        /// </summary>
        public int ByteCount { get; set; }
    }
}

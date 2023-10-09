using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitaPlatform.DeviceAccess.Base
{
    public class MitsublshiAddress:CommAddress
    {
        public MitsublshiAddress()
        {
                
        }
        public MitsublshiAddress(MitsublshiAddress mi)
        {
            AreaType = mi.AreaType;
            AreaAddress = mi.AreaAddress;
            Length = mi.Length;
            DataType = mi.DataType;
            Format = mi.Format;
        }
        /// <summary>读取区域</summary>
        public MitsublshiAreaTypes AreaType { get; set; }
        /// <summary>地址位置</summary>
        public int AreaAddress { get; set; }
        /// <summary>长度</summary>
        public int Length { get; set; }
        /// <summary>
        /// 数据的类型，0代表按字，1代表按位
        /// </summary>
        public byte DataType { get; set; }
        /// <summary>
        /// 指示地址是10进制，还是16进制的
        /// </summary>
        public int Format { get; set; }

    }

    public class DataTypes
    {
        /// <summary>
        /// 数据的类型，0代表按字，1代表按位
        /// </summary>
        public byte DataType { get; set; }
        /// <summary>
        /// 指示地址是10进制，还是16进制的
        /// </summary>
        public int Format { get; set; }
    }
}

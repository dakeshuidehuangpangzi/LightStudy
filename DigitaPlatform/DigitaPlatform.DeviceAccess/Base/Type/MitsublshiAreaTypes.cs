using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitaPlatform.DeviceAccess.Base
{
    public enum MitsublshiAreaTypes
    {
        /// <summary>输入</summary>
        X = 0x9C,
        /// <summary>输出</summary>
        Y = 0x9D,
        /// <summary>内部继电器</summary>
        M = 0X90,
        /// <summary>锁存继电器</summary>
        //L = 0X92,
        /// <summary>报警器</summary>
        //F = 0X93,
        /// <summary>变址继电器</summary>
        //V = 0X94,
        /// <summary>链接继电器</summary>
        B = 0XA0,
        /// <summary>数据寄存器</summary>
        D = 0XA8,
        /// <summary>链接寄存器</summary>
        //W = 0XB4,
        /// <summary>定时器  触点</summary>
        //TS = 0XC1,
        /// <summary>定时器  线圈</summary>
        //TC = 0XC0,
        /// <summary>定时器  当前值</summary>
        //TN = 0XC2,
        /// <summary>累计定时器 触点</summary>
       // SS = 0XC7,
        /// <summary>累计定时器 线圈</summary>
        //SC = 0XC6,
        /// <summary>累计定时器  当前值</summary>
        //SN = 0XC8,
        /// <summary>计数器  触点</summary>
        //CS = 0XC4,
        /// <summary>计数器 线圈</summary>
        //CC = 0XC3,
        /// <summary>计数器 当前值</summary>
        //CN = 0XC5,
        /// <summary>链接特殊继电器</summary>
        //SB = 0XA1,
        /// <summary>链接特殊寄存器</summary>
       // SW = 0XB5,
        /// <summary>步进寄存器</summary>
        //S = 0X98,
        /// <summary>直接输入</summary>
        //DX = 0XA2,
        /// <summary>直接输出</summary>
       // DY = 0XA3,
        /// <summary>特殊继电器</summary>
        SM = 0X91,
        /// <summary>特殊寄存器</summary>
        SD = 0XA9,
        /// <summary>变址寄存器</summary>
        Z = 0XCC,
        /// <summary>文件寄存器</summary>
        R = 0XAF,
        /// <summary>文件寄存器</summary>
        ZR = 0XB0,

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitaPlatform.DeviceAccess.Execute.MITSUBISHI
{
    internal class Mitsubishi_3E: MitsubishiBase
    {
        /// <summary>
        /// 错误字典
        /// </summary>
        Dictionary<int, string> ErrCode = new Dictionary<int, string>()
        {
            {0x55,"将运行中写入设置为不允许时，通过对象设备向 CPU 模块发出了运行中数据写入请求" },
            {0xC050,"在 “通信数据代码设置”中，设置了 ASCII 代码通信时，接收了不能转换为二进制代码的 ASCII 代码" },
            {0xC051,"写入或读取点数超出了允许范围" },
            {0xC052,"写入或读取点数超出了允许范围" },
            {0xC053,"写入或读取点数超出了允许范围" },
            {0xC054,"写入或读取点数超出了允许范围" },
            {0XC056,"写入及读取请求超出了最大地址" },
            {0XC058,"ASCII- 二进制转换后的请求数据长与字符部分 ( 文本的一部分 ) 的数据数不相符" },
            {0XC059,"指令、子指令的指定有误/是在 CPU 模块中禁止使用的指令、子指令" },
            {0XC05B," CPU 模块不能对指定软元件进行写入及读取" },
            {0XC05C,"请求内容中存在有错误。( 以位为单位对字软元件进行了写入及读取等)" },
            {0XC05D,"未进行监视登录" },
            {0XC05F,"是不能对对象 CPU 模块执行的请求" },
            {0XC060,"请求内容中存在有错误。( 对位软元件进行的数据指定中存在有错误等 ) " },
            {0XC061,"请求数据长与字符部分 ( 文本的一部分 ) 的数据数不相符" },
            {0XC06F,"“通信数据代码设置”为二进制时，接收了 ASCII 的请求报文。此外，设置为 ASCII 时，接收了二进制的请求报文" },
            {0XC070,"不能对对象站进行软元件存储器的扩展指定" },
            {0XC0B5,"指定了在 CPU 模块中不能使用的数据" },
            {0XC200,"远程口令有错误" },
            {0XC201,"通信所使用的端口处于远程口令的锁定状态/或者 “通信数据代码设置”为 ASCII 代码时，由于处于远程口令的锁定状态，因此无法将子指令以后转换为二进制代码" },
            {0XC204,"与发出了远程口令解锁处理请求的对象设备不相同" },
        };
    }
}

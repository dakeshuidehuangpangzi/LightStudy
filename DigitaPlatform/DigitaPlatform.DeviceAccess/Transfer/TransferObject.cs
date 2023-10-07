using DigitaPlatform.DeviceAccess.Base;
using DigitaPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitaPlatform.DeviceAccess.Transfer
{
    /*连接通讯的，可以不需要外部查询里面的实际运行状况*/
    internal abstract class TransferObject
    {
        /// <summary>通讯单元</summary>

        public object TUnit { get; set; }

        /// <summary>连接状态</summary>
        internal bool ConnectState { get; set; } = false;

        internal List<string> Conditions = new List<string>();

        /// <summary>
        /// 参数设置
        /// </summary>
        /// <param name="props">设置通讯单元需要的信息</param>
        /// <returns></returns>
        internal virtual Result Config(List<DevicePropItemEntity> props) =>new Result();
        
        internal virtual Result Connect(int trycount = 30) =>new Result();

        internal virtual Result Close()
        {
            this.ConnectState = false;
            return new Result();
        }
#pragma warning disable CS8603 // 可能返回 null 引用。
        internal virtual Result<List<byte>> SendAndReceived(List<byte> req, int len1, int len2, int timeout) => null;
#pragma warning restore CS8603 // 可能返回 null 引用。

        // 参数：calcLen
        // 委托，作用是
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
        internal virtual Result<List<byte>> SendAndReceived(List<byte> req, int len1, int timeout, Func<byte[], int> calcLen = null)
#pragma warning restore CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
            => new Result<List<byte>>(false, "NULL");



    }
}

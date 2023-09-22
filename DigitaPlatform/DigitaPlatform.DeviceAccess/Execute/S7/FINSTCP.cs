using DigitaPlatform.DeviceAccess.Base;
using DigitaPlatform.DeviceAccess.Transfer;
using DigitaPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitaPlatform.DeviceAccess.Execute.S7
{
    internal class FINSTCP:ExecuteObject
    {
        /// <summary>
        /// 参数匹配写入
        /// </summary>
        /// <param name="props"></param>
        /// <param name="tos"></param>
        /// <returns></returns>
        internal override Result Match(List<DevicePropItemEntity> props, List<TransferObject> tos)
        {
           var pr=props.Where(x=>x.PropValue=="Port"||x.PropName=="Ip").Select(x=>x.PropValue).ToList();
            return this.Match(props, tos,pr, "SocketUnit");
        }
        /// <summary>
        ///连接   但读取和写入的时候如果都加连接请求，那这个cinnect方法其实可以不需要也行
        /// </summary>
        public override void Connect()
        {
            this.TransferObject.Connect();
        }

        public override Result Dispose()
        {
            return base.Dispose();
        }
    }
}

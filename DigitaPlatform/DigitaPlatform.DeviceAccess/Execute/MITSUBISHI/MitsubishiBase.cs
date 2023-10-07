using DigitaPlatform.DeviceAccess.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitaPlatform.DeviceAccess.Execute.MITSUBISHI
{
    internal abstract class MitsubishiBase: ExecuteObject
    {
        Dictionary <string, DataTypes> binary=new Dictionary<string, DataTypes>()
        {
            {"X",new DataTypes{ DataType=1,Format=16 } },
            {"Y",new DataTypes{ DataType=1,Format=16 } },
            {"M",new DataTypes{ DataType=1,Format=10 } },
            {"L",new DataTypes{ DataType=1,Format=10 } },
            {"F",new DataTypes{ DataType=1,Format=10 } },
            {"V",new DataTypes{ DataType=1,Format=10 } },
            {"B",new DataTypes{ DataType=1,Format=16 } },
            {"D",new DataTypes{ DataType=0,Format=10 } },
            {"W",new DataTypes{ DataType=0,Format=16 } },
            {"TS",new DataTypes{ DataType=0,Format=10 } },
            {"TN",new DataTypes{ DataType=1,Format=10 } },
            {"SS",new DataTypes{ DataType=1,Format=10 } },
            {"SC",new DataTypes{ DataType=1,Format=10 } },
            {"SN",new DataTypes{ DataType=1,Format=10 } },
            {"CS",new DataTypes{ DataType=1,Format=10 } },
            {"CC",new DataTypes{ DataType=1,Format=10 } },
            {"CN",new DataTypes{ DataType=1,Format=10 } },
            {"SB",new DataTypes{ DataType=0,Format=16 } },
            {"SW",new DataTypes{ DataType=0,Format=16 } },
            {"S",new DataTypes{ DataType=0,Format=10 } },
            {"DX",new DataTypes{ DataType=0,Format=16 } },
            {"DY",new DataTypes{ DataType=0,Format=16 } },
            {"SM",new DataTypes{ DataType=1,Format=10 } },
            {"SD",new DataTypes{ DataType=0,Format=10 } },
            {"Z",new DataTypes{ DataType=1,Format=10 } },
            {"R",new DataTypes{ DataType=0,Format=10 } },
            {"ZR",new DataTypes{ DataType=0,Format=10 } },

        };

        #region 地址解析
        /// <summary>
        /// 算出地址的参数内容
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Result<MitsublshiAddress> ConvetAddress_3E(string name)
        {
            string findAddress = name.ToUpper();
            bool isdouble = false;

            var addType = Enum.GetNames(typeof(MitsublshiAddress));   

            var find = addType.ToList().FindIndex(X =>
            {
                if (findAddress[0].ToString().Contains(X))//找到第一个字节
                {
                    int twofind = addType.ToList().FindIndex(b => b.Contains(findAddress.Substring(0, 2)));
                    if (twofind == -1) return true;
                    else return false;
                }
                else
                {
                    if (findAddress.Substring(0, 2) == X)
                    {
                        return isdouble =true;
                    }
                    return false;
                }
            });

            if (find == -1) return new Result<MitsublshiAddress>(false,$"寻找区域失败,错误地址：{name}");

            MitsublshiAddress address = new MitsublshiAddress()
            {
                VariableName = findAddress,
                AreaType = (MitsublshiAreaTypes)Enum.GetValues(typeof(MitsublshiAreaTypes)).GetValue(find),
                DataType = binary[addType[find]].DataType,
                Format = binary[addType[find]].Format,
                AreaAddress = isdouble==true? Convert.ToInt32(findAddress.Substring(2), binary[addType[find]].Format) 
                : Convert.ToInt32(findAddress.Substring(1), binary[addType[find]].Format),
            };
            return  new Result<MitsublshiAddress>() { Data= address };
        }
        #endregion
    }
}

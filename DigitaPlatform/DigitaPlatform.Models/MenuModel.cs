using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitaPlatform.Models
{
    public class MenuModel:ObservableObject
    {
        public int key { get; set; }
        /// <summary>
        /// 菜单头部信息
        /// </summary>

        public string MenuHeader { get; set; }


        private bool _isSelected;
        /// <summary>
        /// 选择
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set { Set( ref _isSelected , value); }
        }
        /// <summary>
        /// 菜单图标
        /// </summary>

        public string MenuIcon { get; set; }

        /// <summary>
        /// 
        /// </summary>

        public string TargetView { get; set; }


    }
}

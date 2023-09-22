using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitaPlatform.ViewModels
{
    public  class MainViewModel:ViewModelBase
    {
        private int _viewBlur = 0;

        public int ViewBlur
        {
            get { return _viewBlur; }
            set { Set(ref _viewBlur, value); }
        }

      //  public UserModel GlobalUserInfo { get; set; } = new UserModel();

        private object _viewContent;

        public object ViewContent
        {
            get { return _viewContent; }
            set { Set(ref _viewContent, value); }
        }

        private bool _isWindowClose;

        public bool IsWindowClose
        {
            get { return _isWindowClose; }
            set { Set(ref _isWindowClose, value); }
        }
        public MainViewModel()
        {
                
        }
    }
}

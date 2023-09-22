using DigitaPlatform.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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

        //菜单集合
        public List<MenuModel> Menus { get; set; }

        
        public RelayCommand<object> SwitchPageCommand { get; set; }

        public MainViewModel()
        {

            #region 页面按钮
            Menus = new List<MenuModel>();
            Menus.Add(new MenuModel
            {
                IsSelected = true,
                MenuHeader = "监控",
                MenuIcon = "\ue639",
                TargetView = "MonitorPage"
            });
            Menus.Add(new MenuModel
            {
                MenuHeader = "趋势",
                MenuIcon = "\ue61a",
                TargetView = "TrendPage"
            });
            Menus.Add(new MenuModel
            {
                MenuHeader = "报警",
                MenuIcon = "\ue60b",
                TargetView = "AlarmPage"
            });
            Menus.Add(new MenuModel
            {
                MenuHeader = "报表",
                MenuIcon = "\ue703",
                TargetView = "ReportPage"
            });
            Menus.Add(new MenuModel
            {
                MenuHeader = "配置",
                MenuIcon = "\ue60f",
                TargetView = "SettingsPage"
            });
            Menus.Add(new MenuModel
            {
                MenuHeader = "配置",
                MenuIcon = "\ue60f",
                TargetView = "SettingsPage"
            });
            Menus.Add(new MenuModel
            {
                MenuHeader = "配置",
                MenuIcon = "\ue60f",
                TargetView = "SettingsPage"
            });

            #endregion
            SwitchPageCommand = new RelayCommand<object>(ShowPage);
        }



        private void ShowPage(object obj)
        {
            // Bug：对象会重复创建，需要处理
            // 第80行解决

            var model = obj as MenuModel;
            if (model != null)
            {
                //if (GlobalUserInfo.UserType == 0 && model.TargetView != "MonitorPage")
                //{
                //    // 提示权限
                //    this.Menus[0].IsSelected = true;
                //    // 提示没有权限操作
                //    if (ActionManager.ExecuteAndResult<object>("ShowRight", null))
                //    {
                //        // 执行重新登录
                //        DoLogout();
                //    }
                //}
                //else
                //{
                    if (ViewContent != null && ViewContent.GetType().Name == model.TargetView) return;

                    Type type = Assembly.Load("Zhaoxi.DigitaPlatform.Views")
                        .GetType("Zhaoxi.DigitaPlatform.Views.Pages." + model.TargetView)!;
                    ViewContent = Activator.CreateInstance(type)!;
                //}
            }
        }

    }
}

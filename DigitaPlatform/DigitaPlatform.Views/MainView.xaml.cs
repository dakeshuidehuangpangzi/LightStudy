using DigitaPlatform.DeviceAccess;
using DigitaPlatform.DeviceAccess.Base;
using DigitaPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DigitaPlatform.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
            List<DevicePropItemEntity> devices = new List<DevicePropItemEntity>();
            devices.Add(new DevicePropItemEntity() { PropName = "Protocol", PropValue = "Mitsubishi_3E" } );
            devices.Add(new DevicePropItemEntity() { PropName = "Ip", PropValue = "192.168.3.39" } );
            devices.Add(new DevicePropItemEntity() { PropName = "Port", PropValue = "6001" });


            var data = communication.GetExecuteObject(devices);
            data.Data.Connect();
            List<CommAddress> address = new List<CommAddress>();
            address.Add(new MitsublshiAddress() { VariableName = "D100", Length = 1 });
            address.Add(new MitsublshiAddress() { VariableName = "D101", Length = 1 });
            address.Add(new MitsublshiAddress() { VariableName = "D102", Length = 1 });
            address.Add(new MitsublshiAddress() { VariableName = "D103", Length = 1 });
            address.Add(new MitsublshiAddress() { VariableName = "D104", Length = 1 });
            address.Add(new MitsublshiAddress() { VariableName = "D105", Length = 1 });

            var   us= data.Data.Read(address);
        }
        Communication communication = Communication.Create();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();   
        }
    }
}

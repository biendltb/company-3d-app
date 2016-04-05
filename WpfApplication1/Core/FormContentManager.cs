using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;

namespace TIS_3dAntiCollision.Core
{
    public static class WindowContentManager
    {
        private const string PLC_CONNECT_IMG = "pack://application:,,,/images/plc_connect.png";
        private const string PLC_DISCONNECT_IMG = "pack://application:,,,/images/plc_disconnect.png";
        private const string SENSOR_CONNECT_IMG = "pack://application:,,,/images/sensor_connect.png";
        private const string SENSOR_DISCONNECT_IMG = "pack://application:,,,/images/sensor_disconnect.png";
        private const string SCAN_ACTIVE_IMG = "pack://application:,,,/images/scan_active.png";
        private const string SCAN_DEACTIVE_IMG = "pack://application:,,,/images/scan_deactive.png";
        private const string MOVE_TO_IMG = "pack://application:,,,/images/move_to.png";
        private const string CHART_IMG = "pack://application:,,,/images/chart.png";

        public static Button GetDefaultBtn()
        {
            Button btn = new Button();
            btn.Width = 50;
            btn.Height = 50;
            btn.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            
            // set margin
            var m = btn.Margin;
            m.Right = 0;
            btn.Margin = m;

            // set style
            btn.SetResourceReference(Control.StyleProperty, "FlatButtonStyle");

            return btn;
        }

        public static void ActivePlcConnectBtn(Button plc_connect_btn)
        {
            setImageToButton(plc_connect_btn, PLC_CONNECT_IMG);
        }

        public static void DeactivePlcConnectBtn(Button plc_connect_btn)
        {
            setImageToButton(plc_connect_btn, PLC_DISCONNECT_IMG);
        }

        public static void ActiveSensorConnectBtn(Button sensor_connect_btn)
        {
            setImageToButton(sensor_connect_btn, SENSOR_CONNECT_IMG);
        }

        public static void DeactiveSensorConnectBtn(Button sensor_connect_btn)
        {
            setImageToButton(sensor_connect_btn, SENSOR_DISCONNECT_IMG);
        }

        public static void ActiveScanBtn(Button scan_btn)
        {
            setImageToButton(scan_btn, SCAN_ACTIVE_IMG);
        }

        public static void DeactiveScanBtn(Button scan_btn)
        {
            setImageToButton(scan_btn, SCAN_DEACTIVE_IMG);
        }

        public static void SetImageMoveBtn(Button move_to_btn)
        {
            setImageToButton(move_to_btn, MOVE_TO_IMG);
        }

        public static void SetImageChartBtn(Button chart_btn)
        {
            setImageToButton(chart_btn, CHART_IMG);
        }

        private static void setImageToButton(Button btn, string img_uri)
        {
            Image img = new Image();
            img.Source = new BitmapImage(new Uri(img_uri));

            StackPanel stkpnl = new StackPanel();
            stkpnl.Orientation = Orientation.Horizontal;
            stkpnl.Margin = new System.Windows.Thickness(5);
            stkpnl.Children.Add(img);

            btn.Content = stkpnl;
        }

    }
}

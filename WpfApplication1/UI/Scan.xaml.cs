using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TIS_3dAntiCollision.Core;
using TIS_3dAntiCollision.Business;
using TIS_3dAntiCollision.Services;
using TIS_3dAntiCollision.Business.Profiling;

namespace TIS_3dAntiCollision.UI
{
    /// <summary>
    /// Interaction logic for Scan.xaml
    /// </summary>
    public partial class Scan : Window
    {
        public Scan()
        {
            InitializeComponent();

            tb_start_x.Text = ConfigParameters.MIN_X_RANGE.ToString();
            tb_stop_x.Text = ConfigParameters.MAX_X_RANGE.ToString();
            tb_speed.Text = ConfigParameters.NORMAL_SPEED.ToString();
            tb_step_length.Text = ConfigParameters.DEFAULT_STEP_LENGTH.ToString();
            tb_plane_angle.Text = "0";
            tb_data_file_name.Text = "scan_data_" + ((int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString() + ".txt";
        }

        private void scan_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // check the PLC and sensor connection
                if (!PlcManager.GetInstance.IsConnect || !SensorManger.GetInstance.IsConnect)
                {
                    MessageBox.Show("Please check the PLC and Sensor connection.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // validate the x position
                if (!Validator.ValidXOutOfRange(double.Parse(tb_start_x.Text)) || !Validator.ValidXOutOfRange(double.Parse(tb_stop_x.Text)))
                    return;

                // trigger the scan
                SinglePlaneScanController.TriggerScan(double.Parse(tb_start_x.Text), double.Parse(tb_stop_x.Text),
                                            short.Parse(tb_speed.Text), double.Parse(tb_step_length.Text), double.Parse(tb_plane_angle.Text), tb_data_file_name.Text);

                this.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Please check all inputs.\n" + "Error: " + ex.ToString());
            }
        }

        private void cancel_btn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

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

namespace TIS_3dAntiCollision.UI
{
    /// <summary>
    /// Interaction logic for MoveTo.xaml
    /// </summary>
    public partial class MoveTo : Window
    {
        public MoveTo()
        {
            InitializeComponent();

            tb_speed.Text = ConfigParameters.NORMAL_SPEED.ToString();
        }

        private void ok_btn_Click(object sender, RoutedEventArgs e)
        {
            if (tb_x_pos.Text == "" || tb_speed.Text == "")
            {
                MessageBox.Show("Please check the input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!PlcManager.GetInstance.IsConnect)
            {
                MessageBox.Show("Please check the PLC connection", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string[] destination_arr = tb_x_pos.Text.Trim().Split(',');
            string[] speed_arr = tb_speed.Text.Trim().Split(',');

            if (destination_arr.Length != speed_arr.Length)
            {
                MessageBox.Show("Please check the input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            for (int i = 0; i < destination_arr.Length; i++)
            {

                if (double.Parse(destination_arr[i].Trim()) < ConfigParameters.MIN_X_RANGE || double.Parse(destination_arr[i].Trim()) > ConfigParameters.MAX_X_RANGE)
                {
                    MessageBox.Show("Destination is out of range", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Logger.Log("Add to itinerary: Destination: " + destination_arr[i].Trim() + " Speed: " + speed_arr[i].Trim());

                MovementController.AddMove(double.Parse(destination_arr[i].Trim()), (short)double.Parse(speed_arr[i].Trim()));
            }

            this.Close();
        }

        private void cancel_btn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

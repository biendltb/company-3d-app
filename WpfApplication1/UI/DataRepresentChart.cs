using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TIS_3dAntiCollision.Services;
using TIS_3dAntiCollision.Core;
using System.Windows.Media.Media3D;
using TIS_3dAntiCollision.Business;

namespace TIS_3dAntiCollision.UI
{
    public partial class DataRepresentChart : Form
    {
        private string file_path = "";
        private Point3D[] point_data;

        public DataRepresentChart(string data_file_path)
        {
            InitializeComponent();

            this.file_path = data_file_path;

            parseData();

            cropData();

            displaySideView();
            //displayTopView();
            //displayFrontView();
        }

        // add for testing purpose
        public DataRepresentChart(Point3D[] points)
        {
            InitializeComponent();

            ContainerStackProfiler csp = new ContainerStackProfiler(points);

            Point3D[] result_points = csp.GetMiddleStackProfile();
            //Point3D[] result_points = csp.GetSideStackProfile(true);

            foreach (Point3D point in points)
                chart1.Series["Series1"].Points.AddXY(point.X, point.Y);
        }

        private void parseData()
        {
            string data_file_content = DataStorageManager.ReadScanData(file_path);
            List<SingleScanData> multi_scan_data_list = ScanDataEncoder.Decode(data_file_content);
            Point3D[] point_arr = SensorOutputParser.Parse3DPoints(multi_scan_data_list.ToArray());
            List<Point3D> list_point = new List<Point3D>();
                        
            foreach (Point3D point in point_arr)
            {
                if (point.Z >= -330 && point.Z <= 270)
                    list_point.Add(point);
            }
            point_data = list_point.ToArray();
        }

        private void displaySideView()
        {
            chart1.Series["Series1"].Points.Clear();

            foreach (Point3D point in point_data)
                chart1.Series["Series1"].Points.AddXY(point.X, point.Y);
        }

        private void displayTopView()
        {
            chart1.Series["Series1"].Points.Clear();

            foreach (Point3D point in point_data)
                chart1.Series["Series1"].Points.AddXY(point.X, point.Z);
        }

        private void displayFrontView()
        {
            chart1.Series["Series1"].Points.Clear();

            foreach (Point3D point in point_data)
                chart1.Series["Series1"].Points.AddXY(point.Z, point.Y);
        }

        private void cropData()
        {
            List<Point3D> result = new List<Point3D>();

            for (int i = 0; i < point_data.Length; i++)
                if (point_data[i].X >= ConfigParameters.MIN_X_RANGE && point_data[i].X <= ConfigParameters.MAX_X_RANGE // valid x range
                    && point_data[i].Y >= ConfigParameters.MIN_Y_RANGE && point_data[i].Y <= ConfigParameters.MAX_Y_RANGE) // valide y range
                    result.Add(point_data[i]);

            point_data = result.ToArray();
        }
    }
}

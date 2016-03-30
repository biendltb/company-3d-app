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
            //Point3D[] result_points = csp.GetSideStackProfile();

            foreach (Point3D point in result_points)
                chart1.Series["Series1"].Points.AddXY(point.X, point.Y);
        }

        private void parseData()
        {
            string data_file_content = DataStorageManager.ReadScanData(file_path);
            List<SingleScanData> multi_scan_data_list = ScanDataEncoder.Decode(data_file_content);

            Point3D[][] multi_scan_3d_point = new Point3D[multi_scan_data_list.Count][];

            List<Point3D> list_point = new List<Point3D>();

            for (int i = 0; i < multi_scan_data_list.Count; i++)
            {
                multi_scan_3d_point[i] = SensorOutputParser.Parse3DPoints(multi_scan_data_list[i].XPos,
                                                                            multi_scan_data_list[i].PlaneAngle,
                                                                            multi_scan_data_list[i].ScanData).ToArray();
            }

            foreach (Point3D[] point_arr in multi_scan_3d_point)
                foreach (Point3D point in point_arr)
                {
                    if (point.Z >= -250 && point.Z <= 250)
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
    }
}

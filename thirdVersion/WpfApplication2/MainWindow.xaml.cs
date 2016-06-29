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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;

namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PerspectiveCamera myPCamera;

        public MainWindow()
        {
            InitializeComponent();
            WavefrontObjLoader wfl = new WavefrontObjLoader();
            slider1.ValueChanged += new RoutedPropertyChangedEventHandler<double>(slider1_ValueChanged);
            slider2.ValueChanged += new RoutedPropertyChangedEventHandler<double>(slider1_ValueChanged);
            slider3.ValueChanged +=new RoutedPropertyChangedEventHandler<double>(slider1_ValueChanged);

           myPCamera = new PerspectiveCamera();

           // Specify where in the 3D scene the camera is.
           myPCamera.Position = new Point3D(0, 0, 0);

           // Specify the direction that the camera is pointing.
           myPCamera.LookDirection = new Vector3D(0, 0, -1);

           // Define camera's horizontal field of view in degrees.
           myPCamera.FieldOfView = 1000;

           // Asign the camera to the viewport
           vp.Camera = myPCamera;

           Model3DGroup myModel3DGroup = new Model3DGroup();

           DirectionalLight myDirectionalLight = new DirectionalLight();
           myDirectionalLight.Color = Colors.White;
           myDirectionalLight.Direction = new Vector3D(-0.61, -0.5, -0.61);

           myModel3DGroup.Children.Add(myDirectionalLight);
           var m = wfl.LoadObjFile(@"F:\MeshedReconstruction.obj");
           m.Content =myModel3DGroup;
           vp.Children.Add(m);
        }

        void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            myPCamera.Position = new Point3D(slider2.Value , slider1.Value, slider3.Value);
        }
    }
}

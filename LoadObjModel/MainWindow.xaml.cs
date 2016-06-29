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
using System.Windows.Media.Animation;

namespace LoadObjModel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PerspectiveCamera camera = new PerspectiveCamera();

        Point mouseLastPosition;
        double mouseDeltaFactor = 2;// determine the angle delta when the mouse drag the 3D view
        double keyDeltaFactor = 4;// determine the angle delta when the ddirection key pressed

        public MainWindow()
        {
            InitializeComponent();

            WavefrontObjLoader wfl = new WavefrontObjLoader();
            slider1.ValueChanged += new RoutedPropertyChangedEventHandler<double>(slider1_ValueChanged);
            slider2.ValueChanged += new RoutedPropertyChangedEventHandler<double>(slider1_ValueChanged);
            slider3.ValueChanged +=new RoutedPropertyChangedEventHandler<double>(slider1_ValueChanged);

            // Specify where in the 3D scene the camera is.
            camera.Position = new Point3D(0, 0, 0);

            // Specify the direction that the camera is pointing.
            camera.LookDirection = new Vector3D(0, 0, -1);

            // Define camera's horizontal field of view in degrees.
            camera.FieldOfView = 1000;

            // Asign the camera to the viewport
            vp.Camera = camera;

            Model3DGroup myModel3DGroup = new Model3DGroup();

            DirectionalLight myDirectionalLight = new DirectionalLight();
            myDirectionalLight.Color = Colors.White;
            myDirectionalLight.Direction = new Vector3D(-0.61, -0.5, -0.61);

            myModel3DGroup.Children.Add(myDirectionalLight);
            var m = wfl.LoadObjFile(@"F:\MeshedReconstruction.obj");
            m.Content = myModel3DGroup;
            vp.Children.Add(m);

            camera.UpDirection.Normalize();
            this.MouseMove += Viewport3D_MouseMove;
            this.MouseLeftButtonDown += Viewport3D_MouseLeftButtonDown;
            this.MouseWheel += Viewport3D_MouseWheel;
            this.KeyDown += Window_KeyDown;

        }

        void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            camera.Position = new Point3D(slider2.Value , slider1.Value, slider3.Value);
        }

        private void Viewport3D_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                Point newMousePosition = e.GetPosition(this);

                if (mouseLastPosition.X != newMousePosition.X)
                {
                    HorizontalTransform(mouseLastPosition.X < newMousePosition.X, mouseDeltaFactor);
                }

                if (mouseLastPosition.Y != newMousePosition.Y)// change position in the horizontal direction
                {

                    VerticalTransform(mouseLastPosition.Y > newMousePosition.Y, mouseDeltaFactor);
                }
                mouseLastPosition = newMousePosition;
            }
        }

        private void Viewport3D_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseLastPosition = e.GetPosition(this);
        }

        private void VerticalTransform(bool upDown, double angleDeltaFactor)
        {
            Vector3D postion = new Vector3D(camera.Position.X, camera.Position.Y, camera.Position.Z);
            Vector3D rotateAxis = Vector3D.CrossProduct(postion, camera.UpDirection);
            RotateTransform3D rt3d = new RotateTransform3D();
            AxisAngleRotation3D rotate = new AxisAngleRotation3D(rotateAxis, angleDeltaFactor * (upDown ? -1 : 1));
            rt3d.Rotation = rotate;
            Matrix3D matrix = rt3d.Value;
            Point3D newPostition = matrix.Transform(camera.Position);
            camera.Position = newPostition;
            camera.LookDirection = new Vector3D(-newPostition.X, -newPostition.Y, -newPostition.Z);

            //update the up direction
            Vector3D newUpDirection = Vector3D.CrossProduct(camera.LookDirection, rotateAxis);
            newUpDirection.Normalize();
            camera.UpDirection = newUpDirection;
        }

        private void HorizontalTransform(bool leftRight, double angleDeltaFactor)
        {
            Vector3D postion = new Vector3D(camera.Position.X, camera.Position.Y, camera.Position.Z);
            Vector3D rotateAxis = camera.UpDirection;
            RotateTransform3D rt3d = new RotateTransform3D();
            AxisAngleRotation3D rotate = new AxisAngleRotation3D(rotateAxis, angleDeltaFactor * (leftRight ? -1 : 1));
            rt3d.Rotation = rotate;
            Matrix3D matrix = rt3d.Value;
            Point3D newPostition = matrix.Transform(camera.Position);
            camera.Position = newPostition;
            camera.LookDirection = new Vector3D(-newPostition.X, -newPostition.Y, -newPostition.Z);
        }

        private void Viewport3D_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scaleFactor = 3;
            //120 near ,   -120 far
            System.Diagnostics.Debug.WriteLine(e.Delta.ToString());
            Point3D currentPosition = camera.Position;
            Vector3D lookDirection = camera.LookDirection;//new Vector3D(camera.LookDirection.X, camera.LookDirection.Y, camera.LookDirection.Z);
            lookDirection.Normalize();

            lookDirection *= scaleFactor;

            if (e.Delta == 120)//getting near
            {
                if ((currentPosition.X + lookDirection.X) * currentPosition.X > 0)
                {
                    currentPosition += lookDirection;
                }
            }
            if (e.Delta == -120)//getting far
            {
                currentPosition -= lookDirection;
            }

            Point3DAnimation positionAnimation = new Point3DAnimation();
            positionAnimation.BeginTime = new TimeSpan(0, 0, 0);
            positionAnimation.Duration = TimeSpan.FromMilliseconds(100);
            positionAnimation.To = currentPosition;
            positionAnimation.From = camera.Position;
            positionAnimation.Completed += new EventHandler(positionAnimation_Completed);
            camera.BeginAnimation(PerspectiveCamera.PositionProperty, positionAnimation, HandoffBehavior.Compose);
        }

        void positionAnimation_Completed(object sender, EventArgs e)
        {
            //Set a Property After Animating It with a Storyboard:http://msdn.microsoft.com/en-us/library/aa970493.aspx
            Point3D position = camera.Position;
            camera.BeginAnimation(PerspectiveCamera.PositionProperty, null);
            camera.Position = position;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left: HorizontalTransform(false, keyDeltaFactor); break;
                case Key.Right: HorizontalTransform(true, keyDeltaFactor); break;
                case Key.Up: VerticalTransform(true, keyDeltaFactor); break;
                case Key.Down: VerticalTransform(false, keyDeltaFactor); break;
            }
        }
    }
}

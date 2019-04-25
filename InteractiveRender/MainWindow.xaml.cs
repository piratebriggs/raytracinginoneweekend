using raytracinginoneweekend;
using raytracinginoneweekend.Hitables;
using RenderLib;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace InteractiveRender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WriteableBitmap _bitmap;
        private RenderParameters _parameters;
        private Vector4[] _buffer;

        public MainWindow()
        {
            InitializeComponent();
            _parameters = new RenderParameters
            {
                nx = 300,
                ny = 300,
                tileSize = 10,
                ns = 10
            };

            _bitmap = new WriteableBitmap(_parameters.nx, _parameters.ny, 96, 96, PixelFormats.Rgb24, BitmapPalettes.WebPalette);
            image.Source = _bitmap;

            _buffer = new Vector4[_parameters.nx * _parameters.ny];

            Button_Click(null, null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            BackgroundWorker worker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            worker.DoWork += Worker_DoWork; ;
            worker.ProgressChanged += Worker_ProgressChanged; ;
            worker.RunWorkerAsync(_parameters);
        }

        /// <summary>
        /// Write current tile from _buffer to _bitmap
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            var currentTile =  (int)e.UserState;
            var (minx, miny, maxx, maxy) = _parameters.GetTileDetails(currentTile);

            var width = maxx - minx + 1;
            var height = maxy - miny + 1;

            var pixelBuffer = new byte[width * height * 3];
            var i = 0;
            for (var y = miny; y <= maxy; y++)
            {
                for (var x = minx; x <= maxx; x++)
                {
                    // No AsMemory() here in framework-land
                    var pix = _buffer[y * _parameters.ny + x];
                    pixelBuffer[i++] = (byte)((float)Math.Sqrt(pix.X) * 255);
                    pixelBuffer[i++] = (byte)((float)Math.Sqrt(pix.Y) * 255);
                    pixelBuffer[i++] = (byte)((float)Math.Sqrt(pix.Z) * 255);
                }
            }

            _bitmap.WritePixels(new Int32Rect(minx, miny, width, height), pixelBuffer, width*3, 0);

        }

        /// <summary>
        /// Render Image, reporting each tile as it's completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var p = (RenderParameters)e.Argument;

            var (world, cam) = Scenes.CornellScene("../../../SampleObj/teapot.obj", new SunsetquestRandom(), p.nx, p.ny);
            var worldBVH = new BVH(world);
            var wl = new IHitable[] { worldBVH };
            int totalRayCount = 0;
            var pathTracer = new PathTracer(p.nx, p.ny, p.ns, false);

            // Fill buffer with gradient 
            Parallel.For(0, p.TileCount, i =>
            {
                //Thread.Sleep(10);

                var (minx, miny, maxx, maxy) = _parameters.GetTileDetails(i);
                // draw a tile of gradient color that should tile smoothly for debug purposes
                for (var y = miny; y <= maxy; y++)
                {
                    for (var x = minx; x <= maxx; x++)
                    {
                        _buffer[y * _parameters.ny + x] += new Vector4((float)y / _parameters.ny, (float)x / _parameters.nx, (float)i / p.TileCount, 0);
                    }
                }
                //(sender as BackgroundWorker).ReportProgress(i, i);
            });

            // single pass render to p.ns samples
            Parallel.For(0, p.TileCount, i =>
            {
                var (minx, miny, maxx, maxy) = _parameters.GetTileDetails(i);
                uint tmpSampleCount = 0;
                var rayCount = pathTracer.RenderScene(wl, cam, _buffer, p.nx, ref tmpSampleCount, newSampleCount => { return newSampleCount < p.ns; }, miny, maxy, minx, maxx);

                (sender as BackgroundWorker).ReportProgress(i, i);
            });
        }
    }
}

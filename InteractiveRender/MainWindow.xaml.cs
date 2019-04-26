using raytracinginoneweekend;
using raytracinginoneweekend.Hitables;
using RenderLib;
using System;
using System.ComponentModel;
using System.Diagnostics;
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
        private BackgroundWorker _worker;

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

            _worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _worker.DoWork += Worker_DoWork; ;
            _worker.ProgressChanged += Worker_ProgressChanged; ;
            _worker.RunWorkerAsync(_parameters);

        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _worker.CancelAsync();
            Stop.IsEnabled = false;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Save.IsEnabled = false;
        }

        /// <summary>
        /// Write current tile from _buffer to _bitmap
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            var currentTile =  e.ProgressPercentage;
            var messsage = e.UserState.ToString();
            if (currentTile == -1)
            {
                Info.Content += messsage;
                return;
            }
            Info.Content = messsage;

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
            var worker = (sender as BackgroundWorker);

            var (world, cam) = Scenes.CornellScene("../../../SampleObj/teapot.obj", new SunsetquestRandom(), p.nx, p.ny);
            var worldBVH = new BVH(world);
            var wl = new IHitable[] { worldBVH };
            var pathTracer = new PathTracer(p.nx, p.ny, p.ns, false);

            var tileSampleCount = new uint[p.TileCount];

            var lockObj = new Object();
            uint totalRayCount = 0;
            var duration = new TimeSpan();


            while (!worker.CancellationPending)
            {
                // single pass render to p.ns samples
                Parallel.For(0, p.TileCount, i =>
                {
                    var (minx, miny, maxx, maxy) = _parameters.GetTileDetails(i);

                    var sw = Stopwatch.StartNew();
                    var inputSampleCount = tileSampleCount[i];
                    var tmpRayCount = pathTracer.RenderScene(wl, cam, _buffer, p.nx, ref tileSampleCount[i], newSampleCount => { return newSampleCount < inputSampleCount + p.ns; }, miny, maxy, minx, maxx);
                    sw.Stop();

                    string info;
                    lock(lockObj)
                    {
                        totalRayCount += tmpRayCount;
                        duration += sw.Elapsed;

                        float seconds = (float)duration.TotalMilliseconds / 1000f;
                        float rate = totalRayCount / seconds;
                        float mRate = rate / 1_000_000;

                        info = $"totalRayCount: {totalRayCount}\r\n";
                        info += $"BVH max depth: {worldBVH.MaxTestCount}\r\n";
                        info += $"Duration: {seconds} | Rate: {mRate} MRays / sec.\r\n";
                    }

                    worker.ReportProgress(i, info);
                });
            }
            worker.ReportProgress(-1, "Stopped");
        }

        /// <summary>
        /// Demo Renderer for debugging the ProgressChanged() event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="p"></param>
        private void RenderGradient(BackgroundWorker sender, RenderParameters p)
        {
            // Fill buffer with gradient 
            Parallel.For(0, p.TileCount, i =>
            {
                Thread.Sleep(10);

                var (minx, miny, maxx, maxy) = _parameters.GetTileDetails(i);
                // draw a tile of gradient color that should tile smoothly for debug purposes
                for (var y = miny; y <= maxy; y++)
                {
                    for (var x = minx; x <= maxx; x++)
                    {
                        _buffer[y * _parameters.ny + x] += new Vector4((float)y / _parameters.ny, (float)x / _parameters.nx, (float)i / p.TileCount, 0);
                    }
                }

                sender.ReportProgress(i, i);
            });
        }
    }
}

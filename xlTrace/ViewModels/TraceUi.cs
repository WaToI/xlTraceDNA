namespace xlTrace.ViewModels {
  using CsPotrace;
  using ExcelDna.Integration;
  using Microsoft.Office.Core;
  using Reactive.Bindings;
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Drawing;
  using System.Numerics;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows;
  using System.Windows.Media.Imaging;
  using xlTrace.Models;
  using XL = Microsoft.Office.Interop.Excel;
  partial class TraceUi : INotifyPropertyChanged, IDisposable {
    public event PropertyChangedEventHandler PropertyChanged;
    public ReactiveProperty<double> WindowOpacity = new ReactiveProperty<double>();
    public ReactiveProperty<CancelEventArgs> WindowClosing = new ReactiveProperty<CancelEventArgs>();
    public TraceUi() {
      WindowOpacity.Value = 0.6;
      GetCameraNames();
      setPoTrace();
      StartCameraAsync();
    }
    public void Dispose() {
      StopCamera();
    }
    public ReactiveProperty<double> CameraFps { get; set; } = new ReactiveProperty<double>(30d);
    public ReactiveProperty<double> CameraFpsAct { get; set; } = new ReactiveProperty<double>();
    DateTime lastTime;
    System.Timers.Timer timer;
    public ReactiveCollection<string> CameraNames { get; set; } = new ReactiveCollection<string>();
    SynchronizationContext sync;
    public Task CaptureTask { get; set; }
    public bool CaptureBusy { get; set; } = true;
    public CancellationTokenSource CaptureCancelTokenSrc { get; set; }
    public CancellationToken CaptureCancelToken { get; set; }
    UsbCamera cam;
    public Bitmap CapturedBmp { get; set; }
    public ReactiveProperty<BitmapFrame> CapturedBmf { get; set; } = new ReactiveProperty<BitmapFrame>();
    public void GetCameraNames() {
      foreach (var dev in UsbCamera.FindDevices()) {
        CameraNames.Add(dev);
      }
    }
    public void StartCameraAsync() {
      sync = SynchronizationContext.Current;
      CaptureTask = CaptureAsync();
    }
    public void StopCamera() {
      timer.Dispose();
      cam.Release();
      cam = null;
    }
    public Task CaptureAsync() {
      CaptureCancelTokenSrc = new CancellationTokenSource();
      CaptureCancelToken = CaptureCancelTokenSrc.Token;
      var camNo = 0;// UsbCamera.FindDevices().Length - 1;
      return Task.Run(() => {
        //cam = new UsbCamera(camNo, 320, 180);
        cam = new UsbCamera(camNo, 640, 360);
        //cam = new UsbCamera(camNo, 1280, 720);
        cam.Start();
        timer = new System.Timers.Timer(1000d / CameraFps.Value);
        timer.Elapsed += captureTimerElapsed;
        CaptureBusy = false;
        timer.Start();
      }, CaptureCancelToken);
    }
    /// <summary>
    /// キャプチャーメイン処理
    /// </summary>
    void captureTimerElapsed(object sender, System.Timers.ElapsedEventArgs e) {
      if (!CaptureBusy) {
        try {
          CaptureBusy = true;
          CameraFpsAct.Value = 1000d / (DateTime.Now - lastTime).Milliseconds;
          lastTime = DateTime.Now;
          CapturedBmp = NewFrame(cam);
          Potrace.Trace(CapturedBmp, (int)TraceThreshold, TracePitch);
          tracedSvgStr = traceSvgStr;
          tracedFaces = Potrace.TraceFaces(traceSvgStr);
          CapturedBmf.Value = CapturedBmp.ToBitmapFrame();
          TracedBmf.Value = tracedBmp.ToBitmapFrame();
          CaptureBusy = false;
        } catch (Exception) { }
      }
    }
    Func<UsbCamera, Bitmap> NewFrame = (c) => {
      return c.GetBitmap();
    };
    public double TraceThreshold = 100d;
    public double TracePitch = 1d;
    public string tracedSvgStr = "";
    public List<List<Vector3>> tracedFaces = null;
    public ReactiveCommand TakeOneShot { get; set; } = new ReactiveCommand();
    public void TraceThresholdChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      TraceThreshold = e.NewValue;
    }
    void setPoTrace() {

      TakeOneShot.Subscribe((_) => takeOneShot());
    }
    string traceSvgStr {
      get {
        return Potrace.TraceSvg(CapturedBmp, (int)TraceThreshold, TracePitch);
      }
    }
    Bitmap tracedBmp {
      get {
        return Potrace.TraceBmp(CapturedBmp, (int)TraceThreshold, TracePitch);
      }
    }
    public ReactiveProperty<BitmapFrame> TracedBmf { get; set; } = new ReactiveProperty<BitmapFrame>();
    void takeOneShot() {
#if DEBUG
      //      var bmppath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\trace_{DateTime.Now.ToString("yymmddhhMMss")}.bmp";
      //      CapturedBmp.Save(bmppath);
      var svgpath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\trace_{DateTime.Now.ToString("yymmddhhMMss")}.svg";
      System.IO.File.WriteAllText(new System.IO.FileInfo(svgpath).FullName, tracedSvgStr);
#endif
      var xl = (XL.Application)ExcelDnaUtil.Application;
      var awb = (XL.Workbook)xl.ActiveWorkbook;
      var aws = (XL.Worksheet)awb.ActiveSheet;
      writeExcelAutoShape(tracedFaces, aws);
      aws = null;
      awb = null;
      xl = null;
    }
    void writeExcelAutoShape(List<List<Vector3>> vector3s, XL.Worksheet ws) {
      foreach (var grp in vector3s) {
        var firstPoint = grp[0];
        XL.FreeformBuilder fb = ws.Shapes.BuildFreeform(MsoEditingType.msoEditingAuto, firstPoint.X, firstPoint.Y);
        for (var i = 1; i < grp.Count; i++) {
          //var t = $"{face.X},{face.Y},{face.Z}";
          fb.AddNodes(MsoSegmentType.msoSegmentLine, MsoEditingType.msoEditingAuto, grp[i].X, grp[i].Y);
        }
        var shape = fb.ConvertToShape();
        shape.Line.ForeColor.RGB = Color.Black.ToArgb();
        //shape.Line.Weight = 2f;
        shape.Fill.ForeColor.RGB = Color.Black.ToArgb();
        shape.Fill.Transparency = .5f;
      }
    }
  }
}

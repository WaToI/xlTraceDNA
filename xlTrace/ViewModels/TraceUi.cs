namespace xlTrace.ViewModels {
  using CsPotrace;
  using DevZest.Windows.Docking;
  using ExcelDna.Integration;
  using GraphicsProcessor;
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
      GetCamNames();
      initSubscribe();
      //StartCameraAsync();
    }
    public void Dispose() {
      CamStop();
    }
    public bool CamEn = false;
    public DockControl DockControl;
    public DockItem DockItem;
    public string ActiveDocName = "";
    public System.Windows.Point DocPoint;
    public System.Windows.Size DocSize;
    public void ActiveDocChanged(object sender, EventArgs e) {
      //DockControl = (DockControl)sender;
      //DockItem = DockControl.ActiveDocument;
      ActiveDocName = DockItem.Name;
      switch (ActiveDocName) {
        case "Camera":
          CamStartAsync();
          ScrStop();
          break;
        case "Screen":
          ScrStartAsync();
          CamStop();
          break;
        default:
          break;
      }
    }
    public ReactiveProperty<double> ScrFps { get; set; } = new ReactiveProperty<double>(30d);
    public ReactiveProperty<double> CamFps { get; set; } = new ReactiveProperty<double>(30d);
    public ReactiveProperty<double> FpsAct { get; set; } = new ReactiveProperty<double>();
    DateTime lastTime;
    System.Timers.Timer scrTimer;
    System.Timers.Timer camTimer;
    public ReactiveCollection<string> CamNames { get; set; } = new ReactiveCollection<string>();
    SynchronizationContext sync;
    public Task ScrCaptureTask { get; set; }
    public Task CamCaptureTask { get; set; }
    public bool ScrCaptureBusy { get; set; } = true;
    public bool CamCaptureBusy { get; set; } = true;
    public CancellationTokenSource ScrCaptureCancelTokenSrc { get; set; }
    public CancellationToken ScrCaptureCancelToken { get; set; }
    public CancellationTokenSource CamCaptureCancelTokenSrc { get; set; }
    public CancellationToken CamCaptureCancelToken { get; set; }
    UsbCamera cam;
    public Bitmap CapturedBmp { get; set; }
    public ReactiveProperty<BitmapFrame> CapturedBmf { get; set; } = new ReactiveProperty<BitmapFrame>();
    public void GetCamNames() {
      foreach (var dev in UsbCamera.FindDevices()) {
        CamNames.Add(dev);
      }
    }
    public void ScrStartAsync() {
      sync = SynchronizationContext.Current;
      ScrCaptureTask = ScrCaptureAsync();
      CamEn = false;
    }
    public void CamStartAsync() {
      sync = SynchronizationContext.Current;
      CamCaptureTask = CamCaptureAsync();
      CamEn = true;
    }
    public void ScrStop() {
      try {
        if (scrTimer != null) {
          scrTimer.Dispose();
        }
      } catch (Exception) { }
    }
    public void CamStop() {
      try {
        CamEn = false;
        if (camTimer != null) {
          camTimer.Dispose();
        }
        if (cam != null) {
          cam.Release();
          cam = null;
        }
      } catch (Exception) { }
    }
    public Task ScrCaptureAsync() {
      ScrCaptureCancelTokenSrc = new CancellationTokenSource();
      ScrCaptureCancelToken = ScrCaptureCancelTokenSrc.Token;
      return Task.Run(() => {
        scrTimer = new System.Timers.Timer(1000d / ScrFps.Value);
        scrTimer.Elapsed += scrCaptureTimerElapsed;
        ScrCaptureBusy = false;
        scrTimer.Start();
      }, ScrCaptureCancelToken);
    }
    public Task CamCaptureAsync() {
      CamCaptureCancelTokenSrc = new CancellationTokenSource();
      CamCaptureCancelToken = CamCaptureCancelTokenSrc.Token;
      var camNo = UsbCamera.FindDevices().Length - 1;
      return Task.Run(() => {
        //cam = new UsbCamera(camNo, 320, 180);
        cam = new UsbCamera(camNo, 640, 360);
        //cam = new UsbCamera(camNo, 1280, 720);
        cam.Start();
        camTimer = new System.Timers.Timer(1000d / CamFps.Value);
        camTimer.Elapsed += camCaptureTimerElapsed;
        CamCaptureBusy = false;
        camTimer.Start();
      }, CamCaptureCancelToken);
    }
    /// <summary>
    /// Scrキャプチャーメイン処理
    /// </summary>
    void scrCaptureTimerElapsed(object sender, System.Timers.ElapsedEventArgs e) {
      if (!ScrCaptureBusy) {
        try {
          ScrCaptureBusy = true;
          FpsAct.Value = 1000d / (DateTime.Now - lastTime).Milliseconds;
          lastTime = DateTime.Now;
          CapturedBmp = ScreenCapture.Instance.CaptureScreen((int)DocPoint.X, (int)DocPoint.Y, (int)DocSize.Width, (int)DocSize.Height);
          CapturedBmp.Save(@"C:\Users\tg30266\Desktop\test.png");
          Potrace.Trace(CapturedBmp, (int)TraceThreshold, TracePitch);
          tracedSvgStr = traceSvgStr;
          tracedFaces = Potrace.TraceFaces(traceSvgStr);
          CapturedBmf.Value = CapturedBmp.ToBitmapFrame();
          TracedBmf.Value = TracedBmp.ToBitmapFrame();
          ScrCaptureBusy = false;
        } catch (Exception) { ScrCaptureBusy = false; }
      }
    }
    /// <summary>
    /// Camキャプチャーメイン処理
    /// </summary>
    void camCaptureTimerElapsed(object sender, System.Timers.ElapsedEventArgs e) {
      if (!CamCaptureBusy) {
        try {
          CamCaptureBusy = true;
          FpsAct.Value = 1000d / (DateTime.Now - lastTime).Milliseconds;
          lastTime = DateTime.Now;
          CapturedBmp = CamNewFrame(cam);
          Potrace.Trace(CapturedBmp, (int)TraceThreshold, TracePitch);
          tracedSvgStr = traceSvgStr;
          tracedFaces = Potrace.TraceFaces(traceSvgStr);
          CapturedBmf.Value = CapturedBmp.ToBitmapFrame();
          TracedBmf.Value = TracedBmp.ToBitmapFrame();
          CamCaptureBusy = false;
        } catch (Exception) { }
      }
    }
    Func<UsbCamera, Bitmap> CamNewFrame = (c) => {
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
    void initSubscribe() {
      TakeOneShot.Subscribe((_) => takeOneShot());
    }
    string traceSvgStr {
      get {
        return Potrace.TraceSvg(CapturedBmp, (int)TraceThreshold, TracePitch);
      }
    }
    Bitmap TracedBmp {
      get {
        return Potrace.TraceBmp(CapturedBmp, (int)TraceThreshold, TracePitch);
      }
    }
    public ReactiveProperty<BitmapFrame> TracedBmf { get; set; } = new ReactiveProperty<BitmapFrame>();
    void takeOneShot() {
#if DEBUG
      //var bmppath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\trace_{DateTime.Now.ToString("yymmddhhMMss")}.bmp";
      //CapturedBmp.Save(bmppath);
      //var svgpath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\trace_{DateTime.Now.ToString("yymmddhhMMss")}.svg";
      //System.IO.File.WriteAllText(new System.IO.FileInfo(svgpath).FullName, tracedSvgStr);
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

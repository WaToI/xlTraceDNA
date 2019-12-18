namespace xlTrace {
  using ExcelDna.Integration;
  using ExcelDna.Integration.CustomUI;
  using System;
  using System.Runtime.InteropServices;
  using xlTrace.Views;
  using WF = System.Windows.Forms;
  [ComVisible(true)]
  public class App : ExcelRibbon, IExcelAddIn {
    static TraceUi traceUi = new TraceUi();
    private static Version AppVer = new Version(19, 9, 30, 2349);
    public App() { }
    public void AutoOpen() {
      //ShowCTP();
    }
    public void AutoClose() { }
    public static CustomTaskPane ctp;
    public static void ShowCTP() {
      if (ctp == null) {
        ctp = CustomTaskPaneFactory.CreateCustomTaskPane(typeof(CTPView), "xlTrace");
        //setPositionCtp();
        ctp.DockPositionStateChange += CTP_DockPositionStateChange;
        ctp.Visible = true;
      } else {
        ctp.Visible = true;
      }

    }
    static void setPositionCTP() {
      string arg = "";
      switch (arg) {
        case "msoCTPDockPositionBottom":
          ctp.DockPosition = MsoCTPDockPosition.msoCTPDockPositionBottom; break;
        case "msoCTPDockPositionFloating":
          ctp.DockPosition = MsoCTPDockPosition.msoCTPDockPositionFloating; break;
        case "msoCTPDockPositionLeft":
          ctp.DockPosition = MsoCTPDockPosition.msoCTPDockPositionLeft; break;
        case "msoCTPDockPositionRight":
          ctp.DockPosition = MsoCTPDockPosition.msoCTPDockPositionRight; break;
        case "msoCTPDockPositionTop":
          ctp.DockPosition = MsoCTPDockPosition.msoCTPDockPositionTop; break;
      }
      if (ctp.DockPosition == MsoCTPDockPosition.msoCTPDockPositionFloating
       || ctp.DockPosition == MsoCTPDockPosition.msoCTPDockPositionLeft
       || ctp.DockPosition == MsoCTPDockPosition.msoCTPDockPositionRight) {
        ctp.Width = 100;
      }
      if (ctp.DockPosition == MsoCTPDockPosition.msoCTPDockPositionFloating
       || ctp.DockPosition == MsoCTPDockPosition.msoCTPDockPositionTop
       || ctp.DockPosition == MsoCTPDockPosition.msoCTPDockPositionBottom) {
        ctp.Height = 100;
      }

    }
    static void CTP_DockPositionStateChange(CustomTaskPane CustomTaskPaneInst) { }
    //    public override string GetCustomUI(string RibbonID) {
    //      return $@"
    //<customUI xmlns='http://schemas.microsoft.com/office/2009/07/customui' loadImage='LoadImage'>
    //    <ribbon>
    //        <tabs>
    //            <tab id='tab1' label='xlTrace'>
    //                <group id='xlTrace' label='xlTrace'>
    //                    <button id='xlTraceOpen' image='img1' size='large' label='Open' onAction='Open'/>
    //                    <button id='xlTraceMyFunc1' image='img2' size='large' label='Func1' onAction='MyFunc1'/>
    //                    <button id='xlTraceMyFunc2' image='img3' size='large' label='Func2' onAction='MyFunc2'/>
    //                </group >
    //            </tab>
    //        </tabs>
    //    </ribbon>
    //    <contextMenus>
    //        <contextMenu idMso='ContextMenuCell'>
    //            <menu id='xlTraceMenu' image='img1' label='xlTrace'>
    //                <button id='xlTraceMyFunc3' image='img4' label='xlTraceMyFunc3' onAction='MyFunc3'/>
    //            </menu>
    //        </contextMenu>
    //    </contextMenus>
    //</customUI>";
    //    }
    [ExcelCommand(MenuName = "xlTrace", MenuText = "About")]
    public static void About() {
      WF.MessageBox.Show(
        $@"xlTrace is Convert a picture to a drawing.

  Powered by Excel-DNA : https://excel-dna.net/
", "About xlTrace");
    }
    [ExcelCommand(MenuName = "xlTrace", MenuText = "Capture")]
    public static void Camera() {
      try {
        traceUi?.Show();
      } catch (Exception) {
        traceUi = new TraceUi();
        traceUi.Show();
      }
    }
    [ComVisible(true)]
    public class CTPView : System.Windows.Forms.UserControl {
      //System.Windows.Forms.Integration.ElementHost elementHost;
      //elementHost.Child = wpfUserControl
    }
  }
}

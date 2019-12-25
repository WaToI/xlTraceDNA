namespace xlTrace.Views {
  using DevZest.Windows.Docking;
  using System;
  using System.Windows;
  using System.Windows.Input;

  public partial class TraceUi : Window {
    dynamic context;
    DockControl dockControl;
    DockItem dockItem;
    Point docPoint;
    Size docSize;
    public TraceUi() {
      InitializeComponent();
      MouseLeftButtonDown += (sender, e) => { dragWindow(sender, e); };
    }
    void closeButton_Click(object sender, RoutedEventArgs e) {
      try {
        Close();
      } catch (Exception) { }
    }
    void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      context = this.DataContext;
      context.Dispose();
    }
    void ThresholdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      try {
        context = this.DataContext;
        context.TraceThresholdChanged(sender, e);
      } catch (Exception) { }
    }
    void Label_MouseDown(object sender, MouseButtonEventArgs e) {
      dragWindowEn = true;
    }
    void Label_MouseUp(object sender, MouseButtonEventArgs e) {
      dragWindowEn = false;
      getDocPoint();
    }
    bool dragWindowEn = false;
    void dragWindow(object sender, MouseButtonEventArgs e) {
      if (dragWindowEn) {
        this.DragMove();
      }
    }
    private void DockControl_ActiveDocumentChanged(object sender, EventArgs e) {
      try {
        context = this.DataContext;
        getDock(sender);
        getDocPoint();
        context.ActiveDocChanged(sender, e);
      } catch (Exception) { }
    }
    void getDock(object sender) {
      try {
        context.DockControl = dockControl = (DockControl)sender;
        context.DockItem = dockItem = dockControl.ActiveDocument;
      } catch (Exception) { }
    }
    void getDocPoint() {
      try {
        context.DocPoint = docPoint = dockItem.PointToScreen(new System.Windows.Point(0, 0));
        context.DocSize = docSize = dockItem.RenderSize;
      } catch (Exception) { }
    }
  }
}

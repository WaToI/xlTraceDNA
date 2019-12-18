namespace xlTrace.Views {
  using System;
  using System.Windows;
  using System.Windows.Input;

  /// <summary>
  /// UsbCameraUI.xaml の相互作用ロジック
  /// </summary>
  public partial class TraceUi : Window {
    public TraceUi() {
      InitializeComponent();
      MouseLeftButtonDown += (sender, e) => { dragWindow(sender, e); };
    }
    void closeButton_Click(object sender, RoutedEventArgs e) {
      Close();
    }
    void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      dynamic context = this.DataContext;
      context.Dispose();
    }
    void ThresholdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      try {
        dynamic context = this.DataContext;
        context.TraceThresholdChanged(sender, e);
      } catch (Exception) { }
    }
    void Label_MouseDown(object sender, MouseButtonEventArgs e) {
      dragWindowEn = true;
    }
    void Label_MouseUp(object sender, MouseButtonEventArgs e) {
      dragWindowEn = false;
    }
    bool dragWindowEn = false;
    void dragWindow(object sender, MouseButtonEventArgs e) {
      if (dragWindowEn) {
        this.DragMove();
      }
    }
  }
}

namespace xlTrace {
  using System;
  using System.Drawing;
  using System.IO;
  using System.Text;
  using System.Text.RegularExpressions;
  using System.Threading.Tasks;
  using System.Windows.Media.Imaging;
  public static class MyExtentiotns {
    public static async Task WriteTextAsync(this FileInfo fi, string text, Encoding _enc = null, string _newLineStr = null) {
      if (!fi.Directory.Exists) {
        fi.Directory.Create();
      }
      var newLineStr = (_newLineStr ?? Environment.NewLine);
      text = Regex.Replace(text, "\r\n|\r|\n", newLineStr);
      var enc = (_enc ?? Encoding.GetEncoding(932));
      var buf = enc.GetBytes(text);
      using (var fs = new FileStream(fi.FullName, FileMode.Append, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true)) {
        await fs.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
      };
    }
  }
  public static class WpfExtension {
    public static BitmapFrame ToBitmapFrame(this Bitmap bmp) {
      using (var ms = new System.IO.MemoryStream()) {
        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        return BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
      }
    }
  }
  public static class ValidatorExtension {
    public static Regex RgxKeyPeriod = new Regex(@"^\d\d\d\d$", RegexOptions.Compiled);
    public static bool IsKeyPeriod(this string v) { return RgxKeyPeriod.IsMatch(v); }
    public static Regex RgxKeyNumber = new Regex(@"^([1-9]\d*|0)(\.\d+)?$");
    public static bool IsKeyNumber(this string v) { return RgxKeyNumber.IsMatch(v); }
    public static Regex RgxInt = new Regex(@"^$|^\d+$");
    public static bool IsInt(this string v) { return RgxKeyNumber.IsMatch(v); }
    public static Regex RgxYMD = new Regex(@"^$|^[0-9]{4}/(0[1-9]|1[0-2])/(0[1-9]|[1-2][0-9]|3[0-1])$");
    public static bool IsYMD(this string v) { return RgxYMD.IsMatch(v); }
    public static Regex RgxIsYMDorBar = new Regex(@"^$|^[0-9]{4}/(0[1-9]|1[0-2])/(0[1-9]|[1-2][0-9]|3[0-1])$|^-$");
    public static bool IsYMDorBar(this string v) { return RgxIsYMDorBar.IsMatch(v); }
    public static Regex RgxYMDorHuyo = new Regex(@"^$|^[0-9]{4}/(0[1-9]|1[0-2])/(0[1-9]|[1-2][0-9]|3[0-1])$|^不要$");
    public static bool IsYMDorHuyo(this string v) { return RgxYMDorHuyo.IsMatch(v); }
    public static Regex RgxHankakuKana = new Regex(@"");
    public static bool IsHankakuKana(this string v) { return RgxHankakuKana.IsMatch(v); }
    public static Regex RgxHankakuEisu = new Regex(@"^$|^[0-9A-Za-z]+$");
    public static bool IsHankakuEisu(this string v) { return RgxHankakuEisu.IsMatch(v); }
    public static Regex RgxHankakuEisuKigo = new Regex(@"^[a-zA-Z0-9!-/:-@\[-`{-~]+$");
    public static bool IsHankakuEisuKigo(this string v) { return RgxHankakuEisuKigo.IsMatch(v); }
  }
}

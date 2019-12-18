namespace xlTrace.Models {
  using ExcelDna.Integration;
  using WF = System.Windows.Forms;
  using XL = Microsoft.Office.Interop.Excel;
  public class ExcelMacros {
    [ExcelCommand(MenuName = "xlTrace", MenuText = "SelectionFreeformToOpenSCAD")]
    public static void SelectionShapeToOpenSCAD() {
      XL.Application xl = (XL.Application)ExcelDnaUtil.Application;
      dynamic sel = xl.Selection;
      XL.ShapeRange sr = (XL.ShapeRange)sel.ShapeRange;
      //WF.MessageBox.Show(sr.Name);
      var vecs = new string[sr.Nodes.Count];
      var i = 0;
      foreach (XL.ShapeNode n in sr.Nodes) {
        float[,] ps = n.Points;
        //Excelは左上に左上に原点,OpenSCADは左下に原点のためyを反転する意味で-1をかける
        vecs[i++] = $"[{ps[1, 1]},-{ps[1, 2]}]";
      }
      WF.Clipboard.SetText($@"points = [
  {string.Join(",\n\t", vecs)}
];
polygon(points=points);");
      sr = null;
      xl = null;
    }
  }
}

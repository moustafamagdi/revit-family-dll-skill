using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Forms = System.Windows.Forms;

namespace RevitFamilyGenerator
{
    [Transaction(TransactionMode.Manual)]
    public sealed class BuildDemoFamily : IExternalCommand
    {
        private const double WidthMm = 200;
        private const double DepthMm = 150;
        private static readonly int[] HeightsMm = { 300, 600, 900 };
        private static double Feet(double mm) { return mm / 304.8; }
        private static string TypeName(int mm) { return "H" + mm.ToString("0000", CultureInfo.InvariantCulture); }

        public Result Execute(ExternalCommandData data, ref string message, ElementSet elements)
        {
            try
            {
                RunLog.Start(data.Application);
                if (data.Application.Application.VersionNumber != "2024")
                    throw new InvalidOperationException("This demonstration DLL requires Revit 2024.");
                string template;
                using (var picker = new Forms.OpenFileDialog
                {
                    Title = "Select an empty Revit 2024 Generic Model family template",
                    Filter = "Revit family template (*.rft)|*.rft",
                    InitialDirectory = data.Application.Application.FamilyTemplatePath,
                    CheckFileExists = true
                })
                {
                    if (picker.ShowDialog() != Forms.DialogResult.OK)
                    { RunLog.Write("CANCELLED template selection"); return Result.Cancelled; }
                    template = picker.FileName;
                }
                string root;
                using (var picker = new Forms.FolderBrowserDialog
                { Description = "Choose output parent folder for the synthetic demo family" })
                {
                    if (picker.ShowDialog() != Forms.DialogResult.OK)
                    { RunLog.Write("CANCELLED output selection"); return Result.Cancelled; }
                    root = picker.SelectedPath;
                }
                string folder = Path.Combine(root, "FamilyGenerator_Demo_" +
                    DateTime.Now.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture) + "_" +
                    Guid.NewGuid().ToString("N").Substring(0, 8));
                Directory.CreateDirectory(folder);
                string notes;
                using (var stream = typeof(BuildDemoFamily).Assembly.GetManifestResourceStream("RevitFamilyGenerator.SourceNotes"))
                {
                    if (stream == null) throw new InvalidOperationException("Embedded source notes missing.");
                    using (var reader = new StreamReader(stream)) notes = reader.ReadToEnd();
                }
                File.WriteAllText(Path.Combine(folder, "SOURCE_NOTES.md"), notes, new UTF8Encoding(false));
                string path = Path.Combine(folder, "Synthetic_Parametric_Box.rfa");
                RunLog.Write("TEMPLATE=" + template + " OUTPUT=" + path);
                Build(data.Application, template, path);
                RunLog.Write("ALL REQUIRED DEMO CHECKS PASSED output=" + path);
                File.WriteAllText(Path.Combine(folder, "VALIDATION_PASSED.txt"),
                    "Synthetic box only. All three type defaults and height flex checks passed; " +
                    "saved/reopened geometry and associations checked. No MEP/network/visual tests.\n" +
                    "Version=" + typeof(BuildDemoFamily).Assembly.GetName().Version + " MVID=" +
                    typeof(BuildDemoFamily).Module.ModuleVersionId + "\nLog=" + RunLog.PathName,
                    new UTF8Encoding(false));
                TaskDialog.Show("Revit Family DLL Demo", "Saved and validated:\n" + path +
                    "\n\nType parameter Demo_Height drives height. Width/depth are fixed." +
                    "\nNo connectors or manufacturer data.\n\nDesktop log:\n" + RunLog.PathName);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                string logFailure = RunLog.TryWrite("RESULT FAILED\n" + ex);
                message = ex.Message;
                TaskDialog.Show("Revit Family DLL Demo - failed", ex.Message + "\n\nLog:\n" +
                    (RunLog.PathName ?? "Not initialized") + (logFailure == null ? "" : "\n\n" + logFailure));
                return Result.Failed;
            }
        }

        private static void Build(UIApplication ui, string template, string path)
        {
            Document doc = ui.Application.NewFamilyDocument(template);
            if (doc == null) throw new InvalidOperationException("Could not open selected template.");
            WithOwnedDocument(doc, () =>
            {
                Transactions.Run(doc, "Create synthetic parametric box", () =>
                {
                    if (doc.OwnerFamily.FamilyCategory == null || doc.OwnerFamily.FamilyCategory.Id !=
                        doc.Settings.Categories.get_Item(BuiltInCategory.OST_GenericModel).Id)
                        throw new InvalidOperationException("Select a Generic Model family template.");
                    if (new FilteredElementCollector(doc).OfClass(typeof(GenericForm)).Any())
                        throw new InvalidOperationException("Select a template without existing forms.");
                    var manager = doc.FamilyManager;
                    if (manager.get_Parameter("Demo_Height") != null)
                        throw new InvalidOperationException("Template already contains Demo_Height; use an empty template.");
                    FamilyParameter height = manager.AddParameter("Demo_Height", GroupTypeId.Geometry, SpecTypeId.Length, false);
                    var profile = new CurveArray();
                    XYZ[] points = {
                        new XYZ(-Feet(WidthMm)/2, -Feet(DepthMm)/2, 0),
                        new XYZ( Feet(WidthMm)/2, -Feet(DepthMm)/2, 0),
                        new XYZ( Feet(WidthMm)/2,  Feet(DepthMm)/2, 0),
                        new XYZ(-Feet(WidthMm)/2,  Feet(DepthMm)/2, 0)
                    };
                    for (int i = 0; i < points.Length; i++)
                        profile.Append(Line.CreateBound(points[i], points[(i + 1) % points.Length]));
                    var profiles = new CurveArrArray(); profiles.Append(profile);
                    var sketch = SketchPlane.Create(doc, Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero));
                    // Create an explicit current type before setting family parameter values.
                    manager.NewType(TypeName(HeightsMm[0]));
                    manager.Set(height, Feet(HeightsMm[0]));
                    Extrusion solid = doc.FamilyCreate.NewExtrusion(true, profiles, sketch, Feet(HeightsMm[0]));
                    Parameter end = solid.get_Parameter(BuiltInParameter.EXTRUSION_END_PARAM);
                    if (end == null || !manager.CanElementParameterBeAssociated(end))
                        throw new InvalidOperationException("Extrusion end cannot be associated.");
                    manager.AssociateElementParameterToFamilyParameter(end, height);
                    foreach (int h in HeightsMm.Skip(1))
                    { manager.NewType(TypeName(h)); manager.Set(height, Feet(h)); }
                });
                ValidateTypes(doc, true);
                Restore(doc);
                RunLog.Write("SAVE=" + path);
                doc.SaveAs(path, new SaveAsOptions { OverwriteExistingFile = false });
            });
            RunLog.Write("REOPEN=" + path);
            Document reopened = ui.Application.OpenDocumentFile(path);
            WithOwnedDocument(reopened, () => ValidateTypes(reopened, false));
        }

        private static void WithOwnedDocument(Document doc, Action action)
        {
            Exception original = null;
            try { action(); }
            catch (Exception ex) { original = ex; throw; }
            finally
            {
                try
                {
                    if (!doc.Close(false)) throw new InvalidOperationException("Temporary family document did not close.");
                    RunLog.Write("OWNED DOCUMENT CLOSED");
                }
                catch (Exception cleanup)
                {
                    string logFailure = RunLog.TryWrite("DOCUMENT CLEANUP FAILED\n" + cleanup);
                    if (original == null) throw;
                    original.Data["DocumentCleanupFailure"] = cleanup.ToString();
                    if (logFailure != null) original.Data["CleanupLogFailure"] = logFailure;
                }
            }
        }

        private static void ValidateTypes(Document doc, bool flex)
        {
            var manager = doc.FamilyManager;
            var types = manager.Types.Cast<FamilyType>().ToList();
            if (types.Count != HeightsMm.Length || !types.Select(x => x.Name).OrderBy(x => x)
                .SequenceEqual(HeightsMm.Select(TypeName).OrderBy(x => x)))
                throw new InvalidOperationException("Catalogue type inventory mismatch (use an empty template).");
            foreach (int h in HeightsMm)
            {
                Transactions.Run(doc, "Select " + TypeName(h), () =>
                    manager.CurrentType = types.Single(t => t.Name == TypeName(h)));
                ValidateGeometry(doc, h);
                if (!flex) continue;
                Transactions.Run(doc, "Flex " + TypeName(h), () =>
                    manager.Set(manager.get_Parameter("Demo_Height"), Feet(h + 25)));
                ValidateGeometry(doc, h + 25);
                Transactions.Run(doc, "Restore " + TypeName(h), () =>
                    manager.Set(manager.get_Parameter("Demo_Height"), Feet(h)));
                ValidateGeometry(doc, h);
            }
        }

        private static void Restore(Document doc)
        {
            Transactions.Run(doc, "Restore first catalogue type", () =>
                doc.FamilyManager.CurrentType = doc.FamilyManager.Types.Cast<FamilyType>()
                    .Single(t => t.Name == TypeName(HeightsMm[0])));
            ValidateGeometry(doc, HeightsMm[0]);
        }

        private static void ValidateGeometry(Document doc, double heightMm)
        {
            var forms = new FilteredElementCollector(doc).OfClass(typeof(GenericForm)).Cast<GenericForm>().ToList();
            if (forms.Count != 1 || !(forms[0] is Extrusion))
                throw new InvalidOperationException("Expected exactly one native extrusion.");
            GenericForm form = forms[0];
            FamilyParameter driver = doc.FamilyManager.GetAssociatedFamilyParameter(
                form.get_Parameter(BuiltInParameter.EXTRUSION_END_PARAM));
            if (driver == null || driver.Definition.Name != "Demo_Height")
                throw new InvalidOperationException("Height association missing.");
            var box = form.get_BoundingBox(null);
            if (box == null) throw new InvalidOperationException("Native geometry has no bounds.");
            Near((box.Max.X - box.Min.X) * 304.8, WidthMm, "actual width mm", 0.05);
            Near((box.Max.Y - box.Min.Y) * 304.8, DepthMm, "actual depth mm", 0.05);
            Near(box.Min.Z * 304.8, 0, "actual base mm", 0.05);
            Near(box.Max.Z * 304.8, heightMm, "actual top mm", 0.05);
            var solids = form.get_Geometry(new Options { IncludeNonVisibleObjects = true })
                .OfType<Solid>().Where(s => s.Volume > 1e-12).ToList();
            if (solids.Count != 1) throw new InvalidOperationException("Expected one positive solid.");
            Near(solids[0].Volume, Feet(WidthMm) * Feet(DepthMm) * Feet(heightMm), "actual volume ft3", 1e-7);
            RunLog.Write("VALIDATED type=" + doc.FamilyManager.CurrentType.Name +
                " measuredHeightMm=" + heightMm.ToString(CultureInfo.InvariantCulture));
        }

        private static void Near(double actual, double expected, string label, double tolerance)
        {
            if (double.IsNaN(actual) || double.IsInfinity(actual) || Math.Abs(actual - expected) > tolerance)
                throw new InvalidOperationException(label + ": actual=" + actual.ToString(CultureInfo.InvariantCulture) +
                    " expected=" + expected.ToString(CultureInfo.InvariantCulture));
        }
    }
}

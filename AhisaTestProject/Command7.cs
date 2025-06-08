namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command7 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var allLinePatterns = new FilteredElementCollector(doc)
                .OfClass(typeof(LinePatternElement))
                .Cast<LinePatternElement>()
                .Where(p => !p.Name.Equals("Solid", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var usedLinePatternIds = new HashSet<ElementId>();

            Category linesCat = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines);
            foreach (Category subCat in linesCat.SubCategories)
            {
                ElementId linePatternId = subCat.GetLinePatternId(GraphicsStyleType.Projection);
                if (linePatternId != ElementId.InvalidElementId)
                    usedLinePatternIds.Add(linePatternId);
            }

            var unusedPatterns = allLinePatterns
                .Where(p => !usedLinePatternIds.Contains(p.Id))
                .ToList();

            int deletedCount = 0;
            int total = unusedPatterns.Count;

            if (total == 0)
            {
                TaskDialog.Show("Line Patterns Cleanup", "No unused line patterns found.");
                return Result.Succeeded;
            }

            ProgressBarHelper progressBar = new ProgressBarHelper();
            progressBar.ShowProgress(total);

            try
            {
                using (Transaction tx = new Transaction(doc, "Delete Unused Line Patterns"))
                {
                    tx.Start();

                    for (int i = 0; i < unusedPatterns.Count; i++)
                    {
                        if (progressBar.IsCancelled())
                        {
                            progressBar.UpdateProgress(i, "Operation cancelled.");
                            tx.RollBack();
                            progressBar.CloseProgress();
                            return Result.Cancelled;
                        }

                        try
                        {
                            doc.Delete(unusedPatterns[i].Id);
                            deletedCount++;
                        }
                        catch { /* skip if in use elsewhere */ }

                        progressBar.UpdateProgress(i + 1, $"Deleting {i + 1} of {total}");
                    }

                    tx.Commit();
                }

                progressBar.UpdateProgress(total, "Cleanup complete.");
                System.Threading.Thread.Sleep(1000);
                progressBar.CloseProgress();

                TaskDialog.Show("Line Patterns Cleanup", $"{deletedCount} unused line pattern(s) deleted.");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                progressBar.CloseProgress();
                TaskDialog.Show("Error", ex.Message);
                return Result.Failed;
            }
        }

        internal static PushButtonData GetButtonData()
        {
            return new Common.ButtonDataClass(
                "btnCommand7",
                "Delete Unused \nLine Patterns",
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.DeleteUnusedLTypes32x32,
                Properties.Resources.DeleteUnusedLTypes32x32,
                "Deletes all unused line patterns from the project.").Data;
        }
    }



}

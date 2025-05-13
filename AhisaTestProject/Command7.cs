namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command7 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Step 1: Get all LinePatternElements
            var allLinePatterns = new FilteredElementCollector(doc)
                .OfClass(typeof(LinePatternElement))
                .Cast<LinePatternElement>()
                .Where(p => !p.Name.Equals("Solid", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Step 2: Get all LineStyles in use
            var usedLinePatternIds = new HashSet<ElementId>();

            var categories = doc.Settings.Categories;
            Category linesCat = categories.get_Item(BuiltInCategory.OST_Lines);
            foreach (Category subCat in linesCat.SubCategories)
            {
                ElementId linePatternId = subCat.GetLinePatternId(GraphicsStyleType.Projection);
                if (linePatternId != ElementId.InvalidElementId)
                    usedLinePatternIds.Add(linePatternId);
            }

            // Step 3: Filter unused line patterns
            var unusedPatterns = allLinePatterns
                .Where(p => !usedLinePatternIds.Contains(p.Id))
                .ToList();

            int deletedCount = 0;
            using (Transaction tx = new Transaction(doc, "Delete Unused Line Patterns"))
            {
                tx.Start();
                foreach (var pattern in unusedPatterns)
                {
                    try
                    {
                        doc.Delete(pattern.Id);
                        deletedCount++;
                    }
                    catch { /* skip if in use elsewhere */ }
                }
                tx.Commit();
            }

            TaskDialog.Show("Line Patterns Cleanup", $"{deletedCount} unused line pattern(s) deleted.");
            return Result.Succeeded;
        }
        

        internal static PushButtonData GetButtonData()
        {
            string buttonInternalName = "btnCommand7";
            string buttonTitle = "Delete Unused \nLine Patterns";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.DeleteUnusedVFilters16x16,
                Properties.Resources.DeleteUnusedVFilters16x16,
                "Deletes all unused line patterns from the project.");

            return myButtonData.Data;
        }
    }



}

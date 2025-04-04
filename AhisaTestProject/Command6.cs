namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command6 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Collect all view filters in the project
            List<ParameterFilterElement> allFilters = new FilteredElementCollector(doc)
                .OfClass(typeof(ParameterFilterElement))
                .Cast<ParameterFilterElement>()
                .ToList();

            // Collect all views (excluding templates)
            List<View> views = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => !v.IsTemplate && v.CanBePrinted) // skip templates & non-view views
                .ToList();

            // Identify unused filters
            List<ParameterFilterElement> unusedFilters = new List<ParameterFilterElement>();
            foreach (ParameterFilterElement filter in allFilters)
            {
                bool isUsed = views.Any(view => view.GetFilters().Contains(filter.Id));
                if (!isUsed)
                    unusedFilters.Add(filter);
            }

            if (unusedFilters.Count == 0)
            {
                TaskDialog.Show("Filter Cleanup", "No unused view filters found.");
                return Result.Succeeded;
            }

            // Confirmation dialog
            TaskDialog dialog = new TaskDialog("Delete Unused View Filters");
            dialog.MainInstruction = "Unused View Filters Detected";
            dialog.MainContent = $"Found {unusedFilters.Count} unused filter(s). Do you want to delete them?";
            dialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
            dialog.DefaultButton = TaskDialogResult.No;

            if (dialog.Show() == TaskDialogResult.Yes)
            {
                using (Transaction trans = new Transaction(doc, "Delete Unused View Filters"))
                {
                    trans.Start();
                    foreach (ParameterFilterElement filter in unusedFilters)
                    {
                        doc.Delete(filter.Id);
                    }
                    trans.Commit();
                }

                TaskDialog.Show("Filter Cleanup", $"{unusedFilters.Count} unused view filter(s) deleted.");
            }

            return Result.Succeeded;
        }

        internal static PushButtonData GetButtonData()
        {
            string buttonInternalName = "btnCommand6";
            string buttonTitle = "Delete Unused \nView Filters";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.DeleteUnusedSBoxes32,
                Properties.Resources.DeleteDWG16,
                "Deletes all unused view filters from the project.");

            return myButtonData.Data;
        }
    }



}
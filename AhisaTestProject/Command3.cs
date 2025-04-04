namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command3 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Collect all scope boxes in the project
            List<Element> scopeBoxes = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_VolumeOfInterest)
                .WhereElementIsNotElementType()
                .ToList();

            // Collect all views and view templates
            List<View> views = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => !v.IsTemplate)
                .ToList();

            List<View> viewTemplates = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => v.IsTemplate)
                .ToList();

            // Identify unused scope boxes
            List<Element> unusedScopeBoxes = new List<Element>();
            foreach (Element scopeBox in scopeBoxes)
            {
                ElementId scopeBoxId = scopeBox.Id;
                bool isUsed = views.Any(v => v.GetParamValue(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP) == scopeBoxId) ||
                              viewTemplates.Any(vt => vt.GetParamValue(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP) == scopeBoxId);

                if (!isUsed)
                {
                    unusedScopeBoxes.Add(scopeBox);
                }
            }

            // If no unused scope boxes found, notify the user
            if (unusedScopeBoxes.Count == 0)
            {
                TaskDialog.Show("Scope Box Cleanup", "No unused scope boxes found in the project.");
                return Result.Succeeded;
            }

            // Prepare list of unused scope boxes for display
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendLine($"Found {unusedScopeBoxes.Count} unused scope boxes:\n");
            foreach (Element scopeBox in unusedScopeBoxes)
            {
                messageBuilder.AppendLine($"- {scopeBox.Name}");
            }
            messageBuilder.AppendLine("\nDo you want to delete these unused scope boxes?");

            // Show confirmation dialog
            TaskDialog dialog = new TaskDialog("Delete Unused Scope Boxes");
            dialog.MainInstruction = "Unused Scope Boxes Detected";
            dialog.MainContent = messageBuilder.ToString();
            dialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
            dialog.DefaultButton = TaskDialogResult.No;

            TaskDialogResult result = dialog.Show();

            if (result == TaskDialogResult.Yes)
            {
                int deletedScopeBoxCount = 0;

                using (Transaction trans = new Transaction(doc, "Delete Unused \nScope Boxes"))
                {
                    trans.Start();
                    foreach (Element scopeBox in unusedScopeBoxes)
                    {
                        doc.Delete(scopeBox.Id);
                        deletedScopeBoxCount++;
                    }
                    trans.Commit();
                }

                TaskDialog.Show("Scope Box Cleanup", $"{deletedScopeBoxCount} unused scope boxes were deleted.");
            }
            else
            {
                TaskDialog.Show("Scope Box Cleanup", "No scope boxes were deleted.");
            }

            return Result.Succeeded;
        }

        internal static PushButtonData GetButtonData()
        {
            string buttonInternalName = "btnCommand3";
            string buttonTitle = "Delete Unused \nScope Boxes";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.DeleteUnusedSBoxes32,
                Properties.Resources.DeleteUnusedSBoxes16,
                "Find and delete all unused scope boxes in the project.");

            return myButtonData.Data;
        }
    }

    /// <summary>
    /// Extension method to safely retrieve parameter values from a View.
    /// </summary>
    public static class ViewExtensions
    {
        public static ElementId GetParamValue(this View view, BuiltInParameter parameter)
        {
            Parameter param = view.get_Parameter(parameter);
            return param != null ? param.AsElementId() : ElementId.InvalidElementId;
        }
    }
}
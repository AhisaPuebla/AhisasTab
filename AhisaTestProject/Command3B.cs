namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command3B : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Collect all scope boxes in the project
            List<ElementId> scopeBoxIds = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_VolumeOfInterest)
                .WhereElementIsNotElementType()
                .Select(e => e.Id)
                .ToList();

            // If no scope boxes found, notify the user
            if (scopeBoxIds.Count == 0)
            {
                TaskDialog.Show("Scope Box Cleanup", "No scope boxes found in the project.");
                return Result.Succeeded;
            }

            // Confirmation dialog
            TaskDialog dialog = new TaskDialog("Delete All Scope Boxes")
            {
                MainInstruction = "Delete All Scope Boxes?",
                MainContent = $"Found {scopeBoxIds.Count} scope boxes.\nDo you want to delete them all?",
                CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No,
                DefaultButton = TaskDialogResult.No
            };

            if (dialog.Show() == TaskDialogResult.Yes)
            {
                using (Transaction trans = new Transaction(doc, "Delete All Scope Boxes"))
                {
                    trans.Start();
                    doc.Delete(scopeBoxIds);
                    trans.Commit();
                }

                TaskDialog.Show("Scope Box Cleanup", $"{scopeBoxIds.Count} scope boxes were deleted.");
            }

            return Result.Succeeded;
        }

        internal static PushButtonData GetButtonData()
        {
            string buttonInternalName = "btnCommand3B";
            string buttonTitle = "Delete All \nScope Boxes";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.DeleteAllSBoxes32x32,
                Properties.Resources.DeleteAllSBoxes16x16,
                "Find and delete all unused scope boxes in the project.");

            return myButtonData.Data;
        }

    }

    /// <summary>
    /// Extension method to safely retrieve parameter values from a View.
    /// </summary>

}
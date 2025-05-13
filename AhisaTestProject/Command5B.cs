namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command5B : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Collect all legend views
            List<View> allLegends = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => v.ViewType == ViewType.Legend)
                .ToList();

            if (allLegends.Count == 0)
            {
                TaskDialog.Show("Legend Cleanup", "No legends found in the project.");
                return Result.Succeeded;
            }

            // Confirmation dialog
            TaskDialog dialog = new TaskDialog("Delete All Legends");
            dialog.MainInstruction = "Delete All Legend Views?";
            dialog.MainContent = $"This will delete {allLegends.Count} legend view(s) from the project. Proceed?";
            dialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
            dialog.DefaultButton = TaskDialogResult.No;

            if (dialog.Show() == TaskDialogResult.Yes)
            {
                using (Transaction trans = new Transaction(doc, "Delete All Legends"))
                {
                    trans.Start();
                    foreach (View legend in allLegends)
                    {
                        doc.Delete(legend.Id);
                    }
                    trans.Commit();
                }

                TaskDialog.Show("Legend Cleanup", $"{allLegends.Count} legend view(s) deleted.");
            }

            return Result.Succeeded;
        }

        internal static PushButtonData GetButtonData()
        {
            string buttonInternalName = "btnCommand5B";
            string buttonTitle = "Delete All Legends";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.DeleteAllLegends32x32,
                Properties.Resources.DeleteAllLegends32x32,
                "Deletes all legend views in the project.");

            return myButtonData.Data;
        }
    }

   
    
}
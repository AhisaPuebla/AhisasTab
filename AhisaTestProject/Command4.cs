namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command4 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Debug to confirm command execution
            TaskDialog.Show("Debug", "Command4 Executed.");

            // Collect all ImportInstance elements in the project
            List<ImportInstance> allImports = new FilteredElementCollector(doc)
                .OfClass(typeof(ImportInstance))
                .WhereElementIsNotElementType()
                .Cast<ImportInstance>()
                .ToList();

            // Display the count of imported instances
            TaskDialog.Show("Debug", $"Found {allImports.Count} ImportInstance elements.");

            return Result.Succeeded;
        }

        internal static PushButtonData GetButtonData()
        {
            string buttonInternalName = "btnCommand4";
            string buttonTitle = "Imported \nDWGs";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.DeleteDWG32,
                Properties.Resources.DeleteDWG16,
                "Finds all imported (not linked) DWG files and optionally deletes them.");

            return myButtonData.Data;
        }
    }
}
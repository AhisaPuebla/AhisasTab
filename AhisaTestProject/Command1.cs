namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            int deletedDetailCount = 0;
            int deletedModelCount = 0;

            using (Transaction trans = new Transaction(doc, "Delete All Non-Placed Groups"))
            {
                trans.Start();

                // Delete all non-placed detail groups
                List<Element> unplacedDetailGroups = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_IOSDetailGroups)
                    .WhereElementIsElementType()
                    .Where(e => (e as GroupType)?.Groups.IsEmpty == true)
                    .ToList();

                foreach (Element groupType in unplacedDetailGroups)
                {
                    doc.Delete(groupType.Id);
                    deletedDetailCount++;
                }

                // Delete all non-placed model groups
                List<Element> unplacedModelGroups = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_IOSModelGroups)
                    .WhereElementIsElementType()
                    .Where(e => (e as GroupType)?.Groups.IsEmpty == true)
                    .ToList();

                foreach (Element groupType in unplacedModelGroups)
                {
                    doc.Delete(groupType.Id);
                    deletedModelCount++;
                }

                trans.Commit();
            }

            // Show TaskDialog with summary
            TaskDialog.Show("Delete Unplaced Groups",
                $"{deletedDetailCount} unplaced detail group types deleted.\n" +
                $"{deletedModelCount} unplaced model group types deleted.");



            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Delete Unplaced Groups";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                //null,
                Properties.Resources.DeleteUnplaced32x32,
                Properties.Resources.DeleteUnplaced16x16,
                "Deletes all non-placed detail and model groups in the project"); 

            return myButtonData.Data;
        }
    }

}

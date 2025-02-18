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
            
            using (Transaction trans = new Transaction(doc, "Explode and Delete Detail Groups"))
            {
                trans.Start();

                // Get all detail groups
                List<Group> placedGroups = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_IOSDetailGroups)
                    .WhereElementIsNotElementType()
                    .Cast<Group>()
                    .ToList();

                int explodedCount = 0;
                foreach (Group group in placedGroups)
                {
                    group.UngroupMembers();
                    explodedCount++;
                }

                // Delete non-placed detail groups
                List<Element> unplacedGroupTypes = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_IOSDetailGroups)
                    .WhereElementIsElementType()
                    .Where(e => (e as GroupType)?.Groups.IsEmpty == true)
                    .ToList();

                foreach (Element groupType in unplacedGroupTypes)
                {
                    doc.Delete(groupType.Id);
                }

                trans.Commit();

                TaskDialog.Show("Detail Groups",
                    $"{explodedCount} placed detail groups exploded.\n" +
                    $"{unplacedGroupTypes.Count} unplaced detail group types deleted.");
            }


            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Explode and \nDelete Detail Groups";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.ExplodeDGroups32,
                Properties.Resources.ExplodeDGroups16,
                "Explode and then delete detail groups in the project");

            return myButtonData.Data;
        }
    }

}

namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command2B : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            int explodedDetailCount = 0;
            int explodedModelCount = 0;
            int deletedDetailCount = 0;
            int deletedModelCount = 0;

            using (Transaction trans = new Transaction(doc, "Ungroup and Delete All Groups"))
            {
                trans.Start();

                // Step 1: Ungroup all placed detail groups
                List<Group> detailGroups = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_IOSDetailGroups)
                    .WhereElementIsNotElementType()
                    .Cast<Group>()
                    .ToList();

                foreach (Group group in detailGroups)
                {
                    group.UngroupMembers();
                    explodedDetailCount++;
                }

                // Step 2: Ungroup all placed model groups
                List<Group> modelGroups = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_IOSModelGroups)
                    .WhereElementIsNotElementType()
                    .Cast<Group>()
                    .ToList();

                foreach (Group group in modelGroups)
                {
                    group.UngroupMembers();
                    explodedModelCount++;
                }

                trans.Commit();
            }

            using (Transaction trans = new Transaction(doc, "Delete All Non-Placed Groups"))
            {
                trans.Start();

                // Step 3: Delete all remaining detail group types
                List<Element> detailGroupTypes = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_IOSDetailGroups)
                    .WhereElementIsElementType()
                    .ToList();

                foreach (Element groupType in detailGroupTypes)
                {
                    doc.Delete(groupType.Id);
                    deletedDetailCount++;
                }

                // Step 4: Delete all remaining model group types
                List<Element> modelGroupTypes = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_IOSModelGroups)
                    .WhereElementIsElementType()
                    .ToList();

                foreach (Element groupType in modelGroupTypes)
                {
                    doc.Delete(groupType.Id);
                    deletedModelCount++;
                }

                trans.Commit();
            }

            // Show TaskDialog with summary of actions
            TaskDialog.Show("Group Cleanup",
                $"{explodedDetailCount} placed detail groups ungrouped.\n" +
                $"{explodedModelCount} placed model groups ungrouped.\n\n" +
                $"{deletedDetailCount} unplaced detail group types deleted.\n" +
                $"{deletedModelCount} unplaced model group types deleted.");

            return Result.Succeeded;
        }


        internal static PushButtonData GetButtonData()
        {

            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand2B";
            string buttonTitle = "Ungroup and \nDelete All Groups";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                //null,
                Properties.Resources.UngroupAndDelete32x32,
                Properties.Resources.UngroupAndDelete16x16,
                "Ungroup and then delete all detail and model groups in the project.");

            return myButtonData.Data;

        }
    }

}
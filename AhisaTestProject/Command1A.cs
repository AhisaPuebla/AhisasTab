namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command1A : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            int deletedModelCount = 0;

            using (Transaction trans = new Transaction(doc, "Delete Unplaced Model Groups"))
            {
                trans.Start();

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

            TaskDialog.Show("Delete Model Groups", $"{deletedModelCount} unplaced model group types deleted.");
            return Result.Succeeded;
        }

        internal static PushButtonData GetButtonData()
        {
            return new Common.ButtonDataClass(
                "btnDeleteModelGroups",
                "Delete Unplaced Model Groups",
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.DeleteUnusedMGroups32x32,
                Properties.Resources.DeleteUnusedMGroups16x16,
                "Deletes all unplaced model group types in the project.").Data;
        }
    }
}



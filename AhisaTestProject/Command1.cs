﻿namespace AhisaTestProject
{
    [Transaction(TransactionMode.Manual)]
    public class Command1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            int deletedDetailCount = 0;

            using (Transaction trans = new Transaction(doc, "Delete Unplaced Detail Groups"))
            {
                trans.Start();

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

                trans.Commit();
            }

            TaskDialog.Show("Delete Detail Groups", $"{deletedDetailCount} unplaced detail group types deleted.");
            return Result.Succeeded;
        }

  

        internal static PushButtonData GetButtonData()
        {
            return new Common.ButtonDataClass(
                "btnDeleteDetailGroups",
                "Delete Unplaced Detail Groups",
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.DeleteUnusedDGroups32x32,
                Properties.Resources.DeleteUnusedDGroups16x16,
                "Deletes all unplaced detail group types in the project.").Data;
        }
    }
}



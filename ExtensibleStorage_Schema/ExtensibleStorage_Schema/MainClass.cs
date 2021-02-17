using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace ExtensibleStorage_Schema
{
    [Transaction(TransactionMode.Manual)]
    public class MainClass : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // ui document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            // document
            Document doc = commandData.Application.ActiveUIDocument.Document;
            // first element from selection
            ElementId elemId = uidoc.Selection.GetElementIds().FirstOrDefault();
            Element elem = doc.GetElement(elemId);

            // transaction 
            Transaction t = new Transaction(doc);
            t.Start("Create Schema");

            // build schema
            SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("A796A94F-9E45-4E82-B716-503178B74446"));
            // access level
            schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
            schemaBuilder.SetWriteAccessLevel(AccessLevel.Vendor);
            // vendor id
            schemaBuilder.SetVendorId("TwentyTwo");
            // schema name
            schemaBuilder.SetSchemaName("MySchemaName");

            // create a field to store a string
            FieldBuilder fieldBuilder = schemaBuilder.AddSimpleField("MySampleField", typeof(string));
            fieldBuilder.SetDocumentation("Documentation for sample schema field.");
            // finish (register)
            Schema schema = schemaBuilder.Finish();
            // create entity (instance)
            Entity entity = new Entity(schema);
            // get field from schema
            Field field = schema.GetField("MySampleField");
            // assign value/data
            string data = "Hello, this is a field sample value.";
            entity.Set(field, data);
            // assign entity to element
            elem.SetEntity(entity);

            // to display dialog
            TaskDialog taskDialog = new TaskDialog("Stored Field Value")
            {
                MainContent = "Field Value : " + data
            };
            taskDialog.Show();

            // close transaction
            t.Commit();

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class GetSchemaClass : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // ui document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            // current document
            Document doc = commandData.Application.ActiveUIDocument.Document;
            // first element from selection
            ElementId elemId = uidoc.Selection.GetElementIds().FirstOrDefault();
            Element elem = doc.GetElement(elemId);

            // get schema by name (custom method)
            Schema schema = GetSchemaByName("MySchemaName");

            // get entity from schema (saved with the element) 
            Entity getEntity = elem.GetEntity(schema);
            // get value of the field from entity
            string fieldMsg = getEntity.Get<string>(schema.GetField("MySampleField"));

            // to display dialog
            TaskDialog taskDialog = new TaskDialog("Get Field Value")
            {
                MainContent = "Field Name : MySampleField" + "\nField Value : " + fieldMsg
            };
            taskDialog.Show();

            return Result.Succeeded;
        }

        // to collect schema by names
        public static Schema GetSchemaByName(string name)
        {
            // schema
            Schema schema = null;
            // list of schema in memory
            IList<Schema> schemas = Schema.ListSchemas();

            if (schemas != null && schemas.Count > 0)
            {
                // iterate schema list
                foreach (Schema s in schemas)
                {
                    if (s.SchemaName == name)
                    {
                        schema = s;
                        break;
                    }
                }
            }
            return schema;
        }
    }
}

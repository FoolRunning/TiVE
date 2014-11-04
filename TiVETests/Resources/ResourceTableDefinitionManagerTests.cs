using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProdigalSoftware.TiVE.Resources;
using ProdigalSoftware.Utils;

namespace TiVETests.Resources
{
    /// <summary>
    /// Test fixture for testing the <see cref="ResourceTableDefinitionManager"/> class
    /// </summary>
    [TestFixture]
    public class ResourceTableDefinitionManagerTests
    {
        #region ParseResourceDefinition tests - invalid
        [Test]
        public void ParseResourceDefinition_InvalidTableName()
        {
            ResourceTableDefinitionManager manager = new ResourceTableDefinitionManager();
            Assert.That(() => manager.ParseResourceDefinition("[]"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[name"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("name]"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("more[name]"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[name]more"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[name with spaces]"), Throws.TypeOf<InvalidResourceDefinitionException>());
        }

        [Test]
        public void ParseResourceDefinition_InvalidValueName()
        {
            ResourceTableDefinitionManager manager = new ResourceTableDefinitionManager();
            Assert.That(() => manager.ParseResourceDefinition("[table]\n i, r, 1"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\n: i, r, 1"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue value: i, r, 1"), Throws.TypeOf<InvalidResourceDefinitionException>());
        }

        [Test]
        public void ParseResourceDefinition_InvalidValueTypes()
        {
            ResourceTableDefinitionManager manager = new ResourceTableDefinitionManager();
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: , r, 1"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: in, r, 1"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: double, r, 1.0"), Throws.TypeOf<InvalidResourceDefinitionException>());
        }

        [Test]
        public void ParseResourceDefinition_InvalidRequired()
        {
            ResourceTableDefinitionManager manager = new ResourceTableDefinitionManager();
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: i, req, 1"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: i, monkey, 1"), Throws.TypeOf<InvalidResourceDefinitionException>());
        }

        [Test]
        public void ParseResourceDefinition_InvalidBoolean()
        {
            ResourceTableDefinitionManager manager = new ResourceTableDefinitionManager();
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: b, , monkey"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: b, , 1.0"), Throws.TypeOf<InvalidResourceDefinitionException>());
        }

        [Test]
        public void ParseResourceDefinition_InvalidInteger()
        {
            ResourceTableDefinitionManager manager = new ResourceTableDefinitionManager();
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: i, , "), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: i, , 1.0"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: i, , 0x1.0"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: i, , 0xG01"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: i, , 0x-AF1"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: i, , monkey"), Throws.TypeOf<InvalidResourceDefinitionException>());
        }

        [Test]
        public void ParseResourceDefinition_InvalidFloat()
        {
            ResourceTableDefinitionManager manager = new ResourceTableDefinitionManager();
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: f, , "), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: f, , 1.0ab"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: f, , monkey"), Throws.TypeOf<InvalidResourceDefinitionException>());
        }

        [Test]
        public void ParseResourceDefinition_InvalidColor()
        {
            ResourceTableDefinitionManager manager = new ResourceTableDefinitionManager();
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: c, , "), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: c, , 1"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: c, , (1)"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: c, , (1.0, 1.0)"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: c, , (255, 255)"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: c, , (1.0, 255, 0, 255)"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: c, , (255, 255, 255, 1.0)"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: c, , 255, 255, 255, 255"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: c, , (255, 255, 255, 255, 5)"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: c, , (255d, 255c, 255b, 255a)"), Throws.TypeOf<InvalidResourceDefinitionException>());
            Assert.That(() => manager.ParseResourceDefinition("[table]\nvalue: c, , monkey"), Throws.TypeOf<InvalidResourceDefinitionException>());
        }
        #endregion

        #region ParseResourceDefinition tests - valid
        [Test]
        public void ParseResourceDefinition_Empty()
        {
            ResourceTableDefinitionManager manager = new ResourceTableDefinitionManager();
            manager.ParseResourceDefinition("");

            Assert.That(manager.Definitions.Count(), Is.EqualTo(0));
        }

        [Test]
        public void ParseResourceDefinition_ParseBoolean()
        {
            ResourceTableDefinitionManager manager = new ResourceTableDefinitionManager();
            manager.ParseResourceDefinition(@"
[myBoolTable]
useIt       : Boolean   ,           , true 
doIt        : bool      , R         , TrUE
other       : b         , required  , false
quitIt      : b         ,           , FaLse");

            List<TableDefinition> tables = manager.Definitions.ToList();
            Assert.That(tables.Count, Is.EqualTo(1));
            Assert.That(tables[0].Name, Is.EqualTo("myBoolTable"));

            List<EntryDefinition> values = tables[0].Entries.ToList();
            Assert.That(values.Count, Is.EqualTo(4));
            VerifyValueDef(values[0], "useIt", EntryValueType.Boolean, false, true);
            VerifyValueDef(values[1], "doIt", EntryValueType.Boolean, true, true);
            VerifyValueDef(values[2], "other", EntryValueType.Boolean, true, false);
            VerifyValueDef(values[3], "quitIt", EntryValueType.Boolean, false, false);
        }

        [Test]
        public void ParseResourceDefinition_ParseInteger()
        {
            ResourceTableDefinitionManager manager = new ResourceTableDefinitionManager();
            manager.ParseResourceDefinition(@"
[myIntTable]
time        : Integer   ,           , 32 
distance    : int       , R         , 10703
other       : i         , required  , 0x1FA");

            List<TableDefinition> tables = manager.Definitions.ToList();
            Assert.That(tables.Count, Is.EqualTo(1));
            Assert.That(tables[0].Name, Is.EqualTo("myIntTable"));

            List<EntryDefinition> values = tables[0].Entries.ToList();
            Assert.That(values.Count, Is.EqualTo(3));
            VerifyValueDef(values[0], "time", EntryValueType.Integer, false, 32);
            VerifyValueDef(values[1], "distance", EntryValueType.Integer, true, 10703);
            VerifyValueDef(values[2], "other", EntryValueType.Integer, true, 0x1FA);
        }

        [Test]
        public void ParseResourceDefinition_ParseFloat()
        {
            ResourceTableDefinitionManager manager = new ResourceTableDefinitionManager();
            manager.ParseResourceDefinition(@"
[myFloatTable]
time:       FloAt   ,           , 3.2 
distance:   float   ,R          , 100
other:      f       ,Required   , 1.059E5");

            List<TableDefinition> tables = manager.Definitions.ToList();
            Assert.That(tables.Count, Is.EqualTo(1));
            Assert.That(tables[0].Name, Is.EqualTo("myFloatTable"));

            List<EntryDefinition> values = tables[0].Entries.ToList();
            Assert.That(values.Count, Is.EqualTo(3));
            VerifyValueDef(values[0], "time", EntryValueType.Float, false, 3.2f);
            VerifyValueDef(values[1], "distance", EntryValueType.Float, true, 100f);
            VerifyValueDef(values[2], "other", EntryValueType.Float, true, 1.059E5f);
        }

        [Test]
        public void ParseResourceDefinition_ParseString()
        {
            ResourceTableDefinitionManager manager = new ResourceTableDefinitionManager();
            manager.ParseResourceDefinition(@"
[myStringTable]
caption:    String  ,           , My caption!
dialog:     Str     ,R          , This, is some, text with, commas?
other:      s       ,Required   , ");

            List<TableDefinition> tables = manager.Definitions.ToList();
            Assert.That(tables.Count, Is.EqualTo(1));
            Assert.That(tables[0].Name, Is.EqualTo("myStringTable"));

            List<EntryDefinition> values = tables[0].Entries.ToList();
            Assert.That(values.Count, Is.EqualTo(3));
            VerifyValueDef(values[0], "caption", EntryValueType.String, false, "My caption!");
            VerifyValueDef(values[1], "dialog", EntryValueType.String, true, "This, is some, text with, commas?");
            VerifyValueDef(values[2], "other", EntryValueType.String, true, "");
        }

        [Test]
        public void ParseResourceDefinition_ParseColor()
        {
            ResourceTableDefinitionManager manager = new ResourceTableDefinitionManager();
            manager.ParseResourceDefinition(@"
[myColorTable]
startColor: Color   ,           , (152, 27, 78)
endColor:   c       ,R          , (192, 0, 210, 85)
backColor:  c       ,           , (FAF0FF)
textColor:  c       ,           , (FAF0FF0F)");

            List<TableDefinition> tables = manager.Definitions.ToList();
            Assert.That(tables.Count, Is.EqualTo(1));
            Assert.That(tables[0].Name, Is.EqualTo("myColorTable"));

            List<EntryDefinition> values = tables[0].Entries.ToList();
            Assert.That(values.Count, Is.EqualTo(4));
            VerifyValueDef(values[0], "startColor", EntryValueType.Color, false, new Color4b(152, 27, 78, 255));
            VerifyValueDef(values[1], "endColor", EntryValueType.Color, true, new Color4b(192, 0, 210, 85));
            VerifyValueDef(values[2], "backColor", EntryValueType.Color, false, new Color4b(250, 240, 255, 255));
            VerifyValueDef(values[3], "textColor", EntryValueType.Color, false, new Color4b(250, 240, 255, 15));
        }

        [Test]
        public void ParseResourceDefinition_MultipleTables()
        {
            ResourceTableDefinitionManager manager = new ResourceTableDefinitionManager();
            manager.ParseResourceDefinition(@"
[firstTable]
time:       float   , R         , 3.2 
caption:    string  , Required  , caption-y
maxCount:   int     ,           , 0x1FA
color:      color   ,           , (152, 27, 78)

[secondTable]
distance:   float   ,           , 32.5
dialog:     string  ,           , My dialog
minCount:   int     , R         , 21
otherColor: color   , required  , (192, 0, 210, 85)");

            List<TableDefinition> tables = manager.Definitions.ToList();
            Assert.That(tables.Count, Is.EqualTo(2));
            Assert.That(tables[0].Name, Is.EqualTo("firstTable"));

            List<EntryDefinition> values = tables[0].Entries.ToList();
            Assert.That(values.Count, Is.EqualTo(4));
            VerifyValueDef(values[0], "time", EntryValueType.Float, true, 3.2f);
            VerifyValueDef(values[1], "caption", EntryValueType.String, true, "caption-y");
            VerifyValueDef(values[2], "maxCount", EntryValueType.Integer, false, 0x1FA);
            VerifyValueDef(values[3], "color", EntryValueType.Color, false, new Color4b(152, 27, 78, 255));

            Assert.That(tables[1].Name, Is.EqualTo("secondTable"));

            values = tables[1].Entries.ToList();
            Assert.That(values.Count, Is.EqualTo(4));
            VerifyValueDef(values[0], "distance", EntryValueType.Float, false, 32.5f);
            VerifyValueDef(values[1], "dialog", EntryValueType.String, false, "My dialog");
            VerifyValueDef(values[2], "minCount", EntryValueType.Integer, true, 21);
            VerifyValueDef(values[3], "otherColor", EntryValueType.Color, true, new Color4b(192, 0, 210, 85));
        }
        #endregion

        #region Private helper methods
        private static void VerifyValueDef(EntryDefinition definition, string expectedName, 
            EntryValueType expectedValueType, bool expectedRequired, object expectedDefault)
        {
            Assert.That(definition, Is.Not.Null, "Definition should not be null");
            Assert.That(definition.Name, Is.EqualTo(expectedName), "Wrong expected name");
            Assert.That(definition.ValueType, Is.EqualTo(expectedValueType), "Wrong expected type");
            Assert.That(definition.Required, Is.EqualTo(expectedRequired), "Wrong expected required value");
            Assert.That(definition.DefaultValue, Is.EqualTo(expectedDefault), "Wrong default value");
        }
        #endregion
    }
}

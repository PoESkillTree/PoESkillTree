using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using POESKillTree.Localization;
using POESKillTree.Utils;

namespace POESKillTree.TreeGenerator.Model.PseudoAttributes
{
    public class PseudoAttributeDataInvalidException : Exception
    {
        public PseudoAttributeDataInvalidException(string message)
            : base(message)
        { }

        public PseudoAttributeDataInvalidException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    public class PseudoAttributeLoader
    {
        private static readonly string DataPath = AppData.GetFolder(Path.Combine("Data", "PseudoAttributes"));

        private readonly Dictionary<string, PseudoAttribute> _pseudoNameDict = new Dictionary<string, PseudoAttribute>();

        private readonly Dictionary<string, List<string>> _nestedPseudosDict = new Dictionary<string, List<string>>();
        
        public List<PseudoAttribute> LoadPseudoAttributes(XmlPseudoAttributes xmlPseudoAttributes = null)
        {
            XmlPseudoAttribute[] xmlPseudos;
            if (xmlPseudoAttributes == null)
            {
                // Deserialize all files in DataPath that end with .xml
                // and select the XmlPseudoAttribute objects.
                xmlPseudos = (from file in Directory.GetFiles(DataPath)
                    where file.EndsWith(".xml")
                    from pseudo in DeserializeFile(file).PseudoAttributes
                    select pseudo).ToArray();
            }
            else
            {
                xmlPseudos = xmlPseudoAttributes.PseudoAttributes;
            }

            if (xmlPseudos.Length == 0)
            {
                throw new PseudoAttributeDataInvalidException(string.Format(L10n.Message("No Pseudo Attributes loaded. Make sure {0} is not empty."), DataPath));
            }

            _pseudoNameDict.Clear();
            _nestedPseudosDict.Clear();
            // Inductive converting.
            var pseudos = ConvertFromXml(xmlPseudos);
            // Replace nested pseudo attributes by proper object.
            ResolveNesting(pseudos);
            
            return pseudos;
        }

        private XmlPseudoAttributes DeserializeFile(string filename)
        {
            var ser = new XmlSerializer(typeof(XmlPseudoAttributes));
            using (var reader = XmlReader.Create(filename))
            {
                try
                {
                    var ret = (XmlPseudoAttributes)ser.Deserialize(reader);
                    if (ret.PseudoAttributes == null) ret.PseudoAttributes = new XmlPseudoAttribute[0];
                    return ret;
                }
                catch (InvalidOperationException e)
                {
                    throw new PseudoAttributeDataInvalidException(L10n.Message("Invalid Xml file: ") + e.Message + " " + (e.InnerException != null ? e.InnerException.Message : ""), e);
                }
            }
        }

        private List<PseudoAttribute> ConvertFromXml(IEnumerable<XmlPseudoAttribute> xmlPseudoAttributes)
        {
            var pseudos = new List<PseudoAttribute>();
            foreach (var xmlPseudo in xmlPseudoAttributes)
            {
                var pseudo = new PseudoAttribute(xmlPseudo.Name, xmlPseudo.Group);
                _pseudoNameDict[pseudo.Name] = pseudo;
                if (xmlPseudo.Hidden != "True")
                {
                    pseudos.Add(pseudo);
                }

                _nestedPseudosDict[pseudo.Name] = new List<string>();
                foreach (var xmlNestedPseudo in xmlPseudo.PseudoAttributes ?? new XmlNestedPseudoAttribute[0])
                {
                    _nestedPseudosDict[pseudo.Name].Add(xmlNestedPseudo.Name);
                }

                foreach (var xmlAttr in xmlPseudo.Attributes ?? new XmlNestedAttribute[0])
                {
                    var attr = new Attribute(xmlAttr.Name);
                    if (xmlAttr.ConversionMultiplierSpecified)
                    {
                        attr.ConversionMultiplier = (float) xmlAttr.ConversionMultiplier;
                    }
                    pseudo.Attributes.Add(attr);

                    var xmlConditions = xmlAttr.Conditions ?? new XmlAttributeConditions() {Items = new object[0]};
                    for (var i = 0; i < xmlConditions.Items.Length; i++)
                    {
                        ICondition condition;
                        var xmlCondition = xmlConditions.Items[i];
                        switch (xmlConditions.ItemsElementName[i])
                        {
                            case XmlItemsChoiceType.AndComposition:
                                var xmlAndComp = (XmlAndComposition)xmlCondition;
                                var andComp = new AndComposition();
                                if (xmlAndComp.Keystone != null)
                                {
                                    andComp.Conditions.Add(new KeystoneCondition(xmlAndComp.Keystone));
                                }
                                if (xmlAndComp.OffHand != null)
                                {
                                    andComp.Conditions.Add(new OffHandCondition(xmlAndComp.OffHand));
                                }
                                if (xmlAndComp.Tag != null)
                                {
                                    andComp.Conditions.Add(new TagCondition(xmlAndComp.Tag));
                                }
                                if (xmlAndComp.WeaponClass != null)
                                {
                                    andComp.Conditions.Add(new WeaponClassCondition(xmlAndComp.WeaponClass));
                                }

                                if (xmlAndComp.NotCondition != null)
                                {
                                    var xmlNotCond = xmlAndComp.NotCondition;
                                    ICondition innerCond;
                                    if (xmlNotCond.Keystone != null)
                                    {
                                        innerCond = new KeystoneCondition(xmlNotCond.Keystone);
                                    }
                                    else if (xmlNotCond.OffHand != null)
                                    {
                                        innerCond = new OffHandCondition(xmlNotCond.OffHand);
                                    }
                                    else if (xmlNotCond.Tag != null)
                                    {
                                        innerCond = new TagCondition(xmlNotCond.Tag);
                                    }
                                    else if (xmlNotCond.WeaponClass != null)
                                    {
                                        innerCond = new WeaponClassCondition(xmlNotCond.WeaponClass);
                                    }
                                    else
                                    {
                                        throw new PseudoAttributeDataInvalidException(
                                            string.Format(L10n.Message("Empty not condition in attribute {0} in pseudo attribute {1}"), attr.Name, pseudo.Name));
                                    }
                                    andComp.Conditions.Add(new NotCondition(innerCond));
                                }
                                condition = andComp;
                                break;

                            case XmlItemsChoiceType.OffHand:
                                condition = new OffHandCondition(xmlCondition.ToString());
                                break;
                            case XmlItemsChoiceType.Tag:
                                condition = new TagCondition(xmlCondition.ToString());
                                break;
                            case XmlItemsChoiceType.WeaponClass:
                                condition = new WeaponClassCondition(xmlCondition.ToString());
                                break;
                            case XmlItemsChoiceType.Keystone:
                                condition = new KeystoneCondition(xmlCondition.ToString());
                                break;

                            default:
                                throw new PseudoAttributeDataInvalidException(
                                    string.Format(L10n.Message("Unsupported condition type in attribute {0} in pseudo attribute {1}"), attr.Name, pseudo.Name));
                        }
                        attr.Conditions.Add(condition);
                    }
                }
            }
            return pseudos;
        }

        private void ResolveNesting(List<PseudoAttribute> pseudos)
        {
            foreach (var pseudo in pseudos)
            {
                var nestedNames = new Queue<string>(_nestedPseudosDict[pseudo.Name]);
                var depth = 0;
                while (nestedNames.Count != 0)
                {
                    if (depth++ > 100)
                    {
                        throw new PseudoAttributeDataInvalidException(L10n.Message("A PseudoAttribute is nested in itself or nesting depth is too high"));
                    }
                    var name = nestedNames.Dequeue();
                    // Add Attributes of current nested one to top level PseudoAttribute.
                    try
                    {
                        pseudo.Attributes.AddRange(_pseudoNameDict[name].Attributes);
                    }
                    catch (KeyNotFoundException e)
                    {
                        throw new PseudoAttributeDataInvalidException(string.Format(L10n.Message("Nested PseudoAttribute {0} does not exist as top level PseudoAttribute"), name), e);
                    }
                    // Enqueue pseudo attributes nested in this one.
                    foreach (var newName in _nestedPseudosDict[name])
                    {
                        nestedNames.Enqueue(newName);
                    }
                }
            }
        }
    }
}
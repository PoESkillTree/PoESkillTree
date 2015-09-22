using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using POESKillTree.Localization;
using POESKillTree.TreeGenerator.Model;
using POESKillTree.TreeGenerator.Model.Conditions;
using POESKillTree.Utils;
using Attribute = POESKillTree.TreeGenerator.Model.Attribute;

namespace POESKillTree.TreeGenerator.Utils
{
    public class PseudoAttributeDataInvalidException : Exception
    {
        public PseudoAttributeDataInvalidException()
        { }

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

        private static List<PseudoAttribute> _cachedPseudoAttributes;

        private readonly bool _useCache;

        public PseudoAttributeLoader(bool useCache = true)
        {
            _useCache = useCache;
        }
        
        public List<PseudoAttribute> LoadPseudoAttributes()
        {
            if (_useCache && _cachedPseudoAttributes != null)
            {
                return _cachedPseudoAttributes;
            }

            // Deserialize all files in DataPath that end with .xml
            // and select the XmlPseudoAttribute objects.
            var xmlPseudos = from file in Directory.GetFiles(DataPath)
                             where file.EndsWith(".xml")
                             from pseudo in DeserializeFile(file).PseudoAttributes
                             select pseudo;
            // Inductive converting.
            var pseudos = ConvertFromXml(xmlPseudos);
            // Replace nested pseudo attributes by proper object.
            ResolveNesting(pseudos);
            // Assign ids to attributes.
            AssignIds(pseudos);
            
            if (_useCache)
            {
                _cachedPseudoAttributes = pseudos;
            }
            return pseudos;
        }

        private XmlPseudoAttributes DeserializeFile(string filename)
        {
            var ser = new XmlSerializer(typeof(XmlPseudoAttributes));
            using (var reader = XmlReader.Create(filename))
            {
                try
                {
                    return (XmlPseudoAttributes)ser.Deserialize(reader);
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
                var pseudo = new PseudoAttribute(xmlPseudo.Name)
                {
                    Group = xmlPseudo.Group,
                    Hidden = xmlPseudo.Hidden == "True"
                };
                foreach (var xmlNestedPseudo in xmlPseudo.PseudoAttributes ?? new XmlNestedPseudoAttribute[0])
                {
                    pseudo.Attributes.Add(new PseudoAttribute(xmlNestedPseudo.Name));
                }
                foreach (var xmlAttr in xmlPseudo.Attributes ?? new XmlNestedAttribute[0])
                {
                    var attr = new Attribute(xmlAttr.Name);
                    if (xmlAttr.ConversionRateSpecified)
                    {
                        attr.ConversionRate = (double) xmlAttr.ConversionRate;
                    }
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
                                        throw new PseudoAttributeDataInvalidException(L10n.Message("Empty not condition in attribute ") + attr.Name
                                            + L10n.Message(" in pseudo attribute ") + pseudo.Name);
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
                                throw new PseudoAttributeDataInvalidException(L10n.Message("Unsupported condition type in attribute ") + attr.Name
                                    + L10n.Message(" in pseudo attribute ") + pseudo.Name);
                        }
                        attr.Conditions.Add(condition);
                    }
                    pseudo.Attributes.Add(attr);
                }
                pseudos.Add(pseudo);
            }
            return pseudos;
        }

        private void ResolveNesting(List<PseudoAttribute> pseudos)
        {
            var pseudoNameDict = pseudos.ToDictionary(p => p.Name);

            foreach (var pseudo in pseudos)
            {
                try
                {
                    pseudo.Attributes = (from attr in pseudo.Attributes
                                         select attr is PseudoAttribute ? pseudoNameDict[attr.Name] : attr).ToList();
                }
                catch (KeyNotFoundException e)
                {
                    throw new PseudoAttributeDataInvalidException(L10n.Message("Nested PseudoAttribute does not exist as top level PseudoAttribute: ") + e.Message, e);
                }
            }
        }

        private void AssignIds(List<PseudoAttribute> pseudos)
        {
            var i = 0;
            foreach (var pseudo in pseudos)
            {
                pseudo.Id = i++;
                foreach (var attr in pseudo.Attributes)
                {
                    if (!(attr is PseudoAttribute))
                    {
                        attr.Id = i++;
                    }
                }
            }
        }
    }
}
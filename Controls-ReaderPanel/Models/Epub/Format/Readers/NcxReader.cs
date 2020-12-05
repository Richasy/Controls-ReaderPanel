﻿using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Richasy.Controls.Reader.Models.Epub.Format.Readers
{
    internal static class NcxReader
    {
        public static NcxDocument Read(XDocument xml)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            if (xml.Root == null) throw new ArgumentException("XML document has no root element.", nameof(xml));

            var navMap = xml.Root.Element(NcxElements.NavMap);
            var pageList = xml.Root.Element(NcxElements.PageList);
            var navInfo = pageList?.Element(NcxElements.NavInfo);
            var navList = xml.Root.Element(NcxElements.NavList);

            var ncx = new NcxDocument
            {
                Meta = xml.Root.Element(NcxElements.Head)?.Elements(NcxElements.Meta).AsObjectList(elem => new NcxMeta
                {
                    Name = (string)elem.Attribute(NcxMeta.Attributes.Name),
                    Content = (string)elem.Attribute(NcxMeta.Attributes.Content),
                    Scheme = (string)elem.Attribute(NcxMeta.Attributes.Scheme)
                }),
                DocTitle = xml.Root.Element(NcxElements.DocTitle)?.Element(NcxElements.Text)?.Value,
                DocAuthor = xml.Root.Element(NcxElements.DocAuthor)?.Element(NcxElements.Text)?.Value,
                NavMap = new NcxNapMap
                {
                    Dom = navMap,
                    NavPoints = navMap == null ? new List<NcxNavPoint>() : navMap.Elements(NcxElements.NavPoint).AsObjectList(ReadNavPoint)
                },
                PageList = pageList == null ? null : new NcxPageList
                {
                    NavInfo = navInfo == null ? null : new NcxNavInfo { Text = navInfo.Element(NcxElements.Text)?.Value },
                    PageTargets = pageList.Elements(NcxElements.PageTarget).AsObjectList(elem => new NcxPageTarget
                    {
                        Id = (string)elem.Attribute(NcxPageTarget.Attributes.Id),
                        Class = (string)elem.Attribute(NcxPageTarget.Attributes.Class),
                        Value = (int?)elem.Attribute(NcxPageTarget.Attributes.Value),
                        Type = (NcxPageTargetType?)(elem.Attribute(NcxPageTarget.Attributes.Type) == null ? null : Enum.Parse(typeof(NcxPageTargetType), (string)elem.Attribute("type"), true)),
                        NavLabelText = elem.Element(NcxElements.NavLabel)?.Element(NcxElements.Text)?.Value,
                        ContentSrc = (string)elem.Element(NcxElements.Content)?.Attribute(NcxPageTarget.Attributes.ContentSrc)
                    })
                },
                NavList = navList == null ? null : new NcxNavList
                {
                    Id = (string)navList.Attribute("id"),
                    Class = (string)navList.Attribute("class"),
                    Label = navList.Element(NcxElements.NavLabel)?.Element(NcxElements.Text)?.Value,
                    NavTargets = navList.Elements(NcxElements.NavTarget).AsObjectList(elem => new NcxNavTarget
                    {
                        Id = (string)elem.Attribute("id"),
                        Class = (string)elem.Attribute("class"),
                        Label = navList.Element(NcxElements.NavLabel)?.Element(NcxElements.Text)?.Value,
                        PlayOrder = (int?)elem.Attribute("playOrder"),
                        ContentSource = (string)elem.Element(NcxElements.Content)?.Attribute("src")
                    })
                }
            };
            
            return ncx;
        }

        private static NcxNavPoint ReadNavPoint(XElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (element.Name != NcxElements.NavPoint) throw new ArgumentException("The element is not <navPoint>", nameof(element));
            string id = (string)element.Attribute(NcxNavPoint.Attributes.Id);
            string cla = (string)element.Attribute(NcxNavPoint.Attributes.Class);
            string navText = element.Element(NcxElements.NavLabel)?.Element(NcxElements.Text)?.Value;
            string src = (string)element.Element(NcxElements.Content)?.Attribute(NcxNavPoint.Attributes.ContentSrc);
            int? order = -1;
            try
            {
                order = (int?)element.Attribute(NcxNavPoint.Attributes.PlayOrder);
            }
            catch (Exception)
            {}
            return new NcxNavPoint
            {
                Id = id,
                Class = cla,
                NavLabelText = navText,
                ContentSrc = src,
                PlayOrder = order,
                NavPoints = element.Elements(NcxElements.NavPoint).AsObjectList(ReadNavPoint)
            };
        }
    }
}

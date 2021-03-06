﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Vitevic.Foundation.Extensions
{
    public static class XmlExtensions
    {
        public static XElement RequiredElement(this XElement root, XName name)
        {
            Debug.Assert(root != null);

            var element = root.Element(name);
            if (element == null)
            {
                element = new XElement(name);
                root.Add(element);
            }

            return element;
        }

        public static String OptionalElementStr(this XElement root, XName name)
        {
            Debug.Assert(root != null);

            var element = root.Element(name);

            return element != null ? element.Value : null;
        }
    }
}

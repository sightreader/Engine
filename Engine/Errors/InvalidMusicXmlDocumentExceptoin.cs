﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SightReader.Engine.Errors
{
    public class InvalidMusicXmlDocumentException : Exception
    {
        public InvalidMusicXmlDocumentException(XmlException? innerException, string error = "There was an error parsing the XML document as MusicXML.")
            : base(error, innerException)
        {
        }
    }
}
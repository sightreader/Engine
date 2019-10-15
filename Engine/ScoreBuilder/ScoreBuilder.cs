using MusicXmlSchema;
using SightReader.Engine.Errors;
using SightReader.Engine.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SightReader.Engine.ScoreBuilder
{
    class ScoreBuilder
    {
        private ScorePartwise CreateFromMusicXml(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(ScorePartwise));

            try
            {
               return (ScorePartwise)serializer.Deserialize(stream);
            }
            catch (Exception ex) when (ex.InnerException is XmlException)
            {
                throw new InvalidMusicXmlDocumentException(ex.InnerException as XmlException);
            }
        }
    }
}

/***************************************************************************
=========================================================================
  								Rabit
  
DireenTech Inc. 
www.DireenTech.com

Develpers:
	Harry Direen PhD

Start Date:  August, 2015
=========================================================================
               Copyright (c) 2015 DireenTech Inc.
All or Portions of This Software are Copyrighted by DireenTech Inc.
                        Company Proprietary
=========================================================================
****************************************************************************/

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Rabit.Utils
{
    /// <summary>
    /// A Generic XML file reader/writer.  
    /// Serializes a Class/Object to and XML files and will
    /// also read an xml file into the given class/object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class XMLFileReaderWriter<T> where T : new() 
    {
        private string _configFilename = null;
        public string ConfigFilename
        {
            get
            {
                if (null == _configFilename)
                    _configFilename = string.Empty;
                return string.Copy(_configFilename);
            }
            set
            {
                _configFilename = string.Copy(value);
            }
        }

        public bool Error = false;

        public XMLFileReaderWriter()
        {
            //Default config filename
            _configFilename = "unknownObject.xml";
        }

        public XMLFileReaderWriter(string cfgFilename)
        {
            //Default config filename
            _configFilename = cfgFilename;
        }

        public T Load()
        {
            XmlSerializer xmlSer = null;
            FileStream fs = null;
            Error = true;
            T xmlObject = new T();
            try
            {
                fs = File.Open(_configFilename, FileMode.Open, FileAccess.Read, FileShare.None);

                xmlSer = new XmlSerializer((new T()).GetType());
                /* If the XML document has been altered with unknown 
                nodes or attributes, handle them with the 
                UnknownNode and UnknownAttribute events.*/
                xmlSer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
                xmlSer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);

                xmlObject = (T)xmlSer.Deserialize(fs);
                fs.Flush();
                fs.Close();
                Error = false;
            }
            catch (Exception e)
            {
				Console.WriteLine("Failure loading config file with error, " + e.Message);
                Error = true;
            }
            return xmlObject;
        }


        public bool Save(T xmlObject, string filename)
        {
            _configFilename = filename;
            return Save(xmlObject);
        }

        public bool Save(T xmlObject)
        {
            XmlSerializer xmlSer = null;
            FileStream fs = null;
            Error = true;

            if (xmlObject == null)
            {
                Error = true;
                return Error;
            }
            try
            {
                fs = File.Open(_configFilename, FileMode.Create, FileAccess.Write, FileShare.None);
                xmlSer = new XmlSerializer(xmlObject.GetType());
                xmlSer.Serialize(fs, xmlObject);
                fs.Flush();
                fs.Close();
                Error = false;
            }
            catch (Exception e)
            {
				Console.WriteLine("Failure saving config file with error, " + e.Message);
                Error = true;
            }
            return Error;
        }

        private void serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
			Console.WriteLine("Config file Unknown Node:" + e.Name + "\t" + e.Text);
        }

        private void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
			Console.WriteLine("Config file Unknown attribute " +
                                attr.Name + "='" + attr.Value + "'");
        }

    }
}

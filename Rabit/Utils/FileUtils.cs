/***************************************************************************
=========================================================================
			DireenTech Inc. (www.DireenTech.com)

  File Name:      FileUtils.cs
  Created:        Aug. 30, 2012
  Author:		  Harry Direen, DireenTech
  Description:    Various File and Directory Utility Functions 
	
=========================================================================
               Copyright (c) 2012 - 2015 DireenTech Inc.
All or Portions of This Software are Copyrighted by DireenTech Inc.
                        Company Proprietary
=========================================================================
***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Rabit.Utils
{
    public class FileUtils
    {
        public static string createDirFilename(string dirName, string fileName)
        {
            return string.Concat(dirName, RabitGlobals.DirSeparator, fileName);
        }

        /// <summary>
        /// Check to see if a directory exists, if it does not exist... create the directory.
        /// </summary>
        /// <param name="dirname"></param>
        /// <returns>true if there was an error creating the directory, false otherwise.</returns>
        public static bool createDirectory(string dirname)
        {
            bool error = false;
            try
            {
                DirectoryInfo dinfo = new DirectoryInfo(dirname);
                if (!dinfo.Exists)
                {
                    dinfo.Create();
                }
            }
            catch (Exception ex)
            {
				Console.WriteLine("Could not create directory: " + dirname
                                    + "System Err: " + ex.Message.ToString());
                error = true;
            }
            return error;
        }


        /// <summary>
        /// Add and index to a filename:
        /// example:  datafile.dat --> datafile_123.dat
        /// </summary>
        /// <param name="fn"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static string addIndexToFilename(string fn, int idx)
        {
            string fnwidx = fn;
            if (fn != null && fn.Length > 0)
            {
                string bn = fn;
                string ext = "";
                int n = fn.LastIndexOf('.');
                if (n > 0)
                {
                    int m = fn.Length - n;
                    bn = fn.Substring(0, n);
                    if (m > 0)
                        ext = fn.Substring(n, m);
                }
                fnwidx = string.Concat(bn, "_", idx.ToString(), ext);
            }
            return fnwidx;
        }



        /// <summary>
        /// Find a file on the system, return the file name.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //public static string findFile(string startDir, string defaultFileExt)
        //{
        //    string filename = null;
        //    OpenFileDialog fileDialog = new OpenFileDialog();
        //    //fileDialog.Filter = "XML file (*.xml)|*.xml|All files (*.*)|*.*";
        //    if (defaultFileExt == null || defaultFileExt.Length < 1)
        //        defaultFileExt = "dat";

        //    if (startDir == null || startDir.Length < 1)
        //        startDir = "c:\\LaserToolInspector";

        //    fileDialog.Filter = string.Concat("File (*.", defaultFileExt, ")|*.", defaultFileExt, "|All files (*.*)|*.*");
        //    //fileDialog.Filter = "File (*.dat)|*.dat|All files (*.*)|*.*";
        //    fileDialog.Title = "Find File";
        //    fileDialog.InitialDirectory = startDir;

        //    if (fileDialog.ShowDialog() == DialogResult.OK)
        //    {
        //        filename = fileDialog.FileName;
        //    }
        //    return filename;
        //}




    }
}

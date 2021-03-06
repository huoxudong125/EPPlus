/* 
 * You may amend and distribute as you like, but don't remove this header!
 * 
 * EPPlus provides server-side generation of Excel 2007 spreadsheets.
 * EPPlus is a fork of the ExcelPackage project
 * See http://www.codeplex.com/EPPlus for details.
 * 
 * All rights reserved.
 * 
 * EPPlus is an Open Source project provided under the 
 * GNU General Public License (GPL) as published by the 
 * Free Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
 * 
 * The GNU General Public License can be viewed at http://www.opensource.org/licenses/gpl-license.php
 * If you unfamiliar with this license or have questions about it, here is an http://www.gnu.org/licenses/gpl-faq.html
 * 
 * The code for this project may be used and redistributed by any means PROVIDING it is 
 * not sold for profit without the author's written consent, and providing that this notice 
 * and the author's name and all copyright notices remain intact.
 * 
 * All code and executables are provided "as is" with no warranty either express or implied. 
 * The author accepts no liability for any damage or loss of business that this product may cause.
 *
 * 
 * Code change notes:
 * 
 * Author							Change						Date
 * ******************************************************************************
 * Jan K�llman		                Initial Release		        2009-10-01
 *******************************************************************************/

/* 
 * You may amend and distribute as you like, but don't remove this header!
 * 
 * ExcelPackage provides server-side generation of Excel 2007 spreadsheets.
 * See http://www.codeplex.com/ExcelPackage for details.
 * 
 * Copyright 2007 � Dr John Tunnicliffe 
 * mailto:dr.john.tunnicliffe@btinternet.com
 * All rights reserved.
 * 
 * ExcelPackage is an Open Source project provided under the 
 * GNU General Public License (GPL) as published by the 
 * Free Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
 * 
 * The GNU General Public License can be viewed at http://www.opensource.org/licenses/gpl-license.php
 * If you unfamiliar with this license or have questions about it, here is an http://www.gnu.org/licenses/gpl-faq.html
 * 
 * The code for this project may be used and redistributed by any means PROVIDING it is 
 * not sold for profit without the author's written consent, and providing that this notice 
 * and the author's name and all copyright notices remain intact.
 * 
 * All code and executables are provided "as is" with no warranty either express or implied. 
 * The author accepts no liability for any damage or loss of business that this product may cause.
 */

/*
 * Code change notes:
 * 
 * Author							Change						Date
 * ******************************************************************************
 * John Tunnicliffe		Initial Release		                    01-Jan-2007
 * Jan K�llman          Don't access the XML directly any more.  05-Oct-2009
 * ******************************************************************************
 */
using System;
using System.Xml;
using OfficeOpenXml.Style;
namespace OfficeOpenXml
{
	/// <summary>
	/// Represents an individual column within the worksheet
	/// </summary>
	public class ExcelColumn
	{
		private ExcelWorksheet _xlWorksheet;
		private XmlElement _colElement = null;
		private XmlNamespaceManager _nsManager;

		#region ExcelColumn Constructor
		/// <summary>
		/// Creates a new instance of the ExcelColumn class.  
		/// For internal use only!
		/// </summary>
		/// <param name="Worksheet"></param>
		/// <param name="col"></param>
		protected internal ExcelColumn(ExcelWorksheet Worksheet, int col)
        {
            _xlWorksheet = Worksheet;
            _columnMin = col;
            _columnMax = col;

            //if (col < 1 || col > ExcelPackage.MaxColumns)
            //{
            //    throw(new Exception("Column out of range"));
            //}
            //NameTable nt = new NameTable();
            //_nsManager = new XmlNamespaceManager(nt);
            //_nsManager.AddNamespace("d", ExcelPackage.schemaMain);

            //_xlWorksheet = Worksheet;
            //XmlNode parent = Worksheet.WorksheetXml.SelectSingleNode("//d:cols", _nsManager);
            //if (parent == null)
            //{
            //    parent = (XmlNode)Worksheet.WorksheetXml.CreateElement("cols", ExcelPackage.schemaMain);
            //    XmlNode refChild = Worksheet.WorksheetXml.SelectSingleNode("//d:sheetData", _nsManager);
            //    parent = Worksheet.WorksheetXml.DocumentElement.InsertBefore(parent, refChild);
            //}
            ////Std with for new columns
            //XmlAttribute minAttr;
            //XmlAttribute maxAttr;
            //XmlNode insertBefore = null;
            //// the column definitions cover a range of columns, so find the one we want
            //bool insertBeforeFound = false;
            //foreach (XmlNode colNode in parent.ChildNodes)
            //{
            //    int min = 1;
            //    int max = 1;
            //    minAttr = (XmlAttribute)colNode.Attributes.GetNamedItem("min");
            //    if (minAttr != null)
            //        min = int.Parse(minAttr.Value);
            //    maxAttr = (XmlAttribute)colNode.Attributes.GetNamedItem("max");
            //    if (maxAttr != null)
            //        max = int.Parse(maxAttr.Value);
            //    if (!insertBeforeFound && (col <= min || col <= max))
            //    {
            //        insertBeforeFound = true;
            //        insertBefore = colNode;
            //    }
            //    if (col >= min && col <= max)
            //    {
            //        _colElement = (XmlElement)colNode;
            //        break;
            //    }
            //}
            //if (_colElement == null)
            //{
            //    // create the new column definition
            //    _colElement = Worksheet.WorksheetXml.CreateElement("col", ExcelPackage.schemaMain);
            //    _colElement.SetAttribute("min", col.ToString());
            //    _colElement.SetAttribute("max", col.ToString());

            //    if (insertBefore != null)
            //        parent.InsertBefore(_colElement, insertBefore);
            //    else
            //        parent.AppendChild(_colElement);
            //}
            
            ////Set default width if it is missing
            //if (_colElement.GetAttribute("width") == "") 
            //{
            //    Width = Width;
            //}
        }
		#endregion

		/// <summary>
		/// Returns a reference to the Element that represents the column.
		/// For internal use only!
		/// </summary>
		protected internal XmlElement Element { get { return (_colElement); } }
		
		/// <summary>
		/// Sets the first column the definition refers to.
		/// </summary>
        int _columnMin;
		public int ColumnMin 
		{
            get { return _columnMin;/*(int.Parse(_colElement.GetAttribute("min"))); */}
			set { _columnMin=value;/*_colElement.SetAttribute("min", value.ToString());*/ } 
		}
		
		/// <summary>
		/// Sets the last column the definition refers to.
		/// </summary>
        int _columnMax;
        public int ColumnMax 
		{ 
            get { return _columnMax;/*(int.Parse(_colElement.GetAttribute("max"))); */}
			set { _columnMax = value;/*_colElement.SetAttribute("max", value.ToString());*/ } 
		}
        long _cellID = long.MinValue;
        internal ulong ColumnID
        {
            get
            {
                return ExcelColumn.GetColumnID(_xlWorksheet.SheetID, ColumnMin);
            }
        }
		#region ExcelColumn Hidden
		/// <summary>
		/// Allows the column to be hidden in the worksheet
		/// </summary>
        bool _hidden=false;
        public bool Hidden
		{
			get
			{
                //bool retValue = false;
                //string hidden = _colElement.GetAttribute("hidden", "1");
                //if (hidden == "1") retValue = true;
                //return (retValue);
                return _hidden;
			}
			set
			{
                //if (value)
                //    _colElement.SetAttribute("hidden", "1");
                //else
                //    _colElement.SetAttribute("hidden", "0");
                _hidden = value;
			}
		}
		#endregion

		#region ExcelColumn Width
		/// <summary>
		/// Sets the width of the column in the worksheet
		/// </summary>
        double _width = 10;
        public double Width
		{
			get
			{
                //double retValue = 10;  // default column size
                //string width = _colElement.GetAttribute("width");
                //if (width != "") retValue = double.Parse(width, ExcelWorksheet._ci);
                //return retValue;
                return _width;
			}
			set	
            {
                _width = value;
                //_colElement.SetAttribute("width", value.ToString()); 
            }
		}
        public bool BestFit
        {
            get;
            set;
        }
        public bool Collapsed { get; set; }
        public int OutlineLevel { get; set; }
        public bool Phonetic { get; set; }
        #endregion

		#region ExcelColumn Style
        /// <summary>
        /// The Style applied to the whole column. Only effekt cells with no individual style set. 
        /// Use Range object if you want to set specific styles.
        /// </summary>
        public ExcelStyle Style
        {
            get
            {
                return _xlWorksheet.Workbook.Styles.GetStyleObject(_styleID, _xlWorksheet.PositionID, ExcelCell.GetColumnLetter(ColumnMin));                
            }
        }
        string _styleName="";
        /// <summary>
		/// Sets the style for the entire column using a style name.
		/// </summary>
		public string StyleName
		{
            get
            {
                return _styleName;
            }
            set
            {
                _styleID = _xlWorksheet.Workbook.Styles.GetStyleIdFromName(value);
                _styleName = value;
            }
		}
		/// <summary>
		/// Sets the style for the entire column using the style ID.  
		/// </summary>
        int _styleID = 0;
        public int StyleID
		{
            get
            {
                return _styleID;
            }
            set
            {
                _styleID = value;
            }
		}
		#endregion

		/// <summary>
		/// Returns the range of columns covered by the column definition.
		/// </summary>
		/// <returns>A string describing the range of columns covered by the column definition.</returns>
		public override string ToString()
		{
			return string.Format("Column Range: {0} to {1}", _colElement.GetAttribute("min"), _colElement.GetAttribute("min"));
		}
        /// <summary>
        /// Get the internal RangeID
        /// </summary>
        /// <param name="sheetID">Sheet no</param>
        /// <param name="column">Column</param>
        /// <returns></returns>
        internal static ulong GetColumnID(int sheetID, int column)
        {
            return ((ulong)sheetID) + (((ulong)column) << 15);
        }
    }
}

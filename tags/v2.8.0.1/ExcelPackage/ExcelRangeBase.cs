﻿/* 
 * You may amend and distribute as you like, but don't remove this header!
 * 
 * EPPlus provides server-side generation of Excel 2007 spreadsheets.
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
 * Jan Källman		                Added this class		        2010-01-28
 *******************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using OfficeOpenXml.Style;
using System.Xml;
using System.Drawing;
using System.Globalization;
using System.Collections;
using OfficeOpenXml.Table;
using System.Text.RegularExpressions;
using System.IO;
namespace OfficeOpenXml
{
    /// <summary>
    /// A range of cells 
    /// </summary>
    public class ExcelRangeBase : ExcelAddress, IExcelCell, IDisposable, IEnumerable<ExcelRangeBase>, IEnumerator<ExcelRangeBase>
    {
        protected ExcelWorksheet _worksheet;
        private delegate void _changeProp(_setValue method, object value);
        private delegate void _setValue(object value, int row, int col);
        private _changeProp _changePropMethod;
        private int _styleID;
        #region "Constructors"
        protected internal ExcelRangeBase(ExcelWorksheet xlWorksheet)
        {
            _worksheet = xlWorksheet;
            _ws = _worksheet.Name;
            SetDelegate();  
        }

        protected internal ExcelRangeBase(ExcelWorksheet xlWorksheet, string address) :
            base(address)
        {
            _worksheet = xlWorksheet;
            _ws = _worksheet.Name;
            SetDelegate();
        }   
        #endregion
        #region Set Value Delegates
        private void SetDelegate()
        {
            if (_fromRow == -1)
            {
                _changePropMethod = SetUnknown;
            }
            //Single cell
            else if (_fromRow == _toRow && _fromCol == _toCol && Addresses==null)
            {
                _changePropMethod = SetSingle;
            }
            //Range (ex A1:A2)
            else if (Addresses == null)
            {
                _changePropMethod = SetRange;
            }
            //Multi Range (ex A1:A2,C1:C2)
            else
            {
                _changePropMethod = SetMultiRange;
            }
        }
        /// <summary>
        /// We dont know the address yet. Set the delegate first time a property is set.
        /// </summary>
        /// <param name="valueMethod"></param>
        /// <param name="value"></param>
        private void SetUnknown(_setValue valueMethod, object value)
        {
            //Address is not set use, selected range
            if (_fromRow == -1)
            {
                SetToSelectedRange();
            }
            SetDelegate();
            _changePropMethod(valueMethod, value);
        }
        /// <summary>
        /// Set a single cell
        /// </summary>
        /// <param name="valueMethod"></param>
        /// <param name="value"></param>
        private void SetSingle(_setValue valueMethod, object value)
        {
            valueMethod(value, _fromRow, _fromCol);
        }
        /// <summary>
        /// Set a range
        /// </summary>
        /// <param name="valueMethod"></param>
        /// <param name="value"></param>
        private void SetRange(_setValue valueMethod, object value)
        {
            SetValueAddress(this, valueMethod, value);
        }
        /// <summary>
        /// Set a multirange (A1:A2,C1:C2)
        /// </summary>
        /// <param name="valueMethod"></param>
        /// <param name="value"></param>
        private void SetMultiRange(_setValue valueMethod, object value)
        {
            SetValueAddress(this, valueMethod, value);
            foreach (var address in Addresses)
            {
                SetValueAddress(address, valueMethod, value);
            }
        }
        /// <summary>
        /// Set the property for an address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="valueMethod"></param>
        /// <param name="value"></param>
        private static void SetValueAddress(ExcelAddress address, _setValue valueMethod, object value)
        {
            for (int col = address.Start.Column; col <= address.End.Column; col++)
            {
                for (int row = address.Start.Row; row <= address.End.Row; row++)
                {
                    valueMethod(value, row, col);
                }
            }
        }
        #endregion
        #region Set property methods
        private void Set_StyleID(object value, int row, int col)
        {
            _worksheet.Cell(row, col).StyleID = (int)value;
        }
        private void Set_StyleName(object value, int row, int col)
        {
            _worksheet.Cell(row, col).SetNewStyleName(value.ToString(), _styleID);
        }
        private void Set_Value(object value, int row, int col)
        {
            ExcelCell c = _worksheet.Cell(row, col);
            if (c._sharedFormulaID > 0) SplitFormulas();
            _worksheet.Cell(row, col).Value = value;
        }
        private void Set_Formula(object value, int row, int col)
        {
            ExcelCell c = _worksheet.Cell(row, col);
            if (c._sharedFormulaID > 0) SplitFormulas();

            string formula = value.ToString();
            if (string.IsNullOrEmpty(formula))
            {
                c.Formula = string.Empty;
            }
            else
            {
                if (formula[0] == '=') value = formula.Substring(1, formula.Length - 1); // remove any starting equalsign.
                c.Formula = formula;
            }
        }
        /// <summary>
        /// Handles shared formulas
        /// </summary>
        /// <param name="value"></param>
        /// <param name="address"></param>
        private void Set_SharedFormula(string value, ExcelAddress address, bool IsArray)
        {
            if (address.Start.Row == address.End.Row && address.Start.Column == address.End.Column)             //is it really a shared formula?
            {
                //Nope, single cell. Set the formula
                Set_Formula(value, address.Start.Row, address.Start.Column);
                return;
            }
            //RemoveFormuls(address);
            CheckAndSplitSharedFormula();
            ExcelWorksheet.Formulas f = new ExcelWorksheet.Formulas();
            f.Formula = value;
            f.Index = _worksheet.GetMaxShareFunctionIndex(IsArray);
            f.Address = address.FirstAddress;
            f.StartCol = address.Start.Column;
            f.StartRow = address.Start.Row;
            f.IsArray = IsArray;

            _worksheet._sharedFormulas.Add(f.Index, f);
            _worksheet.Cell(address.Start.Row, address.Start.Column).SharedFormulaID = f.Index;
            _worksheet.Cell(address.Start.Row, address.Start.Column).Formula = value;

            for (int col = address.Start.Column; col <= address.End.Column; col++)
            {
                for (int row = address.Start.Row; row <= address.End.Row; row++)
                {
                    _worksheet.Cell(row, col).SharedFormulaID = f.Index;
                }
            }
       }
        /// <summary>
        /// Handles array formulas
        /// </summary>
        /// <param name="value"></param>
        /// <param name="address"></param>
        private void Set_ArrayFormula(string value, ExcelAddress address)
        {
            RemoveFormuls(address);
            ExcelWorksheet.Formulas f = new ExcelWorksheet.Formulas();
            f.Formula = value;
            f.Index = _worksheet.GetMaxShareFunctionIndex(true);
            f.Address = address.FirstAddress;
            f.StartCol = address.Start.Column;
            f.StartRow = address.Start.Row;

            _worksheet._sharedFormulas.Add(f.Index, f);
            _worksheet.Cell(address.Start.Row, address.Start.Column).SharedFormulaID = f.Index;
            _worksheet.Cell(address.Start.Row, address.Start.Column).Formula = value;

            for (int col = address.Start.Column; col <= address.End.Column; col++)
            {
                for (int row = address.Start.Row; row <= address.End.Row; row++)
                {
                    _worksheet.Cell(row, col).SharedFormulaID = f.Index;
                }
            }
        }
        private void Set_HyperLink(object value, int row, int col)
        {
            _worksheet.Cell(row, col).Hyperlink = value as Uri;
        }
        private void Set_IsRichText(object value, int row, int col)
        {
            _worksheet.Cell(row, col).IsRichText = (bool)value;
        }
        private void Exists_Comment(object value, int row, int col)
        {
            if (_worksheet.Cell(row, col).Comment != null)
            {
                throw (new InvalidOperationException(string.Format("Cell {0} already contain a comment.", new ExcelCellAddress(row, col).Address)));
            }
        }
        private void Set_Comment(object value, int row, int col)
        {
            string[] v = (string[])value;
            var comment = Worksheet.Comments.Add(new ExcelRangeBase(_worksheet, GetAddress(_fromRow, _fromCol)), v[0], v[1]);
            _worksheet.Cell(row, col).Comment = comment;
        }
        #endregion
        private void SetToSelectedRange()
        {
            if (_worksheet.View.SelectedRange == "")
            {
                Address = "A1";
            }
            else
            {
                Address = _worksheet.View.SelectedRange;
            }
        }
        #region "Public Properties"
        /// <summary>
        /// The styleobject for the range.
        /// </summary>
        public ExcelStyle Style
        {
            get
            {
                if (_fromRow == -1)
                {
                    SetToSelectedRange();
                }
                return _worksheet.Workbook.Styles.GetStyleObject(_worksheet.Cell(_fromRow, _fromCol).StyleID, _worksheet.PositionID, _address);
            }
        }
        /// <summary>
        /// The named style
        /// </summary>
        public string StyleName
        {
            get
            {
                return _worksheet.Cell(_fromRow, _fromCol).StyleName;
            }
            set
            {
                _styleID = _worksheet.Workbook.Styles.GetStyleIdFromName(value);
                _changePropMethod(Set_StyleName, value);
            }
        }
        /// <summary>
        /// The style ID. 
        /// It is not recomended to use this one. Use Named styles as an alternative.
        /// If you do, make sure that you use the Style.UpdateXml() method to update any new styles added to the workbook.
        /// </summary>
        public int StyleID
        {
            get
            {
                return _worksheet.Cell(_fromRow, _fromCol).StyleID;
            }
            set
            {
                _changePropMethod(Set_StyleID, value);
            }
        }
        /// <summary>
        /// Set the range to a specific value
        /// </summary>
        public object Value
        {
            get
            {
                return _worksheet.Cell(_fromRow, _fromCol).Value;
            }
            set
            {
                _changePropMethod(Set_Value, value);
            }
        }
        /// <summary>
        /// Gets or sets a formula for a range.
        /// </summary>
        public string Formula
        {
            get
            {
                return _worksheet.Cell(_fromRow, _fromCol).Formula;
            }
            set
            {
                if (_fromRow == _toRow && _fromCol == _toCol)
                {
                    Set_Formula(value, _fromRow, _fromCol);
                }
                else
                {
                    Set_SharedFormula(value, this, false);
                    if (Addresses != null)
                    {
                        foreach (var address in Addresses)
                        {
                            Set_SharedFormula(value, address, false);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Gets or Set a formula in R1C1 format.
        /// </summary>
        public string FormulaR1C1
        {
            get
            {
                return _worksheet.Cell(_fromRow, _fromCol).FormulaR1C1;
            }
            set
            {
                if (value.Length > 0 && value[0] == '=') value = value.Substring(1, value.Length - 1); // remove any starting equalsign.

                if (Addresses == null)
                {
                    Set_SharedFormula(ExcelCell.TranslateFromR1C1(value, _fromRow, _fromCol), this,false);
                }
                else
                {
                    Set_SharedFormula(ExcelCell.TranslateFromR1C1(value, _fromRow, _fromCol), new ExcelAddress(FirstAddress), false);
                    foreach (var address in Addresses)
                    {
                        Set_SharedFormula(ExcelCell.TranslateFromR1C1(value, address.Start.Row, address.Start.Column), address, false);
                    }
                }
            }
        }
        /// <summary>
        /// Set the hyperlink property for a range of cells
        /// </summary>
        public Uri Hyperlink
        {
            get
            {
                return _worksheet.Cell(_fromRow, _fromCol).Hyperlink;
            }
            set
            {
                _changePropMethod(Set_HyperLink, value);
            }
        }
        /// <summary>
        /// If the cells in the range are merged.
        /// </summary>
        public bool Merge
        {
            get
            {
                for (int col = _fromCol; col <= _toCol; col++)
                {
                    for (int row = _fromRow; row <= _toRow; row++)
                    {
                        if (!_worksheet.Cell(row, col).Merge)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            set
            {
                SetMerge(value, FirstAddress);
                if (Addresses != null)
                {
                    foreach (var address in Addresses)
                    {
                        SetMerge(value, address._address);
                    }
                }
            }
        }

        private void SetMerge(bool value, string address)
        {
            if (!value)
            {
                if (_worksheet.MergedCells.List.Contains(address))
                {
                    SetCellMerge(false,address);
                    _worksheet.MergedCells.List.Remove(address);
                }
                else if (!CheckMergeDiff(false, address))
                {
                    throw (new Exception("Range is not fully merged.Specify the exact range"));
                }
            }
            else
            {
                if (CheckMergeDiff(false, address))
                {
                    SetCellMerge(true, address);
                    _worksheet.MergedCells.List.Add(address);
                }
                else
                {
                    if (!_worksheet.MergedCells.List.Contains(address))
                    {
                        throw (new Exception("Cells are already merged"));
                    }
                }
            }
        }
        /// <summary>
        /// Set an autofilter for the range
        /// </summary>
        public bool AutoFilter
        {
            get
            {
                ExcelAddressBase address = _worksheet.AutoFilterAddress;
                if (_fromRow >= address.Start.Row
                    &&
                    _toRow <= address.End.Row
                    &&
                    _fromCol >= address.Start.Column
                    &&
                    _toCol <= address.End.Column)
                {
                    return true;
                }
                return false;
            }
            set
            {
                _worksheet.AutoFilterAddress = this;
                if (_worksheet.Names.ContainsKey("_xlnm._FilterDatabase"))
                {
                    _worksheet.Names.Remove("_xlnm._FilterDatabase");
                }
                var result = _worksheet.Names.Add("_xlnm._FilterDatabase", this);
                result.IsNameHidden = true;
            }
        }        
        /// <summary>
        /// If the value is in richtext format.
        /// </summary>
        public bool IsRichText
        {
            get
            {
                return _worksheet.Cell(_fromRow, _fromCol).IsRichText;
            }
            set
            {
                _changePropMethod(Set_IsRichText, value);
            }
        }
        /// <summary>
        /// Is the range a part of an Arrayformula
        /// </summary>
        public bool IsArrayFormula
        {
            get
            {
                return _worksheet.Cell(_fromRow, _fromCol).IsArrayFormula;
            }
        }
        ExcelRichTextCollection _rtc = null;
        /// <summary>
        /// Cell value is richtext formated. 
        /// </summary>
        public ExcelRichTextCollection RichText
        {
            get
            {
                if (_rtc == null)
                {
                    XmlDocument xml = new XmlDocument();
                    if (_worksheet.Cell(_fromRow, _fromCol).Value != null)
                    {
                        xml.LoadXml("<d:si xmlns:d=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" ><d:r><d:t>" + _worksheet.Cell(_fromRow, _fromCol).Value.ToString() + "</d:t></d:r></d:si>");
                    }
                    else
                    {
                        xml.LoadXml("<d:si xmlns:d=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" />");
                    }
                    IsRichText = true;
                    _rtc = new ExcelRichTextCollection(_worksheet.NameSpaceManager, xml.SelectSingleNode("d:si", _worksheet.NameSpaceManager), this);
                    if (_rtc.Count == 1)
                    {
                        var fnt = _worksheet.Cell(_fromRow, _fromCol).Style.Font;
                        _rtc[0].PreserveSpace = true;
                        _rtc[0].Bold = fnt.Bold;
                        _rtc[0].FontName = fnt.Name;
                        _rtc[0].Italic = fnt.Italic;
                        _rtc[0].Size = fnt.Size;
                        _rtc[0].UnderLine = fnt.UnderLine;
                        
                        int hex;
                        if (fnt.Color.Rgb != "" && int.TryParse(fnt.Color.Rgb, NumberStyles.HexNumber,null, out hex))
                        {
                            _rtc[0].Color = Color.FromArgb(hex);
                        }

                    }
                }
                return _rtc;
            }
        }
        /// <summary>
        /// returns the comment object of the first cell in the range
        /// </summary>
        public ExcelComment Comment
        {
            get
            {
                ulong cellID= GetCellID(_worksheet.SheetID, _fromRow, _fromCol);
                if(_worksheet.Comments._comments.ContainsKey(cellID))
                {
                    return _worksheet._comments._comments[cellID] as ExcelComment;
                }
                return null;
            }
        }
        /// <summary>
        /// WorkSheet object 
        /// </summary>
        public ExcelWorksheet Worksheet
        {
            get
            {
                return _worksheet;
            }
        }
        /// <summary>
        /// Address including sheetname
        /// </summary>
        public string FullAddress
        {
            get
            {
                string fullAddress=GetFullAddress(_worksheet.Name, _address);
                if (Addresses != null)
                {
                    foreach (var a in Addresses)
                    {
                        fullAddress += "," + GetFullAddress(_worksheet.Name, a.Address); ;
                    }
                }
                return fullAddress;
            }
        }
        /// <summary>
        /// Address including sheetname
        /// </summary>
        public string FullAddressAbsolute
        {
            get
            {
                string fullAddress = GetFullAddress(_worksheet.Name, GetAddress(_fromRow, _fromCol, _toRow, _toCol, true));
                if (Addresses != null)
                {
                    foreach (var a in Addresses)
                    {
                        fullAddress += "," + GetFullAddress(_worksheet.Name, GetAddress(a.Start.Row, a.Start.Column, a.End.Row, a.End.Column, true)); ;
                    }
                }
                return fullAddress;
            }
        }

        #endregion
        #region "Private Methods"
        /// <summary>
        /// Check if the range is partly merged
        /// </summary>
        /// <param name="startValue">the starting value</param>
        /// <param name="address">the address</param>
        /// <returns></returns>
        private bool CheckMergeDiff(bool startValue, string address)
        {
            ExcelAddress a = new ExcelAddress(address);
            for (int col = a._fromCol; col <= a._toCol; col++)
            {
                for (int row = a._fromRow; row <= a._toRow; row++)
                {
                    if (_worksheet.Cell(row, col).Merge != startValue)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// Set the merge flag for the range
        /// </summary>
        /// <param name="value"></param>
        internal void SetCellMerge(bool value, string address)
        {
            ExcelAddress a = new ExcelAddress(address);
            for (int col = a._fromCol; col <= a._toCol; col++)
            {
                for (int row = a._fromRow; row <= a._toRow; row++)
                {
                    _worksheet.Cell(row, col).Merge = value;
                }
            }
        }
        /// <summary>
        /// Set the value without altering the richtext property
        /// </summary>
        /// <param name="value">the value</param>
        internal void SetValueRichText(object value)
        {
            for (int col = _fromCol; col <= _toCol; col++)
            {
                for (int row = _fromRow; row <= _toRow; row++)
                {
                    _worksheet.Cell(row, col).SetValueRichText(value);
                }
            }
        }
        /// <summary>
        /// Removes a shared formula
        /// </summary>
        private void RemoveFormuls(ExcelAddress address)
        {
            List<int> removed = new List<int>();
            int fFromRow, fFromCol, fToRow, fToCol;
            foreach (int index in _worksheet._sharedFormulas.Keys)
            {
                ExcelWorksheet.Formulas f = _worksheet._sharedFormulas[index];
                ExcelCell.GetRowColFromAddress(f.Address, out fFromRow, out fFromCol, out fToRow, out fToCol);
                if (((fFromCol >= address.Start.Column && fFromCol <= address.End.Column) ||
                   (fToCol >= address.Start.Column && fToCol <= address.End.Column)) &&
                   ((fFromRow >= address.Start.Row && fFromRow <= address.End.Row) ||
                   (fToRow >= address.Start.Row && fToRow <= address.End.Row)))
                {
                    for (int col = fFromCol; col <= fToCol; col++)
                    {
                        for (int row = fFromRow; row <= fToRow; row++)
                        {
                            _worksheet.Cell(row, col).SharedFormulaID = int.MinValue;
                        }
                    }
                    removed.Add(index);
                }
            }
            foreach (int index in removed)
            {
                _worksheet._sharedFormulas.Remove(index);
            }
        }
        internal void SetSharedFormulaID(int id)
        {
                for (int col = _fromCol; col <= _toCol; col++)
                {
                    for (int row = _fromRow; row <= _toRow; row++)
                    {
                        _worksheet.Cell(row, col).SharedFormulaID = id;
                    }
                }
        }
        private void CheckAndSplitSharedFormula()
        {            
            for (int col = _fromCol; col <= _toCol; col++)
            {
                for (int row = _fromRow; row <= _toRow; row++)
                {
                    if (_worksheet.Cell(row, col).SharedFormulaID >= 0)
                    {
                        SplitFormulas();
                        return;
                    }
                }
            }
        }

        private void SplitFormulas()
        {
            List<int> formulas=new List<int>();
            for (int col = _fromCol; col <= _toCol; col++)
            {
                for (int row = _fromRow; row <= _toRow; row++)
                {
                    int id = _worksheet.Cell(row, col).SharedFormulaID;
                    if (id >= 0 && !formulas.Contains(id))
                    {
                        if (_worksheet._sharedFormulas[id].IsArray && 
                            Collide(_worksheet.Cells[_worksheet._sharedFormulas[id].Address])==eAddressCollition.Partly) // If the formula is an array formula and its on inside the overwriting range throw an exception
                        {
                            throw (new Exception("Can not overwrite a part of an array-formula"));
                        }
                        formulas.Add(_worksheet.Cell(row, col).SharedFormulaID);
                    }
                }
            }

            foreach (int ix in formulas)
            {
                SplitFormula(ix);
            }
        }

        private void SplitFormula(int ix)
        {
            var f = _worksheet._sharedFormulas[ix];
            var fRange = _worksheet.Cells[f.Address];
            var collide = Collide(fRange);
            
            //The formula is inside the currenct range, remove it
            if (collide == eAddressCollition.Inside)
            {
                _worksheet._sharedFormulas.Remove(ix);
                fRange.SetSharedFormulaID(int.MinValue);
            }
            else if (collide == eAddressCollition.Partly)
            {
                //The formula partly collides with the current range
                bool fIsSet = false;
                string formulaR1C1 = fRange.FormulaR1C1;
                //Top Range
                if (fRange._fromRow < _fromRow)
                {
                    f.Address = ExcelCell.GetAddress(fRange._fromRow, fRange._fromCol, _fromRow - 1, fRange._toCol);
                    fIsSet=true;
                }
                //Left Range
                if (fRange._fromCol < _fromCol)
                {
                    if (fIsSet)
                    {
                        f = new ExcelWorksheet.Formulas();
                        f.Index = _worksheet.GetMaxShareFunctionIndex(false);
                        f.StartCol = fRange._fromCol;
                        f.IsArray = false;
                        _worksheet._sharedFormulas.Add(f.Index, f);
                    }
                    else
                    {
                        fIsSet = true;                        
                    }
                    if (fRange._fromRow < _fromRow)
                        f.StartRow = _fromRow;
                    else
                    {
                        f.StartRow = fRange._fromRow;
                    }
                    if (fRange._toRow < _toRow)
                    {
                        f.Address = ExcelCell.GetAddress(f.StartRow, f.StartCol,
                            fRange._toRow, _fromCol - 1);
                    }
                    else
                    {
                        f.Address = ExcelCell.GetAddress(f.StartRow, f.StartCol,
                           _toRow, _fromCol - 1);                    
                    }
                    f.Formula = TranslateFromR1C1(formulaR1C1, f.StartRow, f.StartCol);
                    _worksheet.Cells[f.Address].SetSharedFormulaID(f.Index);
                }
                //Right Range
                if (fRange._toCol > _toCol)
                {
                    if (fIsSet)
                    {
                        f = new ExcelWorksheet.Formulas();
                        f.Index = _worksheet.GetMaxShareFunctionIndex(false);
                        f.IsArray = false;
                        _worksheet._sharedFormulas.Add(f.Index, f);
                    }
                    else
                    {
                        fIsSet = true;
                    }
                    f.StartCol = _toCol+1;
                    if (_fromRow < fRange._fromRow)
                        f.StartRow = fRange._fromRow;
                    else
                    {
                        f.StartRow = _fromRow;
                    }

                    if (fRange._toRow < _toRow)
                    {
                        f.Address = ExcelCell.GetAddress(f.StartRow, f.StartCol,
                            fRange._toRow, fRange._toCol);
                    }
                    else
                    {
                        f.Address = ExcelCell.GetAddress(f.StartRow, f.StartCol,
                            _toRow, fRange._toCol);
                    }
                        f.Formula = TranslateFromR1C1(formulaR1C1, f.StartRow, f.StartCol);
                    _worksheet.Cells[f.Address].SetSharedFormulaID(f.Index);
                }
                //Bottom Range
                if (fRange._toRow > _toRow)
                {
                    if (fIsSet)
                    {
                        f = new ExcelWorksheet.Formulas();
                        f.Index = _worksheet.GetMaxShareFunctionIndex(false);
                        f.IsArray = false;
                        _worksheet._sharedFormulas.Add(f.Index, f);
                    }

                    f.StartCol = fRange._fromCol;
                    f.StartRow = _toRow + 1;

                    f.Formula = TranslateFromR1C1(formulaR1C1, f.StartRow, f.StartCol);

                    f.Address = ExcelCell.GetAddress(f.StartRow, f.StartCol,
                        fRange._toRow, fRange._toCol);
                    _worksheet.Cells[f.Address].SetSharedFormulaID(f.Index);

                }
            }
        }
        #endregion
        #region "Public Methods"
        #region "LoadFromDataTable"
        /// <summary>
        /// Load the data from the datatable starting from the top left cell of the range
        /// </summary>
        /// <param name="Table">The datatable to load</param>
        /// <param name="PrintHeaders">Print the column names on first row</param>
        /// <param name="TableStyle">The table style to apply to the data</param>
        public void LoadFromDataTable(DataTable Table, bool PrintHeaders, TableStyles TableStyle)
        {
            LoadFromDataTable(Table, PrintHeaders);

            int rows = Table.Rows.Count + (PrintHeaders ? 1 : 0)-1;
            if (rows > 0 && Table.Columns.Count>0)
            {
                var tbl = _worksheet.Tables.Add(new ExcelAddressBase(_fromRow, _fromCol, _fromRow + rows, _fromCol + Table.Columns.Count-1), Table.TableName);
                tbl.ShowHeader = PrintHeaders;
                tbl.TableStyle = TableStyle;
            }
        }
        /// <summary>
        /// Load the data from the datatable starting from the top left cell of the range
        /// </summary>
        /// <param name="Table">The datatable to load</param>
        /// <param name="PrintHeaders">Print the column names on first row</param>
        public void LoadFromDataTable(DataTable Table, bool PrintHeaders)
        {
            if (Table == null)
            {
                throw (new ArgumentNullException("Table can't be null"));
            }

            int col = _fromCol, row = _fromRow;
            if (PrintHeaders)
            {
                foreach (DataColumn dc in Table.Columns)
                {
                    _worksheet.Cell(row, col++).Value = dc.ColumnName;
                }
                row++;
                col = _fromCol;
            }
            foreach (DataRow dr in Table.Rows)
            {
                foreach (object value in dr.ItemArray)
                {
                    _worksheet.Cell(row, col++).Value = value;
                }
                row++;
                col = _fromCol;
            }
        }
        #endregion

        /// <summary>
		/// Loads data from the collection of arrays of objects into the range, starting from
		/// the top-left cell.
		/// </summary>
		/// <param name="Data">The data.</param>
		public ExcelRangeBase LoadFromArrays(IEnumerable<object[]> Data)
		{
			//thanx to Abdullin for the code contibution
            if (Data == null) throw new ArgumentNullException("data");

            int column = _fromCol, row = _fromRow;

			foreach (var rowData in Data)
			{
                column = _fromCol;
                foreach (var cellData in rowData)
				{
					_worksheet.Cell(row, column).Value = cellData;
					column += 1;
				}
				row += 1;
			}
            return _worksheet.Cells[_fromRow,_fromCol, row-1, column-1];
		}
        #region LoadFromText
        /// <summary>
        /// Loads a CSV text into a range starting from the top left cell.
        /// Default settings is Comma separation
        /// </summary>
        /// <param name="Text">The Text</param>
        /// <returns>The range containg the data</returns>
        public ExcelRangeBase LoadFromText(string Text)
        {
            return LoadFromText(Text, new ExcelTextFormat());
        }
        /// <summary>
        /// Loads a CSV text into a range starting from the top left cell.
        /// </summary>
        /// <param name="Text">The Text</param>
        /// <param name="Format">Information how to load the text</param>
        /// <returns>The range containg the data</returns>
        public ExcelRangeBase LoadFromText(string Text, ExcelTextFormat Format)
        {
            if (Format == null) Format = new ExcelTextFormat();
            string[] lines = Regex.Split(Text, Format.EOL);
            int row = _fromRow;
            int col = _fromCol;
            int maxCol = col;
            int lineNo = 1;
            if (Text == "")
            {
                _worksheet.Cells[_fromRow, _fromCol].Value = "";
            }
            else
            {
                foreach (string line in lines)
                {
                    if (lineNo > Format.SkipLinesBeginning && lineNo <= lines.Length - Format.SkipLinesEnd)
                    {
                        col = _fromCol;
                        string v = "";
                        bool isText = false, isQualifier = false;
                        int QCount = 0;
                        foreach (char c in line)
                        {
                            if (Format.TextQualifier != 0 && c == Format.TextQualifier)
                            {
                                if (!isText && v != "")
                                {
                                    throw (new Exception(string.Format("Invalid Text Qualifier in line : {0}", line)));
                                }
                                isQualifier = !isQualifier;
                                QCount += 1;
                                isText = true;
                            }
                            else
                            {
                                if (QCount > 1)
                                {
                                    v += new string(Format.TextQualifier, QCount / 2);
                                }
                                if (isQualifier)
                                {
                                    v += c;
                                }
                                else
                                {
                                    if (c == Format.Delimiter)
                                    {
                                        _worksheet.Cell(row, col).Value = ConvertData(Format, v, col - _fromCol, isText);
                                        v = "";
                                        isText = false;
                                        col++;
                                    }
                                    else
                                    {
                                        if (QCount % 2 == 1)
                                        {
                                            throw (new Exception(string.Format("Text delimiter is not closed in line : {0}", line)));
                                        }
                                        v += c;
                                    }
                                }
                                QCount = 0;
                            }
                        }
                        if (QCount > 1)
                        {
                            v += new string(Format.TextQualifier, QCount / 2);
                        }

                        _worksheet.Cell(row, col).Value = ConvertData(Format, v, col - _fromCol, isText);
                        if (col > maxCol) maxCol = col;
                        row++;
                    }
                    lineNo++;
                }
            }
            return _worksheet.Cells[_fromRow, _fromCol, row - 1, maxCol];
        }
        /// <summary>
        /// Loads a CSV text into a range starting from the top left cell.
        /// </summary>
        /// <param name="Text">The Text</param>
        /// <param name="Format">Information how to load the text</param>
        /// <param name="TableStyle">Create a table with this style</param>
        /// <param name="FirstRowIsHeader">Use the first row as header</param>
        /// <returns></returns>
        public ExcelRangeBase LoadFromText(string Text, ExcelTextFormat Format, TableStyles TableStyle, bool FirstRowIsHeader)
        {
            var r = LoadFromText(Text, Format);

            var tbl = _worksheet.Tables.Add(r,"");
            tbl.ShowHeader = FirstRowIsHeader;
            tbl.TableStyle = TableStyle;

            return r;
        }
        /// <summary>
        /// Loads a CSV file into a range starting from the top left cell.
        /// </summary>
        /// <param name="Text">The Text</param>
        /// <returns></returns>
        public ExcelRangeBase LoadFromText(FileInfo TextFile)
        {
            return LoadFromText(File.ReadAllText(TextFile.FullName, Encoding.ASCII));
        }
        /// <summary>
        /// Loads a CSV file into a range starting from the top left cell.
        /// </summary>
        /// <param name="TextFile">The Text</param>
        /// <param name="Format">Information how to load the text</param>
        /// <returns></returns>
        public ExcelRangeBase LoadFromText(FileInfo TextFile, ExcelTextFormat Format)
        {
            return LoadFromText(File.ReadAllText(TextFile.FullName, Format.Encoding), Format);
        }
        /// <summary>
        /// Loads a CSV file into a range starting from the top left cell.
        /// </summary>
        /// <param name="Text">The Text</param>
        /// <param name="Format">Information how to load the text</param>
        /// <param name="TableStyle">Create a table with this style</param>
        /// <param name="FirstRowIsHeader">Use the first row as header</param>
        /// <returns></returns>
        public ExcelRangeBase LoadFromText(FileInfo TextFile, ExcelTextFormat Format, TableStyles TableStyle, bool FirstRowIsHeader)
        {
            return LoadFromText(File.ReadAllText(TextFile.FullName, Format.Encoding), Format, TableStyle, FirstRowIsHeader);
        }
        #endregion
        private object ConvertData(ExcelTextFormat Format, string v, int col, bool isText)
        {
            if (isText && (Format.DataTypes == null || Format.DataTypes.Length < col)) return v;

            double d;
            DateTime dt;
            if (Format.DataTypes == null || Format.DataTypes.Length < col || Format.DataTypes[col] == eDataTypes.Unknown)
            {
                string v2 = v.EndsWith("%") ? v.Substring(0, v.Length - 1) : v;
                if (double.TryParse(v2, NumberStyles.Any, Format.Culture, out d))
                {
                    if (v2 == v)
                    {
                        return d;
                    }
                    else
                    {
                        return d / 100;
                    }
                }
                if (DateTime.TryParse(v, Format.Culture, DateTimeStyles.None, out dt))
                {
                    return dt;
                }
                else
                {
                    return v;
                }
            }
            else
            {
                switch (Format.DataTypes[col])
                {
                    case eDataTypes.Number:
                        if (double.TryParse(v, NumberStyles.Any, Format.Culture, out d))
                        {
                            return d;
                        }
                        else
                        {
                            return v;
                        }
                    case eDataTypes.DateTime:
                        if (DateTime.TryParse(v, Format.Culture, DateTimeStyles.None, out dt))
                        {
                            return dt;
                        }
                        else
                        {
                            return v;
                        }
                    case eDataTypes.Percent:
                        string v2 = v.EndsWith("%") ? v.Substring(0, v.Length - 1) : v;
                        if (double.TryParse(v2, NumberStyles.Any, Format.Culture, out d))
                        {
                            return d / 100;
                        }
                        else
                        {
                            return v;
                        }

                    default:
                        return v;

                }
            }
        }
        /// <summary>
        /// Get a range with an offset from the top left cell.
        /// The new range has the same dimensions as the current range
        /// </summary>
        /// <param name="RowOffset">Row Offset</param>
        /// <param name="ColumnOffset">Column Offset</param>
        /// <returns></returns>
        public ExcelRangeBase Offset(int RowOffset, int ColumnOffset)
        {            
            if(_fromRow+RowOffset<1 || _fromCol+ColumnOffset<1 || _fromRow+RowOffset>ExcelPackage.MaxRows || _fromCol+ColumnOffset>ExcelPackage.MaxColumns)
            {
                throw(new ArgumentOutOfRangeException("Offset value out of range"));
            }
            string address = GetAddress(_fromRow+RowOffset, _fromCol+ColumnOffset, _toRow+RowOffset, _toCol+ColumnOffset);
            return new ExcelRangeBase(_worksheet, address);
        }
        /// <summary>
        /// Get a range with an offset from the top left cell.
        /// </summary>
        /// <param name="RowOffset">Row Offset</param>
        /// <param name="ColumnOffset">Column Offset</param>
        /// <param name="NumberOfRows">Number of rows. Minimum 1</param>
        /// <param name="NumberOfColumns">Number of colums. Minimum 1</param>
        /// <returns></returns>
        public ExcelRangeBase Offset(int RowOffset, int ColumnOffset, int NumberOfRows, int NumberOfColumns)
        {
            if (NumberOfRows < 1 || NumberOfColumns < 1)
            {
                throw (new Exception("Number of rows/columns must be greater than 0"));
            }
            NumberOfRows--;
            NumberOfColumns--;
            if(_fromRow+RowOffset<1 || _fromCol+ColumnOffset<1 || _fromRow+RowOffset>ExcelPackage.MaxRows || _fromCol+ColumnOffset>ExcelPackage.MaxColumns ||
               _fromRow+RowOffset+NumberOfRows<1 || _fromCol+ColumnOffset+NumberOfColumns<1 || _fromRow+RowOffset+NumberOfRows>ExcelPackage.MaxRows || _fromCol+ColumnOffset+NumberOfColumns>ExcelPackage.MaxColumns )
            {
                throw(new ArgumentOutOfRangeException("Offset value out of range"));
            }
            string address = GetAddress(_fromRow+RowOffset, _fromCol+ColumnOffset, _fromRow+RowOffset+NumberOfRows, _fromCol+ColumnOffset+NumberOfColumns);
            return new ExcelRangeBase(_worksheet, address);
        }
        /// <summary>
        /// Adds a new comment for the range.
        /// If this range contains more than one cell, the top left comment is returned by the method.
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="Author"></param>
        /// <returns>A reference comment of the top left cell</returns>
        public ExcelComment AddComment(string Text, string Author)
        {
            //Check if any comments exists in the range and throw an exception
            _changePropMethod(Exists_Comment, null);
            //Create the comments
            _changePropMethod(Set_Comment, new string[] {Text, Author});

            return  _worksheet.Cell(_fromRow, _fromCol).Comment;
        }

        /// <summary>
        /// Copies the range of cells to an other range
        /// </summary>
        /// <param name="Destination">The start cell where the range will be copied.</param>
        public void Copy(ExcelRangeBase Destination)
        {
            bool sameWorkbook=Destination._worksheet == _worksheet;
            ExcelStyles sourceStyles=_worksheet.Workbook.Styles,
                        styles = Destination._worksheet.Workbook.Styles;
            Dictionary<int,int> styleCashe=new Dictionary<int,int>();

            //Delete all existing cells;            
            List<ExcelCell> newCells=new List<ExcelCell>();
            foreach (var cell in this)
            {
                //Clone the cell
                var newCell=(_worksheet._cells[GetCellID(_worksheet.SheetID, cell._fromRow, cell._fromCol)] as ExcelCell).Clone(Destination._worksheet);

                //Set the correct row/column
                newCell.Row = Destination._fromRow + (newCell.Row - _fromRow);
                newCell.Column = Destination._fromCol + (newCell.Column - _fromCol);

                //If the formula is shared, remove the shared ID and set the formula for the cell.
                if (newCell._sharedFormulaID >= 0)
                {
                    newCell._sharedFormulaID = int.MinValue;
                    newCell.Formula = cell.Formula;
                }
                
                //If its not the same workbook whe must copy the styles to the new workbook.
                if (!sameWorkbook)
                {
                    if (styleCashe.ContainsKey(cell.StyleID))
                    {
                        newCell.StyleID = styleCashe[cell.StyleID];
                    }
                    else
                    {
                        newCell.StyleID = styles.CloneStyle(sourceStyles, cell.StyleID);
                        styleCashe.Add(cell.StyleID, newCell.StyleID);
                    }
                }
                newCells.Add(newCell);
            }

            //Now clear the workbook.
            Delete(Destination.Offset(0,0,(_toRow-_fromRow)+1, (_toCol-_fromCol)+1));

            //And last add the new cells to the worksheet
            foreach (var cell in newCells)
            {
                Destination.Worksheet._cells.Add(cell);
            }
        }
        /// <summary>
        /// Clear all cells
        /// </summary>
        public void Clear()
        {
            Delete(this);
        }
        /// <summary>
        /// Creates an array-formula.
        /// </summary>
        /// <param name="ArrayFormula">The formula</param>
        public void CreateArrayFormula(string ArrayFormula)
        {
            if (Addresses != null)
            {
                throw (new Exception("An Arrayformula can not have more than one address"));
            }
            Set_SharedFormula(ArrayFormula, this,true);
        }
        private void Delete(ExcelAddressBase Range)
        {
            //First find the start cell
            ulong startID=GetCellID(_worksheet.SheetID, Range._fromRow, Range._fromCol);
            int index = _worksheet._cells.IndexOf(startID);
            if (index < 0)
            {
                index = ~index;
            }
            ExcelCell cell;
            //int row=cell.Row, col=cell.Column;
            //Remove all cells in the range
            while (index < _worksheet._cells.Count)
            {
                cell = _worksheet._cells[index] as ExcelCell;
                if (cell.Row > Range._toRow || cell.Row == Range._toRow && cell.Column > Range._toCol)
                {
                    break;
                }
                else
                {
                    if (cell.Column >= Range._fromCol && cell.Column <= Range._toCol)
                    {
                        _worksheet._cells.Delete(cell.CellID);
                    }
                    else
                    {
                        index++;
                    }
                }
            }

            //Delete multi addresses as well
            if (Addresses != null)
            {
                foreach (var sub in Addresses)
                {
                    Delete(sub);
                }
            }
        }
        #endregion
        #region IDisposable Members

        public void Dispose()
        {
            //_worksheet = null;
        }

        #endregion
        #region "Enumerator"
        int _index;
        ulong _toCellId;
        int _enumAddressIx;
        public IEnumerator<ExcelRangeBase> GetEnumerator()
        {
            Reset();
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Reset();
            return this;
        }

        public ExcelRangeBase Current
        {
            get 
            {
                return new ExcelRangeBase(_worksheet, (_worksheet._cells[_index] as ExcelCell).CellAddress);
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return ((object)(new ExcelRangeBase(_worksheet, (_worksheet._cells[_index] as ExcelCell).CellAddress)));
            }
        }

        public bool MoveNext()
        {
            _index++;
            if (_enumAddressIx == -1)
            {
                GetNextIndexEnum(_fromRow, _fromCol, _toRow, _toCol);
                
                if (_index >= _worksheet._cells.Count || _worksheet._cells[_index].RangeID > _toCellId)
                {
                    if (Addresses == null)
                    {
                        return false;
                    }
                    else
                    {
                        _enumAddressIx++;
                        GetStartIndexEnum(Addresses[0].Start.Row, Addresses[0].Start.Column, Addresses[0].End.Row, Addresses[0].End.Column);
                        return MoveNext();
                    }
                }
                
            }
            else
            {
                GetNextIndexEnum(Addresses[_enumAddressIx].Start.Row, Addresses[_enumAddressIx].Start.Column, Addresses[_enumAddressIx].End.Row, Addresses[_enumAddressIx].End.Column);
                if (_index >= _worksheet._cells.Count || _worksheet._cells[_index].RangeID > _toCellId)
                {
                    if (++_enumAddressIx < Addresses.Count)
                    {
                        GetStartIndexEnum(Addresses[_enumAddressIx].Start.Row, Addresses[_enumAddressIx].Start.Column, Addresses[_enumAddressIx].End.Row, Addresses[_enumAddressIx].End.Column);
                        MoveNext();
                    }
                    else
                    {
                        return false;
                    }
                }                
            }
            return true;
        }

        public void Reset()
        {
            _enumAddressIx = -1;
            GetStartIndexEnum(_fromRow, _fromCol, _toRow, _toCol);
        }

        private void GetNextIndexEnum(int fromRow, int fromCol, int toRow, int toCol)
        {
            if (_index >= _worksheet._cells.Count) return;
            ExcelCell cell = _worksheet._cells[_index] as ExcelCell;
            while (cell.Column > toCol || cell.Column < fromCol)
            {
                if (cell.Column < fromCol)
                {
                    _index = _worksheet._cells.IndexOf(ExcelAddress.GetCellID(_worksheet.SheetID, cell.Row, fromCol));
                }
                else
                {
                    _index = _worksheet._cells.IndexOf(ExcelAddress.GetCellID(_worksheet.SheetID, cell.Row + 1, fromCol));
                }

                if (_index < 0)
                {
                    _index = ~_index;
                }
                if (_index >= _worksheet._cells.Count || _worksheet._cells[_index].RangeID > _toCellId)
                {
                    break;
                }
                cell = _worksheet._cells[_index] as ExcelCell;
            }
        }

        private void GetStartIndexEnum(int fromRow, int fromCol, int toRow, int toCol)
        {
            _index = _worksheet._cells.IndexOf(ExcelCellBase.GetCellID(_worksheet.SheetID, fromRow, fromCol));
            _toCellId = ExcelCellBase.GetCellID(_worksheet.SheetID, toRow, toCol);
            if (_index < 0)
            {
                _index = ~_index;
            }
            _index--;
        }
    }
        #endregion
}

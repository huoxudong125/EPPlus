﻿/* 
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
 * Jan Källman		                Initial Release		        2009-10-01
 *******************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
namespace OfficeOpenXml.Style.XmlAccess
{
    public class ExcelXfs : StyleXmlHelper
    {
        ExcelStyles _styles;
        internal ExcelXfs(XmlNamespaceManager nameSpaceManager, ExcelStyles styles) : base(nameSpaceManager)
        {
            _styles = styles;
            isBuildIn = false;
        }
        internal ExcelXfs(XmlNamespaceManager nsm, XmlNode topNode, ExcelStyles styles) :
            base(nsm, topNode)
        {
            _styles = styles;
            _xfID = GetXmlNodeInt("@xfid");
            if (_xfID == 0) isBuildIn = true; //Normal taggen
            _numFmtId = GetXmlNodeInt("@numFmtId");
            _fontId = GetXmlNodeInt("@fontId");
            _fillId = GetXmlNodeInt("@fillId");
            _borderId = GetXmlNodeInt("@borderId");
            _readingOrder = GetXmlNode(readingOrderPath) == "1" ? true : false;
            _verticalAlignment = GetVerticalAlign(GetXmlNode(verticalAlignPath));
            _horizontalAlignment = GetHorizontalAlign(GetXmlNode(horizontalAlignPath));
            _wrapText = GetXmlNode(wrapTextPath) == "1" ? true : false;
            //ApplyFont = GetXmlNode("@applyFont") == "1" ? true : false;
            //ApplyFill = GetXmlNode("@applyFill") == "1" ? true : false;
            //ApplyBorder = GetXmlNode("@applyBorder") == "1" ? true : false;
            //ApplyNumberFormat = GetXmlNode("@applyNumberFormat") == "1" ? true : false;
            //ApplyFill = GetXmlNode("@applyFont") == "1" ? true : false;
            //ApplyProtection = GetXmlNode("@applyProtection") == "1" ? true : false;
            //ApplyAlignment = GetXmlNode("@applyAlitgnment") == "1" ? true : false;
        }

        private ExcelHorizontalAlignment GetHorizontalAlign(string align)
        {
            if (align == "") return ExcelHorizontalAlignment.Left;
            align = align.Substring(0, 1).ToUpper() + align.Substring(1, align.Length - 1);
            try
            {
                return (ExcelHorizontalAlignment)Enum.Parse(typeof(ExcelHorizontalAlignment), align);
            }
            catch
            {
                return ExcelHorizontalAlignment.Left;
            }
        }

        private ExcelVerticalAlignment GetVerticalAlign(string align)
        {
            if (align == "") return ExcelVerticalAlignment.Bottom;
            align = align.Substring(0, 1).ToUpper() + align.Substring(1, align.Length - 1);
            try
            {
                return (ExcelVerticalAlignment)Enum.Parse(typeof(ExcelVerticalAlignment), align);
            }
            catch
            {
                return ExcelVerticalAlignment.Bottom;
            }
        }
        internal void Xf_ChangedEvent(object sender, EventArgs e)
        {
            //if (_cell != null)
            //{
            //    if (!Styles.ChangedCells.ContainsKey(_cell.Id))
            //    {
            //        //_cell.Style = "";
            //        _cell.SetNewStyleID(int.MinValue.ToString());
            //        Styles.ChangedCells.Add(_cell.Id, _cell);
            //    }
            //}
        }
        int _xfID;
        public int XfId
        {
            get
            {
                return _xfID;
            }
            set
            {
                _xfID = value;
            }
        }
        #region Internal Properties
        int _numFmtId;
        internal int NumberFormatId
        {
            get
            {
                return _numFmtId;
            }
            set
            {
                _numFmtId = value;
            }
        }
        int _fontId;
        internal int FontId
        {
            get
            {
                return _fontId;
            }
            set
            {
                _fontId = value;
            }
        }
        int _fillId;
        internal int FillId
        {
            get
            {
                return _fillId;
            }
            set
            {
                _fillId = value;
            }
        }
        int _borderId;
        internal int BorderId
        {
            get
            {
                return _borderId;
            }
            set
            {
                _borderId = value;
            }
        }
        private bool isBuildIn
        {
            get;
            set;
        }
        //internal bool ApplyNumberFormat
        //{
        //    get
        //    {
        //        return GetXmlNode("@applyNumberFormat") == "1" ? true : false;
        //    }
        //    set
        //    {
        //        SetXmlNode("@applyNumberFormat", value ? "1" : "0");
        //    }
        //}
        //internal bool ApplyFont
        //{
        //    get
        //    {
        //        return GetXmlNode("@applyFont") == "1" ? true : false;
        //    }
        //    set
        //    {
        //        SetXmlNode("@applyFont", value ? "1" : "0");
        //    }
        //}
        //internal bool ApplyFill
        //{
        //    get
        //    {
        //        return GetXmlNode("@applyFill") == "1" ? true : false;
        //    }
        //    set
        //    {
        //        SetXmlNode("@applyFill", value ? "1" : "0");
        //    }
        //}
        //internal bool ApplyBorder
        //{
        //    get
        //    {
        //        return GetXmlNode("@applyBorder") == "1" ? true : false;
        //    }
        //    set
        //    {
        //        SetXmlNode("@applyBorder", value ? "1" : "0");
        //    }
        //}
        //internal bool ApplyAlignment
        //{
        //    get
        //    {
        //        return GetXmlNode("@applyAlignment") == "1" ? true : false;
        //    }
        //    set
        //    {
        //        SetXmlNode("@applyAlignment", value ? "1" : "0");
        //    }
        //}
        //internal bool ApplyProtection
        //{
        //    get
        //    {
        //        return GetXmlNode("@applyProtection") == "1" ? true : false;
        //    }
        //    set
        //    {
        //        SetXmlNode("@applyProtection", value ? "1" : "0");
        //    }
        //}
        #endregion
        #region Public Properties
        public ExcelStyles Styles { get; private set; }
        public ExcelNumberFormatXml Numberformat 
        {
            get
            {
                return _styles.NumberFormats[_numFmtId < 0 ? 0 : _numFmtId];
            }
        }
        public ExcelFontXml Font 
        { 
           get
           {
               return _styles.Fonts[_fontId < 0 ? 0 : _fontId];
           }
        }
        public ExcelFillXml Fill
        {
            get
            {
                return _styles.Fills[_fillId < 0 ? 0 : _fillId];
            }
        }
        public ExcelBorderXml Border
        {
            get
            {
                return _styles.Borders[_borderId < 0 ? 0 : _borderId];
            }
        }
        const string horizontalAlignPath = "d:alignment/@horizontal";
        ExcelHorizontalAlignment _horizontalAlignment = ExcelHorizontalAlignment.Left;
        public ExcelHorizontalAlignment HorizontalAlignment
        {
            get
            {
                return _horizontalAlignment;
            }
            set
            {
                _horizontalAlignment = value;
            }
        }
        const string verticalAlignPath = "d:alignment/@vertical";
        ExcelVerticalAlignment _verticalAlignment=ExcelVerticalAlignment.Bottom;
        public ExcelVerticalAlignment VerticalAlignment
        {
            get
            {
                return _verticalAlignment;
            }
            set
            {
                _verticalAlignment = value;
            }
        }
        const string wrapTextPath = "d:alignment/@wrapText";
        bool _wrapText=false;
        public bool WrapText
        {
            get
            {
                return _wrapText;
            }
            set
            {
                _wrapText = value;
            }
        }
        const string readingOrderPath = "d:alignment/@readingOrder";
        bool _readingOrder=false;
        public bool ReadingOrder
        {
            get
            {
                return _readingOrder;
            }
            set
            {
                _readingOrder = value;
            }
        }
        const string shrinkToFitPath = "d:alignment/@shrinkToFit";
        bool _shrinkToFit = false;
        public bool ShrinkToFit
        {
            get
            {
                return _shrinkToFit;
            }
            set
            {
                _shrinkToFit = value;
            }
        }
        #endregion
        internal void SyncIds(ExcelStyleCollection<ExcelNumberFormatXml> numberFormats, ExcelStyleCollection<ExcelFontXml> fonts, ExcelStyleCollection<ExcelFillXml> fills, ExcelStyleCollection<ExcelBorderXml> borders)
        {
            //ExcelNumberFormat nf = null;
            //if (numberFormats.FindByID(NumberFormatId.ToString(), ref nf))
            //{
            //    Numberformat = nf;
            //}
            //else
            //{
            //    //Build in formats. Just set NumFmtId
            //    //Numberformat = new ExcelNumberFormat(NameSpaceManager, null);
            //    Numberformat.NumFmtId = NumberFormatId;
            //}

            //if (fonts.Count > 0)
            //{
            //    Font = fonts[FontId + 1];
            //}
            //else
            //{
            //    Font = new ExcelFont(NameSpaceManager, null);
            //}

            //if (fills.Count > FillId)
            //{
            //    Fill = fills[FillId + 1];
            //}
            //else
            //{
            //    Fill = new ExcelFill(NameSpaceManager, null);
            //}
            //if (borders.Count > BorderId)
            //{
            //    Border = borders[BorderId + 1];
            //}
            //else
            //{
            //    Border = new ExcelBorder(NameSpaceManager, null);
            //}

            //RegisterEvent(this);
        }
        internal void RegisterEvent(ExcelXfs xf)
        {
            //                RegisterEvent(xf, xf.Xf_ChangedEvent);
        }
        //internal void RegisterEvent(ChangedEventHandler evh)
        //{
        //    RegisterEvent(this, evh);
        //}
        //internal void RegisterEvent(ExcelXfs xf, ChangedEventHandler evh)
        //{
        //    //xf.Numberformat.RegisterForChange(evh);

        //    xf.Font.RegisterForChange(evh);
        //    xf.Font.Color.RegisterForChange(evh);

        //    xf.Fill.RegisterForChange(evh);
        //    xf.Fill.PatternColor.RegisterForChange(evh);
        //    xf.Fill.BackgroundColor.RegisterForChange(evh);

        //    xf.Border.Bottom.RegisterForChange(evh);
        //    xf.Border.Left.RegisterForChange(evh);
        //    xf.Border.Top.RegisterForChange(evh);
        //    xf.Border.Right.RegisterForChange(evh);
        //    xf.Border.Diagonal.RegisterForChange(evh);
        //    xf.RegisterForChange(evh);
        //}
        internal override string Id
        {

            get
            {
                return XfId + "|" + NumberFormatId.ToString() + "|" + FontId.ToString() + "|" + FillId.ToString() + "|" + BorderId.ToString() + VerticalAlignment.ToString() + "|" + HorizontalAlignment.ToString() + "|" + WrapText.ToString() + "|" + ReadingOrder.ToString() + "|" + isBuildIn.ToString();
                //return Numberformat.Id + "|" + Font.Id + "|" + Fill.Id + "|" + Border.Id + VerticalAlignment.ToString() + "|" + HorizontalAlignment.ToString() + "|" + WrapText.ToString() + "|" + ReadingOrder.ToString(); 
            }
        }
        internal ExcelXfs Copy()
        {
            ExcelXfs newXF = new ExcelXfs(NameSpaceManager, _styles);
            newXF.NumberFormatId = _numFmtId;
            newXF.FontId = _fontId;
            newXF.FillId = _fillId;
            newXF.BorderId = _borderId;
            newXF.XfId = _xfID;
            newXF.ReadingOrder = _readingOrder;
            newXF.HorizontalAlignment = _horizontalAlignment;
            newXF.VerticalAlignment = _verticalAlignment;
            newXF.WrapText = _wrapText;
            return newXF;
        }

        internal int GetNewID(ExcelStyleCollection<ExcelXfs> xfsCol, StyleBase styleObject, eStyleClass styleClass, eStyleProperty styleProperty, object value)
        {
            ExcelXfs newXfs = this.Copy();
            switch(styleClass)
            {
                case eStyleClass.Numberformat:
                    newXfs.NumberFormatId = GetIdNumberFormat(styleProperty, value);
                    styleObject.SetIndex(newXfs.NumberFormatId);
                    break;
                case eStyleClass.Font:
                {
                    newXfs.FontId = GetIdFont(styleProperty, value);
                    styleObject.SetIndex(newXfs.FontId);
                    break;
                }
                case eStyleClass.Fill:
                case eStyleClass.FillBackgroundColor:
                case eStyleClass.FillPatternColor:
                    newXfs.FillId = GetIdFill(styleClass, styleProperty, value);
                    styleObject.SetIndex(newXfs.FillId);
                    break;
                case eStyleClass.Border:
                case eStyleClass.BorderBottom:
                case eStyleClass.BorderDiagonal:
                case eStyleClass.BorderLeft:
                case eStyleClass.BorderRight:
                case eStyleClass.BorderTop:
                    newXfs.BorderId = GetIdBorder(styleClass, styleProperty, value);
                    styleObject.SetIndex(newXfs.BorderId);
                    break;
                case eStyleClass.Style:
                    switch(styleProperty)
                    {
                        case eStyleProperty.XfId:
                            newXfs.XfId = (int)value;
                            break;
                        case eStyleProperty.HorizontalAlign:
                            newXfs.HorizontalAlignment=(ExcelHorizontalAlignment)value;
                            break;
                        case eStyleProperty.VerticalAlign:
                            newXfs.VerticalAlignment = (ExcelVerticalAlignment)value;
                            break;
                        case eStyleProperty.WrapText:
                            newXfs.WrapText = (bool)value;
                            break;
                        case eStyleProperty.ReadingOrder:
                            newXfs.ReadingOrder = (bool)value;
                            break;
                        case eStyleProperty.ShrinkToFit:
                            newXfs.ShrinkToFit=(bool)value;
                            break;
                        default:
                            throw (new Exception("Invalid property for class style."));

                    }
                    break;
                default:
                    break;
            }
            int id = xfsCol.FindIndexByID(newXfs.Id);
            if (id < 0)
            {
                return xfsCol.Add(newXfs.Id, newXfs);
            }
            return id;
        }

        private int GetIdBorder(eStyleClass styleClass, eStyleProperty styleProperty, object value)
        {
            ExcelBorderXml border = Border.Copy();

            switch (styleClass)
            {
                case eStyleClass.BorderBottom:
                    SetBorderItem(border.Bottom, styleProperty, value);
                    break;
                case eStyleClass.BorderDiagonal:
                    SetBorderItem(border.Diagonal, styleProperty, value);
                    break;
                case eStyleClass.BorderLeft:
                    SetBorderItem(border.Left, styleProperty, value);
                    break;
                case eStyleClass.BorderRight:
                    SetBorderItem(border.Right, styleProperty, value);
                    break;
                case eStyleClass.BorderTop:
                    SetBorderItem(border.Top, styleProperty, value);
                    break;
                case eStyleClass.Border:
                    if (styleProperty == eStyleProperty.BorderDiagonalUp)
                    {
                        border.DiagonalUp = (bool)value;
                    }
                    else if (styleProperty == eStyleProperty.BorderDiagonalDown)
                    {
                        border.DiagonalDown = (bool)value;
                    }
                    else
                    {
                        throw (new Exception("Invalid property for class Border."));
                    }
                    break;
                default:
                    throw (new Exception("Invalid class/property for class Border."));
            }
            int subId;
            string id = border.Id;
            subId = _styles.Borders.FindIndexByID(id);
            if (subId == int.MinValue)
            {
                return _styles.Borders.Add(id, border);
            }
            return subId;
        }

        private void SetBorderItem(ExcelBorderItemXml excelBorderItem, eStyleProperty styleProperty, object value)
        {
            if(styleProperty==eStyleProperty.Style)
            {
                excelBorderItem.Style = (ExcelBorderStyle)value;
            }
            else if (styleProperty == eStyleProperty.Color)
            {
                if (excelBorderItem.Style == ExcelBorderStyle.None)
                {
                    throw(new Exception("Can't set bordercolor when style is not set."));
                }
                excelBorderItem.Color.Rgb = value.ToString();
            }
        }

        private int GetIdFill(eStyleClass styleClass, eStyleProperty styleProperty, object value)
        {
            ExcelFillXml fill = Fill.Copy();

            switch (styleProperty)
            {
                case eStyleProperty.PatternType:
                    fill.PatternType = (ExcelFillStyle)value;
                    break;
                case eStyleProperty.Color:
                    if (fill.PatternType == ExcelFillStyle.None)
                    {
                        throw (new Exception("Can't set color when patterntype is not set."));
                    }
                    if (styleClass==eStyleClass.FillPatternColor)
                    {
                        fill.PatternColor.Rgb = value.ToString();
                    }
                    else
                    {
                        fill.BackgroundColor.Rgb = value.ToString();
                    }
                    break;
                default:
                    throw (new Exception("Invalid class/property for class Fill."));
            }
            int subId;
            string id = fill.Id;
            subId = _styles.Fills.FindIndexByID(id);
            if (subId == int.MinValue)
            {
                return _styles.Fills.Add(id, fill);
            }
            return subId;
        }
        private int GetIdNumberFormat(eStyleProperty styleProperty, object value)
        {
            if (styleProperty == eStyleProperty.Format)
            {
                ExcelNumberFormatXml item=null;
                if (!_styles.NumberFormats.FindByID(value.ToString(), ref item))
                {
                    item = new ExcelNumberFormatXml(NameSpaceManager) { Format = value.ToString(), NumFmtId = _styles.NumberFormats.NextId++ };
                    _styles.NumberFormats.Add(value.ToString(), item);
                }
                return item.NumFmtId;
            }
            else
            {
                throw (new Exception("Invalid property for class Numberformat"));
            }
        }
        private int GetIdFont(eStyleProperty styleProperty, object value)
        {
            ExcelFontXml fnt = Font.Copy();

            switch (styleProperty)
            {
                case eStyleProperty.Name:
                    fnt.Name = value.ToString();
                    break;
                case eStyleProperty.Size:
                    fnt.Size = (float)value;
                    break;
                case eStyleProperty.Family:
                    fnt.Family = (int)value;
                    break;
                case eStyleProperty.Bold:
                    fnt.Bold = (bool)value;
                    break;
                case eStyleProperty.Italic:
                    fnt.Italic = (bool)value;
                    break;
                case eStyleProperty.Strike:
                    fnt.Strike = (bool)value;
                    break;
                case eStyleProperty.Underline:
                    fnt.UnderLine = (bool)value;
                    break;
                case eStyleProperty.Color:
                    fnt.Color.Rgb=value.ToString();
                    break;
                case eStyleProperty.VerticalAlign:
                    fnt.VerticalAlign = value.ToString();
                    break;
                default:
                    throw (new Exception("Invalid property for class Font"));
            }
            int subId;
            string id = fnt.Id;
            subId = _styles.Fonts.FindIndexByID(id);
            if (subId == int.MinValue)
            {
                return _styles.Fonts.Add(id,fnt);
            }
            return subId;
        }

        internal override XmlNode CreateXmlNode(XmlNode topNode)
        {
            TopNode = topNode;
            if(_numFmtId>=0) SetXmlNode("@numFmtId", _numFmtId.ToString());
            if(_fontId >= 0) SetXmlNode("@fontId", _styles.Fonts[_fontId].newID.ToString());
            if (_fillId >= 0) SetXmlNode("@fillId", _styles.Fills[_fillId].newID.ToString());
            if(_borderId >= 0) SetXmlNode("@borderId", _styles.Borders[_borderId].newID.ToString());
            if(_horizontalAlignment != ExcelHorizontalAlignment.Left) this.SetXmlNode(horizontalAlignPath, SetAlignString(_horizontalAlignment));
            if (XfId > int.MinValue) SetXmlNode("@xfId", _styles.CellStyleXfs[_xfID].newID.ToString());

            if (_verticalAlignment != ExcelVerticalAlignment.Bottom) this.SetXmlNode(verticalAlignPath, SetAlignString(_verticalAlignment));
            if(_wrapText) this.SetXmlNode(wrapTextPath, "1");
            if(_readingOrder) this.SetXmlNode(readingOrderPath, "1");

            return TopNode;
        }

        private string SetAlignString(Enum align)
        {
            string newName = Enum.GetName(align.GetType(), align);
            return newName.Substring(0, 1).ToLower() + newName.Substring(1, newName.Length - 1);
        }
    }
}
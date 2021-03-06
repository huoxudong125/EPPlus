﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeOpenXml;
using System.IO;
using OfficeOpenXml.Drawing;
using System.Drawing;
using OfficeOpenXml.Drawing.Vml;
using OfficeOpenXml.Style;

namespace ExcelPackageTest
{
    [TestClass]
    public class WorkSheetTest
    {
        private TestContext testContextInstance;
        private static ExcelPackage _pck;
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            Directory.CreateDirectory(string.Format("Test"));
            _pck = new ExcelPackage(new FileInfo("Test\\Worksheet.xlsx"));
        }

        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            _pck = null;
        }
        [TestMethod]
        public void LoadData()
        {
            ExcelWorksheet ws = _pck.Workbook.Worksheets.Add("newsheet");
            ws.Cells["U19"].Value = new DateTime(2009, 12, 31);
            ws.Cells["U20"].Value = new DateTime(2010, 1, 1);
            ws.Cells["U21"].Value = new DateTime(2010, 1, 2);
            ws.Cells["U22"].Value = new DateTime(2010, 1, 3);
            ws.Cells["U23"].Value = new DateTime(2010, 1, 4);
            ws.Cells["U24"].Value = new DateTime(2010, 1, 5);
            ws.Cells["U19:U24"].Style.Numberformat.Format = "yyyy-mm-dd";

            ws.Cells["V19"].Value = 100;
            ws.Cells["V20"].Value = 102;
            ws.Cells["V21"].Value = 101;
            ws.Cells["V22"].Value = 103;
            ws.Cells["V23"].Value = 105;
            ws.Cells["V24"].Value = 104;
            ws.Cells["v19:v24"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            ws.Cells["v19:v24"].Style.Numberformat.Format = @"$#,##0.00_);($#,##0.00)";

            ws.Cells["X19"].Value = 210;
            ws.Cells["X20"].Value = 212;
            ws.Cells["X21"].Value = 221;
            ws.Cells["X22"].Value = 123;
            ws.Cells["X23"].Value = 135;
            ws.Cells["X24"].Value = 134;

            // add autofilter
            ws.Cells["U19:X24"].AutoFilter = true;
            ExcelPicture pic = ws.Drawings.AddPicture("Pic1", Properties.Resources.Test1);
            pic.SetPosition(150, 140);

            ws.Cells["A30"].Value = "Text orientation 45";
            ws.Cells["A30"].Style.TextRotation = 45;
            ws.Cells["B30"].Value = "Text orientation 90";
            ws.Cells["B30"].Style.TextRotation = 90;
            ws.Cells["c30"].Value = "Text orientation 180";
            ws.Cells["c30"].Style.TextRotation = 180;
            ws.Cells["D30"].Value = "Text orientation 38";
            ws.Cells["D30"].Style.TextRotation = 38;
            ws.Cells["D30"].Style.Font.Bold = true;
            ws.Cells["D30"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

            ws.Workbook.Names.Add("TestName", ws.Cells["B30:E30"]);
            ws.Workbook.Names["TestName"].Style.Font.Color.SetColor(Color.Red);


            ws.Workbook.Names["TestName"].Offset(1, 0).Value = "Offset test 1";
            ws.Workbook.Names["TestName"].Offset(2,-1, 2, 2).Value = "Offset test 2";

            //Test vertical align
            ws.Cells["E19"].Value = "Subscript";
            ws.Cells["E19"].Style.Font.VerticalAlign = ExcelVerticalAlignmentFont.Subscript;
            ws.Cells["E20"].Value = "Subscript";
            ws.Cells["E20"].Style.Font.VerticalAlign = ExcelVerticalAlignmentFont.Superscript;
            ws.Cells["E21"].Value = "Superscript";
            ws.Cells["E21"].Style.Font.VerticalAlign = ExcelVerticalAlignmentFont.Superscript;
            ws.Cells["E21"].Style.Font.VerticalAlign = ExcelVerticalAlignmentFont.None;


            ws.Cells["E22"].Value = "Indent 2";
            ws.Cells["E22"].Style.Indent = 2;
            ws.Cells["E23"].Value = "Shrink to fit";
            ws.Cells["E23"].Style.ShrinkToFit=true;


            ws.Names.Add("SheetName", ws.Cells["A1:A2"]);
            ws.View.FreezePanes(3, 5);

            _pck.Workbook.Properties.Author = "Jan Källman";
            _pck.Workbook.Properties.Category="Category";
            _pck.Workbook.Properties.Comments = "Comments";
            _pck.Workbook.Properties.Company="Adventure works";
            _pck.Workbook.Properties.Keywords = "Keywords";
            _pck.Workbook.Properties.Title = "Title";
            _pck.Workbook.Properties.Subject = "Subject";
            _pck.Workbook.Properties.Status = "status";
            _pck.Workbook.Properties.HyperlinkBase = new Uri("http://serversideexcel.com",UriKind.Absolute );
            _pck.Workbook.Properties.Manager= "Manager";
            //_pck.Workbook.Properties.LastModifiedBy = "jk";
            //_pck.Workbook.Properties.LastPrinted = "Yesterday";


            _pck.Workbook.Properties.SetCustomPropertyValue("DateTest", new DateTime(2008, 12, 31));
            TestContext.WriteLine(_pck.Workbook.Properties.GetCustomPropertyValue("DateTest").ToString());
            _pck.Workbook.Properties.SetCustomPropertyValue("Author", "Jan Källman");
            _pck.Workbook.Properties.SetCustomPropertyValue("Count", 1);
            _pck.Workbook.Properties.SetCustomPropertyValue("IsTested", false);
            _pck.Workbook.Properties.SetCustomPropertyValue("LargeNo", 123456789123);
            _pck.Workbook.Properties.SetCustomPropertyValue("Author", 3);
        }
        const int PERF_ROWS=5000;
        [TestMethod]
        public void Performance()
        {
            ExcelWorksheet ws=_pck.Workbook.Worksheets.Add("Perf");
            TestContext.WriteLine("StartTime {0}", DateTime.Now);

            Random r = new Random();
            for (int i = 1; i <= PERF_ROWS; i++)
            {
                ws.Cells[i,1].Value=string.Format("Row {0}\n.Test new row\"'",i);
                ws.Cells[i,2].Value=i;
                ws.Cells[i, 2].Style.WrapText = true;
                ws.Cells[i,3].Value=DateTime.Now;
                ws.Cells[i, 4].Value = r.NextDouble()*100000;                
            }            
            ws.Cells[1, 2, PERF_ROWS, 2].Style.Numberformat.Format="#,##0";
            ws.Cells[1, 3, PERF_ROWS, 3].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
            ws.Cells[1, 4, PERF_ROWS, 4].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[PERF_ROWS + 1, 2].Formula = "SUM(B1:B" + PERF_ROWS.ToString() +")";
            ws.Column(1).Width = 12;
            ws.Column(2).Width = 8;
            ws.Column(3).Width = 20;
            ws.Column(4).Width = 14;
            ws.DeleteRow(1000, 3, true);
            ws.DeleteRow(2000, 1, true);

            ws.InsertRow(2001, 4);

            ws.InsertRow(2010, 1, 2010);

            ws.InsertRow(20000, 2);

            ws.DeleteRow(20005, 4, false);

            //Single formula
            ws.Cells["H3"].Formula = "B2+B3";
            ws.DeleteRow(2, 1, true);

            ws.Cells["P3"].IsRichText = true;
            ws.Cells["P3"].Value = "<r><rPr><sz val=\"11\" /><color rgb=\"FFFF0000\" /><rFont val=\"Calibri\" /><family val=\"2\" /><scheme val=\"minor\" /></rPr><t>te</t></r><r><rPr><b /><sz val=\"11\" /> <color theme=\"1\" /><rFont val=\"Calibri\" /><family val=\"2\" /> <scheme val=\"minor\" /> </rPr><t>st</t> </r>";

            //Shared formula
            ws.Cells["H5:H30"].Formula = "B4+B5";
            ws.Cells["H5:H30"].Style.Numberformat.Format= "_(\"$\"* # ##0.00_);_(\"$\"* (# ##0.00);_(\"$\"* \"-\"??_);_(@_)";
            ws.InsertRow(7, 3);
            ws.InsertRow(2, 1);
            ws.DeleteRow(30, 3, true);

            ws.DeleteRow(15, 2, true);
            ws.Cells["a1:B100"].Style.Locked = false;
            ws.Cells["a1:B12"].Style.Hidden = true;
            TestContext.WriteLine("EndTime {0}", DateTime.Now);
        }
        [TestMethod]
        public void InsertDeleteTest()
        {
            ExcelWorksheet ws = _pck.Workbook.Worksheets.Add("InsertDelete");
            ws.Cells["A1:C5"].Value = 1;
            ws.Cells["A1:B3"].Merge = true;
            ws.Cells["D3"].Formula = "A2+C5";
            ws.InsertRow(2, 1);

            ws.Cells["A10:C15"].Value = 1;
            ws.Cells["A11:B13"].Merge = true;
            ws.DeleteRow(12, 1,true);

            ws.Cells["a1:B100"].Style.Locked = false;
            ws.Cells["a1:B12"].Style.Hidden = true;
            ws.Protection.IsProtected=true;
            ws.Protection.SetPassword("Password");


            var range = ws.Cells["B2:D100"];

            ws.PrinterSettings.PrintArea=null;
            ws.PrinterSettings.PrintArea=ws.Cells["B2:D99"];
            ws.PrinterSettings.PrintArea = null;
            ws.Row(15).PageBreak = true;
            ws.Column(3).PageBreak = true;
            ws.View.ShowHeaders = false;
            ws.View.PageBreakView = true;

            ws.Workbook.CalcMode = ExcelCalcMode.Automatic;

            Assert.AreEqual(range.Start.Column, 2);
            Assert.AreEqual(range.Start.Row, 2);
            Assert.AreEqual(range.Start.Address, "B2");

            Assert.AreEqual(range.End.Column, 4);
            Assert.AreEqual(range.End.Row, 100);
            Assert.AreEqual(range.End.Address, "D100");

            ExcelAddress addr = new ExcelAddress("B1:D3");

            Assert.AreEqual(addr.Start.Column, 2);
            Assert.AreEqual(addr.Start.Row, 1);
            Assert.AreEqual(addr.End.Column, 4);
            Assert.AreEqual(addr.End.Row, 3);
        }
        [TestMethod]
        public void RichTextCells()
        {
            ExcelWorksheet ws = _pck.Workbook.Worksheets.Add("RichText");
            var rs = ws.Cells["A1"].RichText;

            var r1 = rs.Add("Test");
            r1.Bold = true;
            r1.Color = Color.Pink;
            
            var r2 = rs.Add(" of");
            r2.Size = 14;
            r2.Italic = true;

            var r3 = rs.Add(" rich");
            r3.FontName = "Arial";
            r3.Size = 18;
            r3.Italic = true;

            var r4 = rs.Add("text.");
            r4.Size = 8.25f;
            r4.Italic = true;
            r4.UnderLine = true;

            rs=ws.Cells["A3:A4"].RichText;

            var r5 = rs.Add("Double");
            r5.Color = Color.PeachPuff;
            r5.FontName = "times new roman";
            r5.Size = 16;

            var r6 = rs.Add(" cells");
            r6.Color = Color.Red;
            r6.UnderLine=true;
        }
        [TestMethod]
        public void SaveWorksheet()
        {
            _pck.Save();
        }
        [TestMethod]
        public void TestComments()
        {
            var ws = _pck.Workbook.Worksheets.Add("Comment");            
            var comment = ws.Comments.Add(ws.Cells["B2"], "Jan Källman\r\nAuthor\r\n", "JK");            

            comment.RichText[0].Bold = true;
            comment.RichText[0].PreserveSpace = true;
            var rt = comment.RichText.Add("Test comment");
            comment.VerticalAlignment = eTextAlignVerticalVml.Center;
            comment.HorizontalAlignment = eTextAlignHorizontalVml.Center;
            comment.Visible = true;
            comment.BackgroundColor = Color.Green;
            comment.To.Row += 4;
            comment.To.Column += 2;
            comment.LineStyle = eLineStyleVml.LongDash;
            comment.LineColor = Color.Red;
            comment.LineWidth = (Single)2.5;
            rt.Color = Color.Red;

            var rt2=ws.Cells["C3"].AddComment("Range Added Comment test test test test test test test test test test testtesttesttesttesttesttesttesttesttesttest", "Jan Källman");
            ws.Cells["c3"].Comment.AutoFit = true;
        }
        [TestMethod]
        public void Address()
        {
            var ws = _pck.Workbook.Worksheets.Add("Address");
            ws.Cells["A1:A4,B5:B7"].Value = "AddressTest";
            ws.Cells["A1:A4,B5:B7"].Style.Font.Color.SetColor(Color.Red);
            ws.Cells["A2:A3,B4:B8"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.LightUp;
            ws.Cells["A2:A3,B4:B8"].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
            ws.Cells["2:2"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Cells["2:2"].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
            ws.Cells["B:B"].Style.Font.Name = "Times New Roman";

            ws.Cells["C4:G4,H8:H30,B15"].FormulaR1C1 = "RC[-1]+R1C[-1]";
            ws.Cells["G1,G3"].Hyperlink = new ExcelHyperLink("Comment!$A$1","Comment");
            ws.Cells["G1,G3"].Style.Font.Color.SetColor(Color.Blue);
            ws.Cells["G1,G3"].Style.Font.UnderLine = true;
        }
        [TestMethod]
        public void WorksheetCopy()
        {
            var ws = _pck.Workbook.Worksheets.Add("Copied Address", _pck.Workbook.Worksheets["Address"]);
            var wsCopy = _pck.Workbook.Worksheets.Add("Copied Comment", _pck.Workbook.Worksheets["Comment"]);
            Assert.AreEqual(2, wsCopy.Comments.Count);
        }
        [TestMethod]
        public void TestDelete()
        {
            string file = "test.xlsx";

            if (File.Exists(file))
                File.Delete(file);

            Create(file);

            ExcelPackage pack = new ExcelPackage(new FileInfo (file ));
            ExcelWorksheet w = pack.Workbook.Worksheets["delete"];
            w.DeleteRow(2, 1);
            pack.Save();
        }
        static void Create(string file)
        {
            ExcelPackage pack = new ExcelPackage(new FileInfo(file));
            ExcelWorksheet w = pack.Workbook.Worksheets.Add("delete");
            w.Cells[1, 1].Value = "test";
            w.Cells[2, 1].Value = "to delete";
            pack.Save();
        }
        [TestMethod]
        public void RowStyle()
        {
            FileInfo newFile = new FileInfo(@"sample8.xlsx");
            if (newFile.Exists)
            {
                newFile.Delete();  // ensures we create a new workbook
                newFile = new FileInfo(@"sample8.xlsx");
            }

            ExcelPackage package = new ExcelPackage();
            //Load the sheet with one string column, one date column and a few random numbers.
            var ws = package.Workbook.Worksheets.Add("First line test");

            ws.Cells[1, 1].Value = "1; 1";
            ws.Cells[2, 1].Value = "2; 1";
            ws.Cells[1, 2].Value = "1; 2";
            ws.Cells[2, 2].Value = "2; 2";

            ws.Row(1).Style.Font.Bold = true;
            ws.Column(1).Style.Font.Bold = true;
            package.SaveAs(newFile);

        }
        [TestMethod]
        public void HideTest()
        {
            var ws = _pck.Workbook.Worksheets.Add("Hidden");
            ws.Cells["A1"].Value = "This workbook is hidden"    ;
            ws.Hidden = eWorkSheetHidden.Hidden;
        }
        [TestMethod]
        public void VeryHideTest()
        {
            var ws = _pck.Workbook.Worksheets.Add("VeryHidden");
            ws.Cells["a1"].Value = "This workbook is hidden";
            ws.Hidden = eWorkSheetHidden.VeryHidden;
        }
        [TestMethod]
        public void PrinterSettings()
        {
            var ws = _pck.Workbook.Worksheets.Add("Sod/Hydroseed");

            ws.Cells[1, 1].Value = "1; 1";
            ws.Cells[2, 1].Value = "2; 1";
            ws.Cells[1, 2].Value = "1; 2";
            ws.Cells[2, 2].Value = "2; 2";
            ws.Cells[1, 1, 1, 2].AutoFilter = true;
            ws.PrinterSettings.BlackAndWhite = true;
            ws.PrinterSettings.ShowGridLines = true;
            ws.PrinterSettings.ShowHeaders = true;
            ws.PrinterSettings.PaperSize = ePaperSize.A4;

            ws.PrinterSettings.RepeatRows = new ExcelAddress("1:1");
            ws.PrinterSettings.RepeatColumns = new ExcelAddress("A:A");

            ws.PrinterSettings.Draft = true;

            ws.PrinterSettings.PrintArea=ws.Cells["A1:B2"];

            ws.Select(new ExcelAddress("3:4,E5:F6"));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using System.Windows;
using System.Windows.Input;
using System.Reflection;

namespace FoodBankInventoryManager
{
    class ExcelExporter<T, U>
        where T : class
        where U : List<T>
    {
        public List<T> dataToPrint;

        private Microsoft.Office.Interop.Excel.Application myExcelApp;
        private Workbooks myBooks;
        private Workbook myBook;
        private Sheets mySheets;
        private Worksheet mySheet;
        private Range myRange;
        private Font myFont;
        private object myOptionalValue = Missing.Value;

        /// <summary>
        /// Constructor
        /// </summary>
        public ExcelExporter()
        {
            myExcelApp = null;
            myBooks = null;
            myBook = null;
            mySheets = null;
            mySheet = null;
            myRange = null;
            myFont = null;
        }
        /// <summary>
        /// Generates excel spreadsheet and then releases item to garbage collector
        /// </summary>
        public void GenerateReport()
        {
            try
            {
                if (dataToPrint != null)
                {
                    if (dataToPrint.Count != 0)
                    {
                        Mouse.SetCursor(Cursors.Wait);
                        CreateExcelRef();
                        FillSheet();
                        OpenReport();
                        Mouse.SetCursor(Cursors.Arrow);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while generating Excel report.");
            }
            finally
            {
                ReleaseObject(mySheet);
                ReleaseObject(mySheets);
                ReleaseObject(myBook);
                ReleaseObject(myBooks);
                ReleaseObject(myExcelApp);
            }
        }
        /// <summary>
        /// Make Excel Visible
        /// </summary>
        private void OpenReport()
        {
            myExcelApp.Visible = true;
        }
        /// <summary>
        /// Populate Excel sheet
        /// </summary>
        private void FillSheet()
        {
            object[] header = CreateHeader();
            WriteData(header);
        }
        /// <summary>
        /// Write data into Excel spreadsheet
        /// </summary>
        /// <param name="header"></param>
        public void WriteData(object[] header)
        {
            object[,] objectData = new object[dataToPrint.Count, header.Length];

            for (int i = 0; i < dataToPrint.Count; i++)
            {
                var item = dataToPrint[i];
                for (int j = 0; j < header.Length; j++)
                {
                    var y = typeof(T).InvokeMember(header[j].ToString(),
                        System.Reflection.BindingFlags.GetProperty, null, item, null);
                    objectData[i, j] = (y == null) ? "" : y.ToString();
                }
            }
            AddExcelRows("A2", dataToPrint.Count, header.Length, objectData);
            AutoFitColumns("A1", dataToPrint.Count + 1, header.Length);
        }
        /// <summary>
        /// Method to make columns auto fit according to data
        /// </summary>
        /// <param name="startRange"></param>
        /// <param name="rowCount"></param>
        /// <param name="colCount"></param>
        private void AutoFitColumns(string startRange, int rowCount, int colCount)
        {
            myRange = mySheet.get_Range(startRange, myOptionalValue);
            myRange = myRange.get_Resize(rowCount, colCount);
            myRange.Columns.AutoFit();
        }
        /// <summary>
        /// Create header from the properties
        /// </summary>
        /// <returns></returns>
        private object[] CreateHeader()
        {
            PropertyInfo[] headerInfo = typeof(T).GetProperties();

            List<object> objectHeaders = new List<object>();
            for (int i = 0; i < headerInfo.Length; i++)
            {
                objectHeaders.Add(headerInfo[i].Name);
            }

            var headerToAdd = objectHeaders.ToArray();
            AddExcelRows("A1", 1, headerToAdd.Length, headerToAdd);
            SetHeaderStyle();

            return headerToAdd;
        }
        /// <summary>
        /// Set Header style as bold
        /// </summary>
        private void SetHeaderStyle()
        {
            myFont = myRange.Font;
            myFont.Bold = true;
        }
        /// <summary>
        /// Add an excel row
        /// </summary>
        /// <param name="startRange"></param>
        /// <param name="rowCount"></param>
        /// <param name="colCount"></param>
        /// <param name="values"></param>
        private void AddExcelRows(string startRange, int rowCount, int colCount, object values)
        {
            myRange = mySheet.get_Range(startRange, myOptionalValue);
            myRange = myRange.get_Resize(rowCount, colCount);
            myRange.set_Value(myOptionalValue, values);
        }
        /// <summary>
        /// Create Excel application parameters instances
        /// </summary>
        private void CreateExcelRef()
        {
            myExcelApp = new Microsoft.Office.Interop.Excel.Application();
            myBooks = (Workbooks)myExcelApp.Workbooks;
            myBook = (Workbook)(myBooks.Add(myOptionalValue));
            mySheets = (Sheets)myBook.Worksheets;
            mySheet = (Worksheet)(mySheets.get_Item(1));
        }
        /// <summary>
        /// Release unused COM objects
        /// </summary>
        /// <param name="obj"></param>
        private void ReleaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show(ex.Message.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }
    }
}

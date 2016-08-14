using System;
using System.Collections.Generic;
using Microsoft.Office.Interop.Excel;
using System.Windows;
using System.Windows.Input;
using System.Reflection;

namespace FoodBankInventoryManager
{
    /// <summary>
    /// When object of this class is instantiated, restricts the second type param to a list of the type of the
    /// first type param
    /// </summary>
    /// <typeparam name="T">Class</typeparam>
    /// <typeparam name="U">List of type T</typeparam>
    class ExcelExporter<T, U>
        where T : class
        where U : List<T>
    {
        //Data to be printed to excel spreadsheet
        public List<T> dataToPrint;

        //Excel associated objects and properties
        private Microsoft.Office.Interop.Excel.Application myExcelApp;
        private Workbooks myBooks;
        private Workbook myBook;
        private Sheets mySheets;
        private Worksheet mySheet;
        private Range myRange;
        private Font myFont;
        private object myOptionalValue = Missing.Value;

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
                //Assuming data isn't null or empty, goes through all of the steps to export data to an excel spreadsheet
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
            //All data that will fill out excel spreadsheet
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

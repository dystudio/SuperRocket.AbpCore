using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using NPOI;
using NPOI.HPSF;
using NPOI.HSSF;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.POIFS;
using NPOI.SS.UserModel;
using NPOI.Util;
using NPOI.XSSF.UserModel;

namespace SuperRocket.ConsoleCore
{
    public static class ExcelUtility
    {
        /// <summary>
        /// 导出Excel数据
        /// </summary>
        /// <param name="ds">数据源</param>
        /// <param name="headLists">标题中英文对照</param>
        /// <param name="SheetNameList">Sheet名</param>
        /// <param name="fileName">文件名称</param>
        public static void ExportExcel(DataSet ds, List<Dictionary<string, string>> headLists, List<string> SheetNameList, string fileName)
        {


            HSSFWorkbook workbook = new HSSFWorkbook();

            HSSFSheet sheet = null;
            HSSFRow headerRow = null;
            string head = string.Empty;
            string sheetName = string.Empty;
            string sheetOrder = string.Empty;
            for (int z = 0; z < ds.Tables.Count; z++)
            {
                DataTable dtData = ds.Tables[z];
                Dictionary<string, string> headList = headLists[z];
                sheetName = SheetNameList[z];

                //设置表头样式
                ICellStyle headStyle = workbook.CreateCellStyle();
                headStyle.VerticalAlignment = VerticalAlignment.Center;
                headStyle.Alignment = HorizontalAlignment.Center;
                IFont font = workbook.CreateFont();
                font.FontHeight = 14 * 14;
                font.Boldweight = 1000;
                headStyle.SetFont(font);

                if (dtData.Rows.Count > 0)
                {
                    int mod = dtData.Rows.Count % 65535;
                    int index = dtData.Rows.Count / 65535;

                    if (mod > 0)
                    {
                        index = index + 1;
                    }

                    ICellStyle dateStyle = workbook.CreateCellStyle();
                    IDataFormat format = workbook.CreateDataFormat();
                    dateStyle.DataFormat = format.GetFormat("yyyy-MM-dd HH:mm");
                    for (int idx = 1; idx <= index; idx++)
                    {
                        int count = 65535;
                        if (idx == index)
                        {
                            count = mod;
                        }
                        if (index == 1)
                            sheetOrder = string.Empty;
                        else
                            sheetOrder = idx.ToString();

                        sheet = workbook.CreateSheet(sheetName + sheetOrder) as HSSFSheet;
                        headerRow = sheet.CreateRow(0) as HSSFRow;

                        for (int j = 0; j < count; j++)//循环记录总数作为行数
                        {
                            HSSFRow dataRow = sheet.CreateRow(j + 1) as HSSFRow;
                            int i = 0;

                            foreach (KeyValuePair<string, string> names in headList)//循环列表头集合作为列数
                            {
                                string[] keys = names.Key.Split('@');
                                string value = dtData.Rows[65535 * (idx - 1) + j][keys[0]].ToString();

                                ICell cell = headerRow.CreateCell(i);
                                cell.CellStyle = headStyle;
                                cell.SetCellValue(names.Value);
                                //收款人解码
                                if (keys[0] == "Payee")
                                {
                                    value = System.Web.HttpUtility.UrlDecode(value);
                                }
                                //数字类型（带格式0:P2）
                                if (keys.Length == 2)
                                {
                                    if (string.IsNullOrEmpty(value))
                                    {
                                        dataRow.CreateCell(i).SetCellValue(value);
                                    }
                                    else
                                    {
                                        dataRow.CreateCell(i).SetCellValue(decimal.Parse(value).ToString(keys[1]));
                                    }
                                }
                                else
                                {
                                    GetStringToObject(value, dateStyle, dataRow.CreateCell(i), dtData.Columns[keys[0]].DataType.ToString());
                                }
                                i++;
                            }

                        }
                    }
                }
                else//若没有记录则添加表头
                {
                    sheet = workbook.CreateSheet(sheetName) as HSSFSheet;
                    headerRow = sheet.CreateRow(0) as HSSFRow;
                    int i = 0;
                    foreach (KeyValuePair<string, string> names in headList)//循环列表头集合作为列数
                    {
                        ICell cell = headerRow.CreateCell(i);
                        cell.CellStyle = headStyle;
                        cell.SetCellValue(names.Value);
                        i++;
                    }
                }
            }
            using (FileStream stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                try
                {
                    System.Console.WriteLine($"start write data to stream for the excel");
                    workbook.Write(stream);
                    System.Console.WriteLine($"stoop write data to stream for the excel");
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Exception: " + ex.Message);
                }
                finally
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }

        public static void GetStringToObject(string drValue, ICellStyle cellStyle, ICell newCell, string colType)
        {

            switch (colType)
            {
                case "System.String"://字符串类型
                    newCell.SetCellValue(drValue);
                    break;
                case "System.DateTime"://日期类型
                    if (!string.IsNullOrEmpty(drValue))
                    {
                        DateTime dateV;
                        DateTime.TryParse(drValue, out dateV);
                        newCell.CellStyle = cellStyle;//格式化显示
                        newCell.SetCellValue(dateV);
                    }
                    break;
                case "System.Boolean"://布尔型
                    bool boolV = false;
                    bool.TryParse(drValue, out boolV);
                    newCell.SetCellValue(boolV);
                    break;
                case "System.Int16"://整型
                case "System.Int32":
                case "System.Int64":
                case "System.Byte":
                    if (!string.IsNullOrEmpty(drValue))
                    {
                        int intV = 0;
                        int.TryParse(drValue, out intV);
                        newCell.SetCellValue(intV);
                    }
                    break;
                case "System.Decimal"://浮点型
                case "System.Double":
                    if (!string.IsNullOrEmpty(drValue))
                    {
                        double doubV = 0;
                        double.TryParse(drValue, out doubV);
                        newCell.SetCellValue(doubV);
                    }
                    break;
                case "System.DBNull"://空值处理
                    newCell.SetCellValue("");
                    break;
                default:
                    newCell.SetCellValue("");
                    break;
            }
        }
    } 
}
   


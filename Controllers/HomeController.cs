using CsvHelper;
using CSVParseApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;

namespace CSVParseApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> BridgeData()
        {

            DateTime currentTime = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
            string filePath = string.Concat(@"D:\datafiles\", unixTime);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            using (var client = new WebClient())
            {
                client.DownloadFile(
                          "https://estore11.sitealivedev.com/datafiles/sa/bridge_condition_open_data_2020_en.csv",
                          filePath + @"\bridge_condition_open_data_2020_en.csv");
            }


            DataTable csvTable = new DataTable();
            string filePathCsv = Directory.GetFiles(filePath).FirstOrDefault();
            var parseCsvFile = GetDataTabletFromCSVFile(filePathCsv);
            return View(parseCsvFile);
        }


        private static DataTable GetDataTabletFromCSVFile(string csv_file_path)
        {
            DataTable csvData = new DataTable();
            DataRow[] result;
            try
            {

                using (TextFieldParser csvReader = new TextFieldParser(csv_file_path))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    string[] colFields = csvReader.ReadFields();
                    foreach (string column in colFields)
                    {
                        DataColumn datecolumn = new DataColumn(column);
                        datecolumn.AllowDBNull = true;
                        csvData.Columns.Add(datecolumn);
                    }

                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();
                        //Making empty value as null
                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            if (fieldData[i] == "")
                            {
                                fieldData[i] = null;
                            }
                        }
                        DataRow rowData = csvData.NewRow();
                        if (fieldData.Contains("404"))
                        {
                            rowData[0] = fieldData[3];
                            rowData[1] = String.Format("https://www.google.ca/maps/search/{0},{1}", fieldData[4], fieldData[5]);
                            rowData[2] = fieldData[6];
                            rowData[3] = fieldData[10];
                            var maxNum = Math.Max(Convert.ToInt64(fieldData[10]),Math.Max(Convert.ToInt64(fieldData[11]), Convert.ToInt64(fieldData[12])));
                            var originalNum = DateTime.Now.Year - maxNum;
                            if (fieldData[10] == "1996" && fieldData[11] == "2016")
                            {
                                originalNum = DateTime.Now.Year - 2016; 
                            }
                            rowData[4] = originalNum.ToString();
                            
                            csvData.Rows.Add(rowData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return csvData;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
using Block.Interface;
using log4net;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace RunClient.mission {
    public class mission {


        private ICellStyle hlink;
        private ICellStyle successStyle;
        private ICellStyle failStyle;

        public string excelFile { get; set; }

        public Uri url { get; set; }

        private XSSFWorkbook xssfworkbook;

        private ISheet configSheet;

        private List<testCase> caseList;


        public string dic { get; set; }





        public void Run() {


            runInit();
            var logger = LogManager.GetLogger(GetType());
            foreach (var tc in caseList) {
                foreach (var rtc in tc.runCases) {
                    OneBlockCase oneCase;
                    int RowNum = rtc.row.RowNum;
                    try {
                        logger.Debug($"获取案例,案例ID:{tc.ID},数据行:{RowNum},接口地址:{url}");
                        oneCase = getCaseByBlock(url, tc.ID, rtc.attrs);
                    } catch (Exception e) {
                        logger.Warn($"获取案例错误,跳过该数据继续执行. \r\n{e.Message} \r\n{e.StackTrace}");
                        continue;
                    }



                    ICaseResult result = null;
                    string resultPath = Path.Combine(dic, DateTime.Now.ToString("yyyyMMddhhmmss"));
                    try {
                        logger.Debug($"案例-{rtc.caseName}[{tc.ID}],数据行-{RowNum} 开始执行...");

                        if (!Directory.Exists(resultPath)) {
                            Directory.CreateDirectory(resultPath);
                        }
                        var RF = new Block.Selenium.Web.RunFactory();

                        var log = new myLog();
                        var runFactory = new Block.Selenium.Web.RunFactory();
                        runFactory.log = log;

                        result = runFactory.startRun(oneCase, resultPath);
                        logger.Info($"案例:{rtc.caseName}[{tc.ID}],数据行-{RowNum} 执行完成!");

                    } catch (Exception e) {
                        logger.Warn($"run事件错误:{e.Message} \r\n{e.StackTrace}");

                        result = new Block.Selenium.Web.CaseResult();
                        result.message = "异常错误:" + e.StackTrace;
                        result.statusCode = CaseResultCode.fatal;
                        result.resultPath = resultPath;
                    }
                    rtc.result = result;

                  
                }
            }
            
            saveExcel($"{dic}/{Path.GetFileNameWithoutExtension(excelFile)}_result.xlsx");
        }


        /// <summary>
        /// 通过数据获取参数
        /// </summary>
        /// <param name="url">平台地址</param>
        /// <param name="id">案例id</param>
        /// <param name="attrs">参数列表</param>
        /// <returns></returns>
        private OneBlockCase getCaseByBlock(Uri url, int id, Dictionary<string, string> attrs) {

            Uri finUri = new Uri(url, "TestCase/runCase/"+id.ToString());
            //创建连接
            HttpWebRequest mHttpRequest = (HttpWebRequest)WebRequest.Create(finUri);
            mHttpRequest.Timeout = 30000;
            mHttpRequest.Method = "POST";
            mHttpRequest.Accept = "*/*";

            mHttpRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            //mHttpRequest.AddRange(100, 10000);//字节范围
            mHttpRequest.UseDefaultCredentials = true;



            //转码
            StringBuilder buffer = new StringBuilder();
            int i = 0;
            foreach (string key in attrs.Keys) {
                if (i > 0) {
                    buffer.AppendFormat("&{0}={1}", key, attrs[key]);
                } else {
                    buffer.AppendFormat("{0}={1}", key, attrs[key]);
                }
                i++;
            }
            var postData = string.Join("&", attrs.Select(
            p => string.Format("{0}={1}", p.Key, System.Web.HttpUtility.UrlEncode(p.Value, Encoding.UTF8))).ToArray());
            byte[] data = Encoding.UTF8.GetBytes(postData);
            using (Stream stream = mHttpRequest.GetRequestStream()) {
                stream.Write(data, 0, data.Length);
            }


            HttpWebResponse response = (HttpWebResponse)mHttpRequest.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.GetEncoding("utf-8"));



            return JsonConvert.DeserializeObject<OneBlockCase>(reader.ReadToEnd());

        }

        private void saveExcel(string excelResultPath) {

            foreach (var cs in caseList) {

                int total = cs.runCases.Count;

                int sucess = cs.runCases.Count(t => t.result != null && t.result.statusCode == CaseResultCode.sucess);

                int fail = cs.runCases.Count(t => t.result != null && t.result.statusCode != CaseResultCode.sucess);

                int noRun = total - sucess - fail;

                cs.configRow.CreateCell(3).SetCellValue(total);

                cs.configRow.CreateCell(4).SetCellValue(sucess);

                cs.configRow.CreateCell(5).SetCellValue(fail);

                cs.configRow.CreateCell(6).SetCellValue(noRun);


                bool setStyle = true;
                foreach (var rcs in cs.runCases) {
                    if (rcs.result == null) continue;

                    var lastCell = rcs.attrs.Count;


                    var message = rcs.row.CreateCell(lastCell + 1);
                    message.SetCellValue(rcs.result.message);
                    if (rcs.result.statusCode == CaseResultCode.sucess) {
                        message.CellStyle = successStyle;
                    } else {
                        message.CellStyle = failStyle;
                    }

                    /***********结果文件**************/
                    var p = rcs.row.CreateCell(lastCell + 2);
                    p.SetCellValue(rcs.result.resultPath);

                    //创建一个超链接对象
                    XSSFHyperlink link = new XSSFHyperlink(HyperlinkType.Unknown);


                    link.Address = $"file:///{ Path.GetFileName(rcs.result.resultPath) }";
                    p.Hyperlink = link;

                    //样式
                    p.CellStyle = hlink;
                    //列宽只需要设置一次,但是其实重复设置并不会有什么问题,这里只是为了维稳
                    if (setStyle) {
                        rcs.row.Sheet.SetColumnWidth(p.ColumnIndex, 50 * 256);
                        setStyle = false;
                    }

                }

            }



            try {
                using (FileStream fileStream = File.Open(excelResultPath, FileMode.Create, FileAccess.Write)) {
                    xssfworkbook.Write(fileStream);
                }
                var logger = LogManager.GetLogger(GetType());
                logger.Info($"场景执行完成,请查看结果文件:{excelResultPath}");
            } catch (Exception e) {
                var logger = LogManager.GetLogger(GetType());
                logger.Warn($"{e.Message}\r\n{e.StackTrace}保存excel失败 path:{excelFile}.");

            }

        }

        /// <summary>
        /// 初始化测试所需资源
        /// </summary>
        private void runInit() {
            xssfworkbook = null;

            //byte[] data = File.ReadAllBytes(excelFile);
            //MemoryStream ms = new MemoryStream(data);
            //xssfworkbook = new XSSFWorkbook(ms);
            using (FileStream ms = File.Open(excelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                xssfworkbook = new XSSFWorkbook(ms);
            }


            XSSFFormulaEvaluator eva = new XSSFFormulaEvaluator(xssfworkbook);
            eva.EvaluateAll();//取结果不取公式

            configSheet = xssfworkbook.GetSheetAt(0);

            //各种样式初始化
            hlink = xssfworkbook.CreateCellStyle();
            IFont hlink_font = xssfworkbook.CreateFont();
            hlink_font.Underline = FontUnderlineType.Single;
            hlink_font.Color = NPOI.HSSF.Util.HSSFColor.Blue.Index;
            hlink.SetFont(hlink_font);



            successStyle = xssfworkbook.CreateCellStyle();
            var successFont = xssfworkbook.CreateFont();
            successFont.Color = NPOI.HSSF.Util.HSSFColor.Green.Index;
            successStyle.SetFont(successFont);

            failStyle = xssfworkbook.CreateCellStyle();
            var failFont = xssfworkbook.CreateFont();
            failFont.Color = NPOI.HSSF.Util.HSSFColor.Red.Index;
            failStyle.SetFont(failFont);

            //各种样式初始化 over


            System.Collections.IEnumerator rows = configSheet.GetRowEnumerator();
            rows.MoveNext();

            IRow headRow = (IRow)rows.Current;
            headRow.CreateCell(3).SetCellValue("案例总数");
            var c4 = headRow.CreateCell(4);
            c4.CellStyle = successStyle;
            c4.SetCellValue("成功");
            var c5 = headRow.CreateCell(5);
            c5.CellStyle = failStyle;
            c5.SetCellValue("失败");

            var c6 = headRow.CreateCell(6);
            c6.SetCellValue("未执行");

            caseList = new List<testCase>();
            while (rows.MoveNext()) {
                IRow row = (IRow)rows.Current;
                string sheetName = row.GetCell(1).ToString();
                if (string.IsNullOrEmpty(sheetName)) break;

                try {
                    var tc = new testCase();
                    runCaseInit(tc, row);
                    caseList.Add(tc);
                } catch (Exception e) {
                    var logger = LogManager.GetLogger(GetType());
                    logger.Warn($"{e.Message} \r\n{e.StackTrace}");
                    break;
                }

            }

        }


        /// <summary>
        /// 初始化案例列表
        /// </summary>
        private static void runCaseInit(testCase tc, IRow configRow) {


            tc.configRow = configRow;
            var nameCell = tc.configRow.GetCell(0);
            if (nameCell != null)
                tc.name = nameCell.StringCellValue;
            var idCell = tc.configRow.GetCell(1);
            if (idCell != null)
                tc.ID = Convert.ToInt32(idCell.StringCellValue);

            ISheet caseSheet = configRow.Sheet.Workbook.GetSheet(tc.ID.ToString());



            tc.runCases = new List<runTestCase>();

            System.Collections.IEnumerator rows = caseSheet.GetRowEnumerator();
            //读取下一行 
            rows.MoveNext();//如果没有要处理下
            IRow headRow = (IRow)rows.Current;

            List<string> attrsKey = new List<string>();

            //首先获得参数列表
            for (int i = 1; i < headRow.LastCellNum; i++) {
                ICell cell = headRow.GetCell(i);
                if (cell == null || string.IsNullOrEmpty(cell.StringCellValue)) break;//有空数据直接退出
                string key = cell.ToString();
                attrsKey.Add(key);
            }



            //逐行转化案例
            while (rows.MoveNext()) {

                IRow row = (IRow)rows.Current;
                var runNameCell = row.GetCell(0);
                if (runNameCell == null || runNameCell.StringCellValue.Trim() == "") break;//案例名字没有退出

                string caseName = runNameCell.StringCellValue;
                Dictionary<string, string> attrs = new Dictionary<string, string>();
                for (int i = 0; i < attrsKey.Count; i++) {
                    ICell cell = row.GetCell(i + 1);
                    string value = "";
                    if (cell != null) {
                        //cell = eva.EvaluateInCell(cell);
                        cell.SetCellType(CellType.String);
                        value = cell.StringCellValue;
                    }
                    attrs[attrsKey[i]] = value;
                }

                runTestCase rt = new runTestCase();
                rt.caseName = caseName;
                rt.attrs = attrs;
                rt.row = row;


                tc.runCases.Add(rt);
            }
        }




    }






}

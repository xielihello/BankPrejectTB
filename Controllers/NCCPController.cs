using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bll;
using IBll;
using Model;
using System.Data;
using System.Data.OleDb;
using System.Text;
using System.IO;
namespace Project.Controllers
{
    public class NCCPController : Controller
    {
        //
        // GET: /NCCP/
        Itb_nccp_filelistService nfbll = new tb_nccp_filelistService();
        Itb_nccp_return_infoService nribll = new tb_nccp_return_infoService();
        Itb_nccp_apply_infoService naibll = new tb_nccp_apply_infoService();
        Itb_nccp_account_infoService naibll2 = new tb_nccp_account_infoService();
        Idal_ComplexPlanService dcpbll = new dal_ComplexPlanService();
        public ActionResult Index()
        {
            string formid = Request["formid"];
            string tellerId = Request["tellerId"];
            string examid = Request["examid"];
            string banksiteid = Request["banksiteid"];
            string DepartmentId = Request["DepartmentId"];
            string planid = Request["planid"];
            string taskid = Request["taskid"];

            Models.userdata userdata = new Models.userdata();
            userdata.formid = formid;
            userdata.tellerId = tellerId;
            userdata.examid = examid;
            userdata.banksiteid = banksiteid;
            userdata.DepartmentId = DepartmentId;
            userdata.planid = planid;

            Session["UserData"] = userdata;

            return View();
        }

        public ActionResult Deduct()
        {

            List<dal_ComplexPlan> plans = dcpbll.Query(a => true).OrderByDescending(a => a.Id).ToList();
            ViewBag.plans = plans; 

            return View();
        }

        [HttpPost, ActionName("Deduct")]
        public ActionResult DeductPost()
        {
            //List<tb_nccp_filelist> list = nfbll.Query(a => true);

            //return Json(new { succ = true, rows = list, total = list.Count });

            Models.userdata ud = Session["UserData"] as Models.userdata;
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            int total = 0;
            string condition = "1=1 and exam_id=" + ud.planid;
            //if (!string.IsNullOrEmpty(Request["examid"]))
            //{
            //    int examid = Convert.ToInt32(Request["examid"]);

            //    if (examid != -1)
            //    {
            //        condition += " and exam_id=" + examid;
            //    }
            //}
            List<tb_nccp_filelist> list = nfbll.Query(condition, "id", page, row, out total);
            List<Dictionary<string, object>> dic = null;
            if (list != null)
            {
                dic = Common.JsonHelper.JSONToObject<List<Dictionary<string, object>>>(Common.JsonHelper.ObjectToJSON(list));
                int len = list.Count();
                for (int i = 0; i < len; i++)
                {
                    dic[i]["date_time"] = list[i].date_time.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    dic[i]["control_apply"] = "请求信息";
                    dic[i]["download"] = "未下载";
                    dic[i]["return_info"] = "回退信息";
                }
            }
            return Json(new { succ = true, rows = dic, total = total });



        }

        public ActionResult ReturnInfo()
        {
            ViewBag.apply_id = Request["apply_id"];
            return View();
        }


        [HttpPost, ActionName("ReturnInfo")]
        public ActionResult ReturnInfoPost()
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            int total = 0;
            string condition = "1=1";
            int apply_id = Convert.ToInt32(Request["apply_id"]);
            condition += " and apply_id=" + apply_id;

            List<tb_nccp_return_info> list = nribll.Query(condition, "id", page, row, out total);
            List<Dictionary<string, object>> dic = null;
            if (list != null)
            {
                dic = Common.JsonHelper.JSONToObject<List<Dictionary<string, object>>>(Common.JsonHelper.ObjectToJSON(list));
                int len = list.Count();
                for (int i = 0; i < len; i++)
                {
                    dic[i]["date_time"] = list[i].datetime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            return Json(new { succ = true, rows = dic, total = total });
        }


        public ActionResult ApplyInfo()
        {
            ViewBag.apply_id = Request["apply_id"];
            return View();
        }

        [HttpPost, ActionName("ApplyInfo")]
        public ActionResult ApplyInfoPost()
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            int total = 0;
            string condition = "1=1";
            int apply_id = Convert.ToInt32(Request["apply_id"]);
            condition += " and apply_id=" + apply_id;
            List<tb_nccp_apply_info> list = naibll.Query(condition, "id", page, row, out total);

            List<Dictionary<string, object>> dic = null;
            if (list != null)
            {
                dic = Common.JsonHelper.JSONToObject<List<Dictionary<string, object>>>(Common.JsonHelper.ObjectToJSON(list));
                int len = list.Count();
                for (int i = 0; i < len; i++)
                {
                    dic[i]["control_account"] = "账户信息";
                }
            }
            return Json(new { succ = true, rows = dic, total = total });

        }

        public ActionResult AccountInfo()
        {
            ViewBag.apply_id = Request["apply_id"];
            ViewBag.userdata = Session["UserData"] as Models.userdata;

            return View();
        }

        [HttpPost, ActionName("AccountInfo")]
        public ActionResult AccountInfoPost()
        {
            int page = Convert.ToInt32(Request["page"]);
            int row = Convert.ToInt32(Request["rows"]);
            int total = 0;
            string condition = "1=1";
            int apply_id = Convert.ToInt32(Request["apply_id"]);
            condition += " and apply_id=" + apply_id;
            List<tb_nccp_account_info> list = naibll2.Query(condition, "id", page, row, out total);

            List<Dictionary<string, object>> dic = null;
            if (list != null)
            {
                dic = Common.JsonHelper.JSONToObject<List<Dictionary<string, object>>>(Common.JsonHelper.ObjectToJSON(list));
                int len = list.Count();
                for (int i = 0; i < len; i++)
                {
                    dic[i]["lsh"] = list[i].ydjqqdh;
                    dic[i]["ydjsj"] = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                    dic[i]["sqkhsj"] = list[i].sqkhsj.HasValue ? list[i].sqkhsj.Value.ToString("yyyy-MM-dd HH:mm:ss") : "-";
                    dic[i]["apply_download"] = "请求下载";
                    dic[i]["submit_result"] = "结果提交";

                }
            }
            return Json(new { succ = true, rows = dic, total = total });
        }


        public ActionResult SubmitResult()
        {
            int apply_id = Convert.ToInt32(Request["apply_id"]);

            ViewBag.apply_id = apply_id;


            tb_nccp_filelist file = nfbll.Single(a => a.id == apply_id);
            tb_nccp_account_info account_info = naibll2.Single(a => a.apply_id == apply_id);

            ViewBag.file = file;
            ViewBag.account_info = account_info;

            return View();
        }

        public ActionResult SaveDeductResult()
        {

            int apply_id = Convert.ToInt32(Request["apply_id"]);
            string deduct_result = Request["deduct_result"];
            string deduct_amount = Request["deduct_amount"];
            string deduct_failed_reason = Request["deduct_failed_reason"];
            string remark = Request["remark"];


            if (deduct_result == "成功")
            {
                tb_nccp_account_info account_info = naibll2.Single(a => a.apply_id == apply_id);
                account_info.zxzt = "已处理";
                naibll2.Update(account_info);

                tb_nccp_filelist apply = nfbll.Single(a => a.id == apply_id);
                apply.apply_return_result = "无退回";
                nfbll.Update(apply);

            }
            if (deduct_result == "失败")
            {
                tb_nccp_filelist apply = nfbll.Single(a => a.id == apply_id);
                apply.apply_return_result = "全部退回";
                nfbll.Update(apply);

                bool is_add = false;
                tb_nccp_return_info return_info = nribll.Single(a => a.apply_id == apply_id);
                if (return_info == null)
                {
                    is_add = true;
                    return_info = new tb_nccp_return_info();
                }

                return_info.apply_id = apply_id;
                return_info.apply_number = apply.apply_number;
                return_info.datetime = DateTime.Now;
                return_info.reason = deduct_failed_reason;
                return_info.remark = remark;
                return_info.return_conn = "张三丰";
                return_info.return_conn_phone = "0755-6747671";

                if (is_add)
                {
                    nribll.Add(return_info);
                }
                else
                {
                    nribll.Update(return_info);
                }
            }

            return Json(new { succ = true });
        }


        public ActionResult M_Apply_Add()
        {
            return View();
        }

        [HttpPost, ActionName("M_Apply_Add")]
        public ActionResult M_Apply_Add_Post()
        {
            string upload_path_01 = Request["upload_path_01"];
            string control_type = Request["control_type"];
            string file_name = upload_path_01.Split('/').Last();
            int examid = Convert.ToInt32(Request["examid"]);
            tb_nccp_filelist apply = new tb_nccp_filelist();

            string upload_path_02 = Request["upload_path_02"];
            DataTable dt = OpenCSV(Server.MapPath(upload_path_02), false);
             
            apply.apply_number = Convert.ToString(dt.Rows[1][1]);
            apply.apply_return_result = "无退回";
            apply.control_type = control_type;
            apply.date_time = DateTime.Now;
            apply.exam_id = examid;
            apply.file_name = file_name;
            apply.download_path = upload_path_01;

            nfbll.Add(apply);
           

           

            tb_nccp_apply_info apply_info = new tb_nccp_apply_info();
            apply_info.apply_id = apply.id;
            apply_info.apply_number = apply.apply_number;
            apply_info.cbfg = Convert.ToString(dt.Rows[9][1]);
            apply_info.cbfg_phone = Convert.ToString(dt.Rows[10][1]);
            apply_info.cbfggwz_code = Convert.ToString(dt.Rows[13][1]);
            apply_info.cbfggzz_code = Convert.ToString(dt.Rows[12][1]);
            apply_info.cbr_address = Convert.ToString(dt.Rows[15][1]);
            apply_info.ID_type = Convert.ToString(dt.Rows[5][1]);
            apply_info.nature = Convert.ToString(dt.Rows[3][1]);
            apply_info.nccp_man_ID_number = Convert.ToString(dt.Rows[6][1]);
            apply_info.nccp_man_name = Convert.ToString(dt.Rows[4][1]);
            apply_info.remark = Convert.ToString(dt.Rows[16][1]);
            apply_info.type = Convert.ToString(dt.Rows[2][1]);
            apply_info.xzkhtzs_name = Convert.ToString(dt.Rows[14][1]);
            apply_info.zxah = Convert.ToString(dt.Rows[11][1]);
            apply_info.zxfy_code = Convert.ToString(dt.Rows[8][1]);
            apply_info.zxfy_name = Convert.ToString(dt.Rows[7][1]);

            naibll.Add(apply_info);

            tb_nccp_account_info account_info = new tb_nccp_account_info();
            account_info.account = Convert.ToString(dt.Rows[20][1]);
            account_info.account_net_point = Convert.ToString(dt.Rows[1][3]);
            account_info.account_net_point_code = Convert.ToString(dt.Rows[2][3]);
            account_info.account_type = Convert.ToString(dt.Rows[0][3]);
            account_info.apply_id = apply.id;
            account_info.apply_number = apply.apply_number;
            account_info.cdwsh = Convert.ToString(dt.Rows[11][3]);
            account_info.control_measures = Convert.ToString(dt.Rows[19][1]);
            account_info.control_type = Convert.ToString(dt.Rows[18][1]);
            account_info.jrzc_name = Convert.ToString(dt.Rows[3][3]);
            account_info.jrzc_type = Convert.ToString(dt.Rows[4][3]);
            account_info.serial_number = Convert.ToString(dt.Rows[17][1]);
            account_info.sfjh = Convert.ToString(dt.Rows[10][3]);
            account_info.sqkhje = Convert.ToString(dt.Rows[9][3]);
            account_info.sqkhjebz = Convert.ToString(dt.Rows[8][3]);
            account_info.sqkznr = Convert.ToString(dt.Rows[6][3]);
            account_info.unit = Convert.ToString(dt.Rows[5][3]);
            account_info.ydjah = Convert.ToString(dt.Rows[17][3]);
            account_info.ydjfs = "资金（限额）";
            account_info.ydjqqdh = Convert.ToString(dt.Rows[18][3]);
            account_info.ydjxh = Convert.ToString(dt.Rows[19][3]);
            account_info.zxkzhhm = Convert.ToString(dt.Rows[12][3]);
            account_info.zxkzhkhh = Convert.ToString(dt.Rows[13][3]);
            account_info.zxkzhkhhhh = Convert.ToString(dt.Rows[14][3]);
            account_info.zxkzhlx = Convert.ToString(dt.Rows[16][3]);
            account_info.zxkzhzh = Convert.ToString(dt.Rows[15][3]).Replace("'", "");
            account_info.zxzt = "未处理";
            account_info.sqkhsj = dt.Rows[7][3] == DBNull.Value ? Convert.ToDateTime("1970-01-01") : Convert.ToDateTime(dt.Rows[7][3]);
            account_info.download_path = Request["upload_path_03"];

            naibll2.Add(account_info);


            return Json(new { succ = true });
        }
 


        public DataSet ExcelToDS(string Path)
        {
            string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + Path + ";" + "Extended Properties=Excel 8.0;";
            OleDbConnection conn = new OleDbConnection(strConn);
            conn.Open();
            string strExcel = "";
            OleDbDataAdapter myCommand = null;
            DataSet ds = null;
            strExcel = "select * from [sheet1$]";
            myCommand = new OleDbDataAdapter(strExcel, strConn);
            ds = new DataSet();
            myCommand.Fill(ds, "table1");
            return ds;
        }


        /// <summary>
        /// 将CSV文件的数据读取到DataTable中
        /// </summary>
        /// <param name="fileName">CSV文件路径</param>
        ///<param name="HasHead">是否包含头部标题</param>
        /// <returns>返回读取了CSV数据的DataTable</returns>
        public static DataTable OpenCSV(string filePath,bool HasHead)
        {
            Encoding encoding = Common.FileEncoder.GetEncoding(filePath); //Encoding.ASCII;
            DataTable dt = new DataTable();
            FileStream fs = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            //StreamReader sr = new StreamReader(fs, Encoding.ASCII);
            StreamReader sr = new StreamReader(fs, encoding);
            //string fileContent = sr.ReadToEnd();
            //encoding = sr.CurrentEncoding;
            //记录每次读取的一行记录
            string strLine = "";
            //记录每行记录中的各字段内容
            string[] aryLine = null;
            string[] tableHead = null;
            //标示列数
            int columnCount = 0;
            bool IsFirst = true; 
           
            //标示是否是读取的第一行
          
            //逐行读取CSV中的数据
            while ((strLine = sr.ReadLine()) != null)
            {
                //strLine = Common.ConvertStringUTF8(strLine, encoding);
                //strLine = Common.ConvertStringUTF8(strLine);
                if (!HasHead && IsFirst)
                {
                    columnCount = strLine.Split(',').Length;
                    tableHead = new string[columnCount];
                    IsFirst = false; 
                    //创建列
                    for (int i = 0; i < columnCount; i++)
                    {
                        tableHead[i] = i.ToString();
                        DataColumn dc = new DataColumn(i.ToString());
                        dt.Columns.Add(dc);
                    } 
                }

                if (IsFirst == true)
                {
                    tableHead = strLine.Split(',');
                    IsFirst = false;
                    columnCount = tableHead.Length;
                    //创建列
                    for (int i = 0; i < columnCount; i++)
                    {
                        DataColumn dc = new DataColumn(tableHead[i]);
                        dt.Columns.Add(dc);
                    } 
                }
                else
                {
                    aryLine = strLine.Split(',');
                    DataRow dr = dt.NewRow();
                    for (int j = 0; j < columnCount; j++)
                    {
                        dr[j] = aryLine[j];
                    }
                    dt.Rows.Add(dr);
                }
            }
            if (aryLine != null && aryLine.Length > 0)
            {
                dt.DefaultView.Sort = tableHead[0] + " " + "asc";
            }

            sr.Close();
            fs.Close();
            return dt;
        }

    }


}

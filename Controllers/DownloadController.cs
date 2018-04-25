using Bll;
using DBUtility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Model;
using IBll;
using Bll;

namespace Project.Controllers
{
    public class DownloadController : Controller
    {
       
        // GET: /Download/
        CommonBll commonbll = new CommonBll();
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 导出Excel表格
        /// </summary>
        public string Export(string sql,string contestName,string nodeid,string orderby)
        { 
            sql = sql.Replace("{", "'").Replace("}", "'").Replace("*", "+");

            Isys_StructureService sbll = new sys_StructureService(); 
            List<sys_Structure> structure_list = sbll.Query("exec proc_Common_NodeList " + nodeid, new object[] { });

            string ids = string.Empty;
            foreach (sys_Structure item in structure_list)
            {
                ids += "," + item.Id;
            }
            ids = ids.Substring(1);

            sql += " and SubBankId in (" + ids + ") " + orderby;
            DataTable dt = commonbll.GetListParaDatatable(sql);
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    if (dt.Rows[i]["ZhiHangLevel"].ToString() == "3")
            //    {
            //        dt.Rows[i]["ShenJi"] = dt.Rows[i]["XianLian"];
            //        dt.Rows[i]["XianLian"] = dt.Rows[i]["ZhiHang"];
            //        dt.Rows[i]["ZhiHang"] = "-";
            //    }
            //}
            dt.Columns.Remove("ZhiHangLevel");
            string ExcelName = DateTime.Now.ToString("yyyyMMdd")+""+DateTime.Now.Millisecond+contestName+"成绩信息";
            string filename = "/Export/" + ExcelName + ".xlsx";

            OfficeHelper officeHp = new OfficeHelper();
            var Result = officeHp.DtToExcel(dt,contestName+ "成绩信息表",new string[]{"考核名称","专业名称","班级名称","学号","学员名称","试卷总分","考试总分","提交时间"},"成绩信息",ExcelName);
            
            var json = new object[] {
                        new{
                            filename=filename,
                        }
                    };//返回路径
            return JsonConvert.SerializeObject(json);
        }

        public ActionResult ExportSXReport()
        {
            return View();
        }
        [HttpPost,ActionName("ExportSXReport")]
        public ActionResult ExportSXReportPost()
        {
            string oterm = " UType = 2 ";
            string teacherid = Request["teacherid"];
            string conditionClass = Request["conditionClass"];
            if (!string.IsNullOrEmpty(teacherid))
            {
                if (teacherid != "1")
                {//如果不是管理员
                    oterm += " and userID=" + teacherid;
                }
            }
            if (!string.IsNullOrEmpty(conditionClass))
            {
                oterm += " and ClassName='" + conditionClass + "'";
            }
            //string accountNo = Request["accountNo"];
            //if (!string.IsNullOrEmpty(accountNo))
            //{
            //    oterm += "and  AccountNo='" + accountNo + "'";
            //}
            string department = Request["department"];
            if (!string.IsNullOrEmpty(department))
            {
                oterm += " and YuanXiName='" + department + "')";
            }
            string train = Request["train"];
            if (!string.IsNullOrEmpty(train))
            {
                oterm += " and Plan_Name like '%" + train + "%'";
            }
            string stu = Request["stu"];
            if (!string.IsNullOrEmpty(stu))
            {
                oterm += " and ','+StudentNames+',' like '%," + stu + ",%'";
            }

            string sql = @"select GroupName,ClassName,SchoolName,YuanXiName,TeacherName,Plan_Name,avg_score,max_score,ROW_NUMBER() OVER(ORDER BY max_score desc,seconds asc) AS sort 
from ( select *, (select top 1 WhenUsed from view_PraticeScore where ExamId= KHID and ','+ StudentIds + ',' like '%,'+ CAST(TellerId as nvarchar(10)) + ',%' and Result=max_score)  as seconds 
from ( select a.Id as ClassId, a.Name as ClassName, a.ParentId, a.ClassLevel, b.Name as GroupName, b.StudentIds, b.StudentNames, c.Name as YuanXiName,
    d.Name as SchoolName, e.Name as TeacherName, e.UType, e.Id as UserId, f.Plan_Name, 
    (select AVG(Result) from view_PraticeScore where ExamId= f.KHID and ','+ b.StudentIds + ',' like '%,'+ CAST(TellerId as nvarchar(10)) + ',%') as avg_score,
    (select Max(Result) from view_PraticeScore where ExamId= f.KHID and ','+ b.StudentIds + ',' like '%,'+ CAST(TellerId as nvarchar(10)) + ',%') as max_score,  
    f.KHID, f.KH_Type, f.PlanId, f.ContestId from sys_Structure as a 
    inner join sys_Grouping as b on b.StructureId = a.Id inner join sys_Structure as c on c.Id = a.ParentId 
    inner join sys_Structure as d on d.Id = c.ParentId inner join sys_person as e on c.Id = e.SubBankId 
    inner join view_PraticeItems as f on (f.UserId = e.Id and f.KH_Type != 0) or (f.ContestId = a.Id and f.KH_Type = 0) )g)h where " + oterm;
                   
            DataTable dt = commonbll.GetListParaDatatable(sql);
            string ExcelName = DateTime.Now.ToString("yyyyMMdd") + "" + DateTime.Now.Millisecond + "实训报告";
            string filename = "/Export/" + ExcelName + ".xlsx";

            OfficeHelper officeHp = new OfficeHelper();
            var Result = officeHp.DtToExcel(dt, "实训报告", new string[] {"分组名称", "班级名称", "学校名称", "院系名称", "老师名称", "考核名称", "小组平均分", "小组最高分","排名" }, "实训报告", ExcelName);

            if (Result != "SUCCESS")
            {
                return Json(new { succ = false, msg = "导出失败" });
            }
            return Json(new { succ = true, path = filename });
        }


        public ActionResult CourseReport()
        {
            return View();
        }

        [HttpPost,ActionName("CourseReport")]
        public ActionResult CourseReportPost()
        {
            string oterm = "";
            string ostrWhere = "1=1 and isDelete=1 "; 
            string oid = Request["oid"];
            if(!string.IsNullOrEmpty(oid))
            { 
                if (oid != "admin")
                {  
                    ostrWhere += " and UploadName in (select id from sys_person where AccountNo='admin' or AccountNo = '" + oid + "')"; 
                    oterm = "and userid in (select id from sys_person where SubBankId in (select id from sys_Structure where ParentId = (select SubBankId from sys_person where AccountNo='" + oid + "')))";
                }
            }
            string oclass = Request["oclass"];
             
            if (!string.IsNullOrEmpty(oclass))
            {
                ostrWhere += " and CourseClassify=" + oclass;
            }
            string oway = Request["oway"];
            if (!string.IsNullOrEmpty(oway))
            {
                ostrWhere += " and CourseUse=" + oway;
            }
            string oname = Request["oname"];
            if (!string.IsNullOrEmpty(oname))
            {
                ostrWhere += " and CourseName like '%" + oname + "%'";
            }
            string sql = @"select CourseName,
	                       case
                             when CourseClassify = 0 then '行业在线自学课程银行模块'
                             when CourseClassify = 1 then '行业在线自学课程保险模块'
                             when CourseClassify = 2 then '行业在线自学课程理财模块'
                             when CourseClassify = 3 then 'BSI实训银行模块'
                             when CourseClassify = 4 then 'BSI实训保险模块'
                             when CourseClassify = 5 then 'BSI实训理财模块'
                           end as CourseClassify,(select SUM(times) from CourseManagementLearningRecords where course_id = a.Id " + oterm + ") times FROM  ykt_CourseManagement a WHERE " + ostrWhere + "order by a.id";

            DataTable dt = commonbll.GetListParaDatatable(sql);
            string ExcelName = DateTime.Now.ToString("yyyyMMdd") + "" + DateTime.Now.Millisecond + "课程库报表";
            string filename = "/Export/" + ExcelName + ".xlsx";

            OfficeHelper officeHp = new OfficeHelper();
            var Result = officeHp.DtToExcel(dt, "课程库报表", new string[] { "课程名称", "课程分类", "学习次数"}, "课程库报表", ExcelName);

            if (Result != "SUCCESS")
            {
                return Json(new { succ = false, msg = "导出失败" });
            }
            return Json(new { succ = true, path = filename });
        }
    }
}

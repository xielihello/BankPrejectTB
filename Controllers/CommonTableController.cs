using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bll;
using Model;
using System.Data;
using Newtonsoft.Json;
namespace Project.Controllers
{
    public class CommonTableController : BaseController
    {

        CommonBll bll = new CommonBll();
        //
        // GET: /CommonTable/ 
        public ActionResult Index()
        {
           
            return View();
        }

        [HttpPost]
        public string LoadTableData()
        { 
            string sql = string.Empty;
            string table = Request["table"];
            string fields = Request["fields"];
            string where = Request["where"];
            if (!string.IsNullOrEmpty(where))
            {
                where = where.Replace("{", "'").Replace("}", "'").Replace("^","=");   
            }
            string orderby = Request["orderby"];
            int page_index = string.IsNullOrEmpty(Request["page"]) ? 1 : Convert.ToInt32(Request["page"]);
            int page_size = string.IsNullOrEmpty(Request["pagesize"]) ? 10 : Convert.ToInt32(Request["pagesize"]);
            string date_format = string.IsNullOrEmpty(Request["date_format"]) ? "yyyy-MM-dd HH:mm:ss" : Request["date_format"];
            bool is_page = string.IsNullOrEmpty(Request["is_page"]) ? true : Convert.ToBoolean(Request["is_page"]);
            if (!is_page)
            {
                page_index = 1;
                page_size = 1000;
            }


            if (string.IsNullOrEmpty(table))
            {
                return JsonConvert.SerializeObject(new { succ = false, msg = "参数table缺失" });
            }
            if (string.IsNullOrEmpty(fields))
            {
                return JsonConvert.SerializeObject(new { succ = false, msg = "参数fields缺失" });
            }
            if (string.IsNullOrEmpty(orderby))
            {
                return JsonConvert.SerializeObject(new { succ = false, msg = "参数orderby缺失" });
            }

            PageModel page = new PageModel();
            page.IsGetCount = true;
            page.PageIndex = page_index;
            page.PageSize = page_size;
            page.Sort = orderby;
            page.strFld = fields;
            page.strWhere = where;
            page.tab = table;

            int page_count = 0;//总数
            DataTable dt = bll.GetListPages(page, ref page_count);

            DataTable dt2 = UpdateDataTable(dt, date_format);
            
            
            return JsonConvert.SerializeObject(JsonResultPagedLists(page_count, page.PageIndex, page.PageSize, dt2));
        }



        /// <summary>
        /// 修改数据表DataTable某一列的类型和记录值(正确步骤：1.克隆表结构，2.修改列类型，3.修改记录值，4.返回希望的结果)
        /// </summary>
        /// <param name="argDataTable">数据表DataTable</param>
        /// <returns>数据表DataTable</returns>  

        private DataTable UpdateDataTable(DataTable argDataTable,string format)
        {
            DataTable dtResult = new DataTable();
            //克隆表结构
            dtResult = argDataTable.Clone();
            foreach (DataColumn col in dtResult.Columns)
            {
                if (col.DataType.Name == "DateTime")
                {
                    //修改列类型
                    col.DataType = typeof(String);
                }
            }

            //修改记录值

            for (int i = 0; i < argDataTable.Rows.Count; i++)
            {
                  DataRow rowNew = dtResult.NewRow();
                  for (int j = 0; j < argDataTable.Columns.Count; j++)
                  {
                      if (argDataTable.Columns[j].DataType.Name == "DateTime")
                      {
                          rowNew[argDataTable.Columns[j].ColumnName] = Convert.ToDateTime(argDataTable.Rows[i][argDataTable.Columns[j].ColumnName]).ToString(format);
                      }
                      else
                      {
                          rowNew[argDataTable.Columns[j].ColumnName] = argDataTable.Rows[i][argDataTable.Columns[j].ColumnName];
                      }
                  }
                  dtResult.Rows.Add(rowNew);
            }
             
            return dtResult;
        } 

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Model;
using System.Data;
namespace Project.Controllers
{
    public class BaseController:Controller
    {

        /// 分页  
        /// </summary>
        /// <typeparam name="TModel">类型</typeparam>
        /// <param name="total">总共条数</param>
        /// <param name="pageIndex">当前页</param>
        /// <param name="model">IEnumerable<TModel></param>
        /// <returns></returns>
        protected PageListModel JsonResultPagedLists(int total, int pageIndex, int PageSize, DataTable tb)
        {
            PageListModel m = new PageListModel();
            m.Total = total;
            m.PageIndex = pageIndex;
            m.PageTotal = total % PageSize == 0 ? (total / PageSize) : (total / PageSize) + 1;
            m.PageSize = PageSize;
            m.Tb = tb;

            return m;
        }
    }
}
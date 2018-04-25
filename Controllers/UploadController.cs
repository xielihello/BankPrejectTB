using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.OleDb;
using Bll;
using IBll;
using Model;
namespace Project.Controllers
{
    public class UploadController : Controller
    {
        //
        // GET: /Upload/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ImportChar()
        {
            return View();
        }

        public ActionResult Do()
        {
            try
            {
                //解决跨域上传中止问题
                Response.ClearContent();
                Response.AddHeader("Access-Control-Allow-Origin", "*");//为了安全*尽可能改为域名
                Response.AddHeader("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
                Response.AddHeader("Access-Control-Allow-Headers", "content-type");
                Response.AddHeader("Access-Control-Max-Age", "30");


                if (Request.HttpMethod == "OPTIONS")
                {
                    return Json(new { });
                }

                HttpPostedFileBase _upfile = Request.Files["Filedata"];
                int fileSize = _upfile.ContentLength;

                if (_upfile == null)
                {
                    return Json(new { status = 0, msg = "请选择要上传文件！" }, JsonRequestBehavior.AllowGet);
                }
                //UpLoad upFiles = new UpLoad();
                //string msg = upFiles.fileSaveAs(_upfile, _isthumbnail, _iswater);

                //下载路径
                string downloadPath = "/Upload/";
                //物理路径
                string savePath = Server.MapPath("/Upload/");

                if (!string.IsNullOrEmpty(Request["sign"]))
                {
                    savePath = savePath + Request["sign"] + "/";
                    downloadPath = downloadPath + Request["sign"] + "/";
                }

                if (!System.IO.Directory.Exists(savePath))
                {
                    System.IO.Directory.CreateDirectory(savePath);
                }


                //原始文件名
                string sourceFileName = _upfile.FileName;
                //原始文件的扩展名
                string fileExt = sourceFileName.Substring(sourceFileName.LastIndexOf('.') + 1);
                //新生成文件名
                string fileName = string.Format("{0}.{1}", DateTime.Now.ToString("yyyyMMddhhmmssff"), fileExt);

                if (Request["rename"] == "false")
                {
                    //不重命名文件
                    fileName = sourceFileName;
                }

                //保存图片
                _upfile.SaveAs(savePath + fileName);

                string action = Request["do"];
                if (action == "import")
                {
                    //再做区分
                    string sign = Request["sign"];
                    string err_msg = "";
                    if (sign == "import_person")
                    {
                        //导入数据库
                        err_msg = SavePersonToDB(savePath + fileName);
                    }
                    else if (sign == "import_char")
                    {
                        err_msg = SaveCharToDB(savePath + fileName);
                    } 
                    
                    if (!string.IsNullOrEmpty(err_msg))
                    {
                        return Json(new { status = 0, msg = "导入失败！" + err_msg }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { status = 1, msg = "上传文件成功！", name = fileName + "导入成功", size = fileSize, ext = fileExt }, JsonRequestBehavior.AllowGet);
                    } 
                }
                else
                { 
                    return Json(new { status = 1, msg = "上传文件成功！", name = fileName + "上传成功", download = downloadPath + fileName, size = fileSize, ext = fileExt }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(new { status = 0, msg = "上传过程中发生意外错误！" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        public string SavePersonToDB(string Path)
        {
            string err_msg = string.Empty;

            Isys_personService personbll = new sys_personService();
            Isys_StructureService sbll = new sys_StructureService();
            Isys_DepartmentService dbll = new sys_DepartmentService();
            Isys_PositionService pbll = new sys_PositionService();
            Itb_Data_DicService ddbll = new tb_Data_DicService();


            DataSet ds = ExcelToDS(Path, "Sheet1");
            DataTable dt = ds.Tables[0];

            List<sys_person> person_list = new List<sys_person>();

            //读取组织架构数据
            List<sys_Structure> structure_list = sbll.Query(a => true);
            //读取部门数据
            List<sys_Department> department_list = dbll.Query(a => true);
            //读取岗位数据
            List<sys_Position> position_list = pbll.Query(a => true);
            //学历水平
            List<tb_Data_Dic> edu_list = ddbll.Query(a => a.Class_Code == "xuelishuipin");
            bool is_pass = true;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //循环退出条件
                if (dt.Rows[i][0] is DBNull && dt.Rows[i][6] is DBNull && dt.Rows[i][9] is DBNull)
                {
                    //柜员姓名、分行、工号同时为空时断定下面无数据
                    break;
                }

                //第一列
                sys_person person = new sys_person();
                if (dt.Rows[i][0] is DBNull)
                {
                    err_msg = "第" + (i + 1) + "行第1列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                person.Name = Convert.ToString(dt.Rows[i][0]);

                //第二列
                if (dt.Rows[i][1] is DBNull)
                {
                    err_msg = "第" + (i + 1) + "行第2列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                DateTime birth_date = DateTime.Now;
                bool is_date_1 = DateTime.TryParse(Convert.ToString(dt.Rows[i][1]), out birth_date);
                if (!is_date_1)
                {
                    err_msg = "第" + (i + 1) + "行第2列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                person.BirthDate = birth_date;

                //第三列
                if (dt.Rows[i][2] is DBNull)
                {
                    err_msg = "第" + (i + 1) + "行第3列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                person.Sex = Convert.ToString(dt.Rows[i][2]) == "男" ? 1 : 0;

                //第四列
                if (dt.Rows[i][3] is DBNull)
                {
                    err_msg = "第" + (i + 1) + "行第4列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                int age = 0;
                bool is_int_1 = int.TryParse(Convert.ToString(dt.Rows[i][3]), out age);
                if (!is_int_1)
                {
                    err_msg = "第" + (i + 1) + "行第4列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                person.Age = age;

                //第五列
                if (dt.Rows[i][4] is DBNull)
                {
                    err_msg = "第" + (i + 1) + "行第5列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                DateTime join_bank_date = DateTime.Now;
                bool is_date_2 = DateTime.TryParse(Convert.ToString(dt.Rows[i][4]), out join_bank_date);
                if (!is_date_2)
                {
                    err_msg = "第" + (i + 1) + "行第5列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                person.JoinBankDate = join_bank_date;

                //第六列
                if (dt.Rows[i][5] is DBNull)
                {
                    err_msg = "第" + (i + 1) + "行第6列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                DateTime join_center_date = DateTime.Now;
                bool is_date_3 = DateTime.TryParse(Convert.ToString(dt.Rows[i][5]), out join_center_date);
                if (!is_date_3)
                {
                    err_msg = "第" + (i + 1) + "行第6列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                person.JoinCenterDate = join_center_date;

                //第七列
                if (dt.Rows[i][6] is DBNull)
                {
                    err_msg = "第" + (i + 1) + "行第7列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                sys_Structure structure = structure_list.SingleOrDefault(a => a.Name == Convert.ToString(dt.Rows[i][6]));
                if (structure == null|| structure.NodeLevel != 3)
                {
                    err_msg = "第" + (i + 1) + "行第7列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                person.SubBankId = structure.Id;

                //第八列
                if (dt.Rows[i][7] is DBNull)
                {
                    err_msg = "第" + (i + 1) + "行第8列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                sys_Position position = position_list.SingleOrDefault(a => a.Name == Convert.ToString(dt.Rows[i][7]));
                if (position == null)
                {
                    err_msg = "第" + (i + 1) + "行第8列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                person.PositionId = position.Id;

                //第九列
                //if (dt.Rows[i][8] is DBNull)
                //{
                //    err_msg = "第" + (i + 1) + "行第9列数据不符合导入规则，请检查";
                //    is_pass = false;
                //    break;
                //}
                //sys_Department department = department_list.SingleOrDefault(a => a.Name == Convert.ToString(dt.Rows[i][8]));
                //if (department == null)
                //{
                //    err_msg = "第" + (i + 1) + "行第9列数据不符合导入规则，请检查";
                //    is_pass = false;
                //    break;
                //}
                person.DepartmentId =3;
                //第九列
                if (dt.Rows[i][8] is DBNull)
                {
                    err_msg = "第" + (i + 1) + "行第9列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                person.UCode = Convert.ToString(dt.Rows[i][8]);

                //第十一列
                if (dt.Rows[i][9] is DBNull)
                {
                    err_msg = "第" + (i + 1) + "行第10列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                tb_Data_Dic edu = edu_list.SingleOrDefault(a => a.Dic_Name == Convert.ToString(dt.Rows[i][9]));
                if (edu == null)
                {
                    err_msg = "第" + (i + 1) + "行第11列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                person.Education = edu.Id;


                //第十二列
                if (dt.Rows[i][10] is DBNull)
                {
                    err_msg = "第" + (i + 1) + "行第12列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                person.Phone = Convert.ToString(dt.Rows[i][10]);

                person.TelPhone = Convert.ToString(dt.Rows[i][11]);
                person.Email = Convert.ToString(dt.Rows[i][12]);

                //第十五列
                if (dt.Rows[i][13] is DBNull)
                {
                    err_msg = "第" + (i + 1) + "行第14列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                person.UType = GetUType(Convert.ToString(dt.Rows[i][13]));

                person.CreateDate = DateTime.Now;
                person.Creator = 0;
                person.AccountPwd = "21218cca77804d2ba1922c33e0151105";
                person_list.Add(person);

            }

            if (is_pass && person_list.Count > 0)
            {
                personbll.Add(person_list);

                //设置账号名
                List<sys_person> person_new = personbll.Query(a => a.AccountNo == null);
                for (int i = 0; i < person_new.Count; i++)
                {
                    person_new[i].AccountNo = "s" + Convert.ToString(person_new[i].Id).PadLeft(7, '0');
                }

                personbll.Update(person_new);
            }

            return err_msg;
        }

        public string SaveCharToDB(string Path) {
            string err_msg = string.Empty;
            Isys_StructureService sbll = new sys_StructureService();
            Idal_CharacterService cbll = new dal_CharacterService();
            Idal_Character_DetailService cdbll = new dal_Character_DetailService();
            //读取组织架构数据
            List<sys_Structure> structure_list = sbll.Query(a => true);
            DataSet ds = ExcelToDS(Path, "Sheet1");
            DataTable dt = ds.Tables[0];

            List<dal_Character> character_list = new List<dal_Character>();
            bool is_pass = true;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //循环退出条件
                if (dt.Rows[i][0] is DBNull && dt.Rows[i][1] is DBNull)
                { 
                    break;
                }
                dal_Character character = new dal_Character();
                //第一列
                if (dt.Rows[i][0] is DBNull)
                {
                    err_msg = "第" + (i + 1) + "行第1列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                sys_Structure structure = structure_list.SingleOrDefault(a => a.Name == Convert.ToString(dt.Rows[i][0]));
                if (structure == null)
                {
                    err_msg = "第" + (i + 1) + "行第1列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                string StructureIds = "";
                int struc_id = structure.Id;
                List<sys_Structure> children = sbll.Query("exec proc_Common_NodeList " + struc_id, new object[] { });
                foreach (sys_Structure item in children)
                {
                    StructureIds += "," + item.Id;
                }
                StructureIds = StructureIds.Substring(1);

                character.StructureIds = StructureIds; 
                character.CharacterName = Convert.ToString(dt.Rows[i][1]);
                character.AddTime = DateTime.Now;
                character.BankSiteId = 0;
                character.ContestId = 0;
                character.CharacterStatus = 1;//默认未激活
                

                DateTime beginTime = DateTime.Now;
                bool is_date_1 = DateTime.TryParse(Convert.ToString(dt.Rows[i][2]), out beginTime);
                if (!is_date_1)
                {
                    err_msg = "第" + (i + 1) + "行第3列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                character.BeginTime = beginTime;

                DateTime endTime = DateTime.Now;
                bool is_date_2 = DateTime.TryParse(Convert.ToString(dt.Rows[i][3]), out endTime);
                if (!is_date_2)
                {
                    err_msg = "第" + (i + 1) + "行第4列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                character.EndTime = endTime;

                int int_01 = 0;
                bool is_int_01 = int.TryParse(Convert.ToString(dt.Rows[i][4]), out int_01);
                if (!is_int_01)
                {
                    err_msg = "第" + (i + 1) + "行第5列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                character.CharacterLength = int_01;//考核时长

                int int_02 = 0;
                bool is_int_02 = int.TryParse(Convert.ToString(dt.Rows[i][5]), out int_02);
                if (!is_int_02)
                {
                    err_msg = "第" + (i + 1) + "行第6列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                character.TotalMark = int_02;//总分

                int int_03 = 0;
                bool is_int_03 = int.TryParse(Convert.ToString(dt.Rows[i][6]), out int_03);
                if (!is_int_03)
                {
                    err_msg = "第" + (i + 1) + "行第7列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                character.TotalLength = int_03;//字符长度

                string string_01 = Convert.ToString(dt.Rows[i][7]);
                character.IfTimeScore = string_01 == "是" ? 1 : 0;

                int int_04 = 0;
                bool is_int_04 = int.TryParse(Convert.ToString(dt.Rows[i][8]), out int_04);
                if (!is_int_04)
                {
                    err_msg = "第" + (i + 1) + "行第9列数据不符合导入规则，请检查";
                    is_pass = false;
                    break;
                }
                character.RepeatNum = int_04;//默认值

                character_list.Add(character);
            }
            if (is_pass && character_list.Count > 0)
            {
                cbll.Add(character_list);

                var character_id = character_list[0].Id;

                DataSet ds2 = ExcelToDS(Path, "Sheet2");
                DataTable dt2 = ds2.Tables[0];

                List<dal_Character_Detail> character_detail_list = new List<dal_Character_Detail>();

                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    //循环退出条件
                    if (dt2.Rows[i][0] is DBNull)
                    {
                        break;
                    }
                    dal_Character_Detail detail = new dal_Character_Detail();
                    detail.CharacterId = character_id;
                    detail.CharacterChar = Convert.ToString(dt2.Rows[i][0]);

                    character_detail_list.Add(detail);
                }
                if (character_detail_list.Count > 0)
                {
                    cdbll.Add(character_detail_list);
                }
            } 
            return err_msg;
        }

        public DataSet ExcelToDS(string Path,string sheetName)
        {
            string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + Path + ";" + "Extended Properties=Excel 8.0;";
            OleDbConnection conn = new OleDbConnection(strConn);
            conn.Open();
            string strExcel = "";
            OleDbDataAdapter myCommand = null;
            DataSet ds = null;
            strExcel = "select * from [" + sheetName + "$]";
            myCommand = new OleDbDataAdapter(strExcel, strConn);
            ds = new DataSet();
            myCommand.Fill(ds, "table1");
            return ds;
        }


        private int GetUType(string name)
        {
            int type = 1;
            switch (name)
            {
                case "柜员":
                    type = 1;
                    break;
                case "管理员":
                    type = 3;
                    break;
            }
            return type;
        }




    }
}

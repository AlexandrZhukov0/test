using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class EmployeeController : Controller
    {
        [HttpGet]
        public ActionResult search()
        {
            return new helpers.JsonNetResult(
                    helpers.EmployeeList()
            );
        }
        [HttpGet]
        public ActionResult Index(int id)
        {
            return new helpers.JsonNetResult(
                    helpers.EmployeeFromStorage(id)
                );
        }
        [HttpPost]
        public ActionResult update(Employee data)
        {
            if (helpers.NotValidEmployee(data))
            {
                Response.Charset = System.Text.Encoding.UTF8.WebName;
                //так обычно не делаю = передаю сообщение в модели в ответе, и статус там - же, к примеру в этом варианте грабли с кодировкой при настройке из коробки,
                //а еще не понятно как себя поведет "какойто другой" браузер
                //или провайдер - мтс например очень любит менять тело ответа, если статус не 200
                //но говорят что в CRUD неверное  возвращать 200, если действие не выполнено
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, "tnumber + staff");
            }
                

            var saveresult = helpers.SaveEmployeeChanges(data);
            if (saveresult.value != null)
            {
                return new helpers.JsonNetResult(new { });
            }
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest,saveresult.ErrorMessage);
        }

        [HttpPost]
        public ActionResult create(Employee data)
        {
            if (helpers.NotValidEmployee(data))
            {
                Response.Charset = System.Text.Encoding.UTF8.WebName;
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, "tnumber + staff");
            }
            var saveresult = helpers.CreateEmployee(data);
            if (saveresult.value != null)
            {
                return new helpers.JsonNetResult(new
                {
                    id = saveresult.value.id
                } );
            }
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, saveresult.ErrorMessage);
        }

        [HttpGet]
        public ActionResult create()
        {
            var defaultEmployee = new Employee();
            return new helpers.JsonNetResult(
                    defaultEmployee
                    );
        }
        [HttpPost]
        public ActionResult uploadFile()
        {
            var files = Request.Files;
            var fileCount = files.Count;
            string result = "";

            if (fileCount ==0 )
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, "empty file list");

            using (var filestream = new StreamReader(files[0].InputStream))
            {
                using (var filecontent = new Newtonsoft.Json.JsonTextReader(filestream))
                {
                    var ser = new Newtonsoft.Json.JsonSerializer();
                    var ListOfEmployeeForUpload = ser.Deserialize<List<Employee>>(filecontent);
                    result = helpers.MergeEmployee(ListOfEmployeeForUpload);
                }
            }
            return new helpers.JsonNetResult(
                    new
                    {
                        filename = "ResultUpload.txt",
                        result = result
                    }
                    );
        }
    }
}

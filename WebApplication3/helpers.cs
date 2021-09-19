using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebApplication3.Models;

namespace WebApplication3
{
    public class EmployeeChangeResult {
        public Employee value { get; set; }

        public string ErrorMessage { get; set; }
    }
    public class helpers
    {
        public static bool NotValidEmployee(Employee item) {
            if (
                   (item.staff == 0 && item.tnumber != 0)
                || (item.staff != 0 && item.tnumber == 0)
                || (String.IsNullOrWhiteSpace(item.fio))
               )
               return true;
            return false;
        }
        private static bool NotUniqueRecordEmployee(AppDbContext EmployeeContext,Employee item)
        {
            if (item.tnumber == 0)
                return false;

            var recordInDb = EmployeeContext.Employees.FirstOrDefault(r => (r.tnumber == item.tnumber && r.staff == item.staff && r.id != item.id));
            return recordInDb != null;
        }
        public static IEnumerable<Employee> EmployeeList()
        {
            IEnumerable<Employee> employees;

            using (AppDbContext appDb = new AppDbContext())
            {
                employees = appDb.Employees.Select(item => item).ToList();
            }
            return employees;
        }

        public static Employee EmployeeFromStorage(int id)
        {
            Employee employee;

            using (AppDbContext appDb = new AppDbContext())
            {
                employee = appDb.Employees.Where(r => r.id == id).FirstOrDefault();
            }
            return employee;
        }

        public static EmployeeChangeResult SaveEmployeeChanges(Employee item) {
            using (AppDbContext appDb = new AppDbContext())
            {
                
                if (NotUniqueRecordEmployee(appDb,item))
                {
                    return new EmployeeChangeResult()
                    {
                        ErrorMessage =  "double record, require change tabel number"
                    };
                }
                appDb.Employees.Add(item);
                appDb.Entry(item).State = System.Data.Entity.EntityState.Modified;
                appDb.SaveChanges();
            }
            return new EmployeeChangeResult() {
                value = item
            };
        }

        public static EmployeeChangeResult CreateEmployee(Employee item)
        {
            Employee result = null;
            using (AppDbContext appDb = new AppDbContext())
            {
                if (NotUniqueRecordEmployee(appDb,item))
                    return new EmployeeChangeResult()
                    {
                        ErrorMessage = "double record, require change tabel number"
                    };
                
                appDb.Employees.Add(item);
                appDb.SaveChanges();

                result = appDb.Employees.Where(r => r.staff == item.staff && r.tnumber == item.tnumber).FirstOrDefault();
            }
            return new EmployeeChangeResult() { 
                value = result
            }
            ;
        }
        private static object[] ArrayForResult(Employee item)
        {
            return new object[] { 
                item.tnumber,
                item.fio,
                item.bdate,
                item.sex.ToString(),
                item.staff
            };
        }
        public static string MergeEmployee(List<Employee> listOfEmployeeForUpload)
        {
            var TextResult = new StringBuilder();
            using (AppDbContext appDb = new AppDbContext())
            {
                var tmp1 = appDb.Employees.Where(r => r.staff == 1).ToList();
                var ListForMerge =
                    from item in listOfEmployeeForUpload
                    join checkrecord in appDb.Employees.Where(r => r.staff == 1) on item.tnumber equals checkrecord.tnumber into joinresult //для тех у кого "в штате" == 0 табельный номер отустствует
                    from old in joinresult.DefaultIfEmpty()
                    select new
                    {
                        i = item,
                        old = old,
                        item = 
                        //по хорошему клонирование надо выносить в методы сущности, которые обеспечат расширения логики при добавлении полей
                        new Employee() {
                            tnumber = item.tnumber,
                            fio = item.fio,
                            sex = item.sex,
                            bdate = item.bdate,
                            staff = item.staff,
                            id = (old == null) ? 0 : old.id
                        },
                        updateoldRecord = (old != null),
                        badData = NotValidEmployee(item),
                        notmodify =
                             //этот кусочек по хорошему нужно засунуть в equals экземпляров класса
                             old != null &&
                             old.fio == item.fio &&
                             old.sex == item.sex &&
                             old.bdate == item.bdate &&
                             old.staff == item.staff
                    };
                var tmpdata = ListForMerge.ToList();
                foreach (var itemContainer in ListForMerge)
                {
                    if (itemContainer.badData)
                    {
                        //пропускаем те которые не отвечают бизнес требованиям

                        TextResult.AppendLine(String.Format("Ошибка в даных {0} {1} {2} {3} {4}", ArrayForResult(itemContainer.item)));
                        continue;
                    };
                    if (itemContainer.notmodify)
                    {
                        //пропускаем те, которые не будут обновлены
                        TextResult.AppendLine(String.Format("Нет изменений {0} {1} {2} {3} {4}", ArrayForResult(itemContainer.item)));
                        continue;
                    }

                    appDb.Employees.Add(itemContainer.item);
                    if (itemContainer.updateoldRecord)
                    {
                        appDb.Entry(itemContainer.item).State = System.Data.Entity.EntityState.Modified;
                        TextResult.AppendLine(String.Format("Обновлено {0} {1} {2} {3} {4}", ArrayForResult(itemContainer.item)));
                    } else
                    {
                        TextResult.AppendLine(String.Format("Добавлено {0} {1} {2} {3} {4}", ArrayForResult(itemContainer.item)));
                    }
                    
                }
                appDb.SaveChanges();
            }
            return TextResult.ToString();
        }

        private const string _bDateDateFormat = "dd.MM.yyyy";
        public class bDateDateTimeConverter : IsoDateTimeConverter
        {
            public bDateDateTimeConverter()
            {
                base.DateTimeFormat = _bDateDateFormat;
            }
        }


        public class JsonNetResult : System.Web.Mvc.ActionResult
        {
            public System.Text.Encoding ContentEncoding { get; set; }
            public string ContentType { get; set; }
            public object Data { get; set; }

            public JsonSerializerSettings SerializerSettings { get; set; }
            public Formatting Formatting { get; set; }

            public JsonNetResult()
            {
                SerializerSettings = new JsonSerializerSettings();
            }
            public JsonNetResult(object data)
            {
                SerializerSettings = new JsonSerializerSettings();
                Data = data;
            }

            public override void ExecuteResult(ControllerContext context)
            {
                if (context == null)
                    throw new ArgumentNullException("context");

                HttpResponseBase response = context.HttpContext.Response;

                response.ContentType = !string.IsNullOrEmpty(ContentType)
                  ? ContentType
                  : "application/json";

                if (ContentEncoding != null)
                    response.ContentEncoding = ContentEncoding;

                if (Data != null)
                {
                    JsonTextWriter writer = new JsonTextWriter(response.Output) { Formatting = Formatting };

                    JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);
                    serializer.Serialize(writer, Data);

                    writer.Flush();
                }
            }
        }

    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class Employee
    {
        public Employee() {
            staff = 1;
            fio = "";
        }
        public int id { get; set; }
        public int tnumber { get; set; }
        
        
        public string fio { get; set; }

        [Column(TypeName = "tinyint")]
        public EnumSex sex { get; set; }

        [Column(TypeName = "Date")]

        [JsonConverter(typeof(helpers.bDateDateTimeConverter))]
        public DateTime bdate { get; set; }
        public byte staff { get; set; }

    }
    public enum EnumSex:byte {
        Other = 0,
        Man = 1,
        Woman = 2,
    }
}
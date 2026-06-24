using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPE_Project.Models
{
    public class AttendanceRecord
    {
        public string Name { get; set; }       // 사원 이름
        public string Time { get; set; }       // 출퇴근 시간
        public string Status { get; set; }     // 출근/퇴근
        public string PPEStatus { get; set; }  // 안전장비 착용여부
    }
}
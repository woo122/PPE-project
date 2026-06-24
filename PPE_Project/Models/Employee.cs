namespace PPE_Project.Models
{
    public class Employee
    {
        public int Id { get; set; }          // 사원 고유번호
        public string Name { get; set; }      // 이름
        public string Position { get; set; }  // 직급
        public byte[] FaceData { get; set; }   // 얼굴 이미지 데이터
    }
}
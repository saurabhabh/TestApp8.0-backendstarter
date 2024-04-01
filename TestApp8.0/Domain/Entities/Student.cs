using Microsoft.EntityFrameworkCore;

namespace TestApp8._0.Domain.Entities
{
    public class Student : AuditableEntity
    {
        public int Id { get; set; } 
        public string Name { get; set; } = "";
        public int Age { get; set; } = 0;
    }
}

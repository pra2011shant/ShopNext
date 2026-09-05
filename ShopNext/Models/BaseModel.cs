using System;

namespace ShopNext.Models
{
    public abstract class BaseModel
    {
        public string? Remark { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int? CreatedById { get; set; }
        public DateTime? UpdatedDate { get; set; } = DateTime.Now;
        public int? UpdatedById { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}


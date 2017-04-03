using CommonUtil.Entities;

namespace EP.DAL.Entities
{
    public class EPComment
    {
        public virtual int Id { get; set; }
        public virtual string Message { get; set; }
        public virtual Profile Commenter { get; set; }
        public virtual string ResultTagId { get; set; }
        public virtual string DateAdded { get; set; } 
    }
}

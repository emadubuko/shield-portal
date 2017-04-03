using System.Collections.Generic;

namespace EP.DAL.Entities
{
    public class ActivityResult
    {
        public virtual int Id { get; set; }
        public ResultType ResultType { get; set; }
        public string Content { get; set; }
        public List<EPComment> Comments { get; set; }
    }

    public enum ResultType
    {
        Narrative, ImageUpload, DocumentUpload
    }
}

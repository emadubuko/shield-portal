using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Comment
    {
        public virtual int Id { get; set; }
        public virtual string Message { get; set; }
        public virtual string Commenter { get; set; }
        public virtual string TagName { get; set; }
        public virtual string DateAdded { get; set; }
      //  public virtual string DMPDocumentId { get; set; }
        public virtual DMPDocument DMPDocument { get; set; }
    }
}

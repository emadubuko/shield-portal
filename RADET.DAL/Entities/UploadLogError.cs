using System.Collections.Generic;

namespace RADET.DAL.Entities
{
    public class UploadLogError
    {
        public virtual int Id { get; set; }
        public virtual RadetUpload RadetUpload { get; set; }
        public virtual List<ErrorDetails> ErrorDetails { get; set; }
        public virtual NotificationStatus Status { get; set; }
    }

    public class ErrorDetails
    {
        public virtual string FileName { get; set; }
        public virtual string FileTab { get; set; }
        public virtual string ErrorMessage { get; set; }
        public virtual string LineNo { get; set; }
        public virtual string PatientNo { get; set; }
      
    }

    public enum NotificationStatus
    {
        Logged = 0,
        MailSent = 1,
        MailFailed = 2
    }
}

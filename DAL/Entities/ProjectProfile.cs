namespace DAL.Entities
{
    public class ProjectProfile 
    {
        public virtual ProjectDetails ProjectDetails { get; set; }
        public virtual EthicsApproval EthicalApproval { get; set; }
               
    }
}

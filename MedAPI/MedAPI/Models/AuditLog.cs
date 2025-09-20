namespace MedAPI.Models
{
    public class AuditLog
    {
        public int AuditLogID { get; set; }         
        public string TableName { get; set; }      
        public int RecordID { get; set; }        
        public string Operation { get; set; }      
        public string OldValue { get; set; }    
        public string NewValue { get; set; }        
        public string ChangedBy { get; set; }     
        public DateTime ChangedAt { get; set; }   
    }
}

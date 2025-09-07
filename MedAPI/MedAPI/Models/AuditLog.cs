namespace MedAPI.Models
{
    public class AuditLog
    {
        public int AuditLogID { get; set; }         // PK
        public string TableName { get; set; }       // Название таблицы
        public int RecordID { get; set; }           // ID изменённой записи
        public string Operation { get; set; }       // Insert/Update/Delete
        public string OldValue { get; set; }        // Старое значение (JSON)
        public string NewValue { get; set; }        // Новое значение (JSON)
        public string ChangedBy { get; set; }       // Кто сделал изменение
        public DateTime ChangedAt { get; set; }     // Когда
    }
}

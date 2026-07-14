namespace DocumentsAPI.Models;

public class MedicalResultBody
{
    public string Complaints { get; set; } = string.Empty;
    public string Conclusion { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
}
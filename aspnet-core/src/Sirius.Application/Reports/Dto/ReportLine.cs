namespace Sirius.Dashboard.Dto
{
    public class ReportLine
    {
        public ReportLine(string description, string amount)
        {
            Description = description;
            Amount = amount;
        }

        public string Description { get; set; }
        public string Amount { get; set; }
    }
}
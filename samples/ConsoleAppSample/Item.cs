namespace ConsoleAppSample
{
    public class Item
    {
        public string Id { get; set; }
        public string Division { get; set; }
        public string CompanyId { get; set; }
        public string EmployeeId { get; set; }

        public Item()
        {
        }

        public Item(string id, string division, string companyId, string employeeId)
        {
            Id = id;
            Division = division;
            CompanyId = companyId;
            EmployeeId = employeeId;
        }
    }
}
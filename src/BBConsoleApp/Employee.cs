namespace BBConsoleApp
{
    internal class Employee
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Address { get; set; }

        public override string ToString()
        {
            return $"Id: {EmployeeId}\nName: {EmployeeName}\nAddress: {Address}";
        }
    }
}
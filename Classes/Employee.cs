namespace NET_9_Business_App.Classes
{
    //create sample employee class and create a constructor for the basic employee using params
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string EmployeeFirstName { get; set; }
        public string EmployeeLastName { get; set; }
        public string EmployeePosition { get; set; }
        public double EmployeeSalary { get; set; }

        public Employee(int employeeId, string employeeFirstName, string employeeLastName, string employeePosition, double employeeSalary)
        {
            EmployeeId = employeeId;
            EmployeeFirstName = employeeFirstName;
            EmployeeLastName = employeeLastName;
            EmployeePosition = employeePosition;
            EmployeeSalary = employeeSalary;
        }
    }//end of Employee class

    // Repository to hold all employees
    static class EmployeesRepository
    {
        private static List<Employee> employees = new List<Employee>
    {
        new Employee(1,"Ozzy","Osbourne", "Membranophone Specialist", 500000),
        new Employee(2,"Tony", "Iommi", "Guitar Player", 500000),
        new Employee(3,"Geezer", "Butler", "Bass Player", 500000),
        new Employee(4,"Bill", "Ward", "Bongos", 500000),
    };

        //get a list of employees
        public static List<Employee> GetEmployees() => employees;

        public static Employee? GetEmployeeById(int employeeId)
        {
            return employees.FirstOrDefault(emp => emp.EmployeeId == employeeId);
        }

        //add an employee
        public static void AddEmployee(Employee? employee)
        {
            if (employee is not null)
            {

                employees.Add(employee);

            }
        }//end AddEmployee

        //update an employee
        public static bool UpdateEmployee(Employee? employee)
        {
            if (employee is not null)//vlidate Employee object passed in
            {
                var emp = employees.FirstOrDefault(x => x.EmployeeId == employee.EmployeeId);//check if existing

                if (emp is not null)//if exists, update with employee param
                {
                    emp.EmployeeId = employee.EmployeeId;
                    emp.EmployeeFirstName = employee.EmployeeFirstName;
                    emp.EmployeeLastName = employee.EmployeeLastName;
                    emp.EmployeePosition = employee.EmployeePosition;
                    emp.EmployeeSalary = employee.EmployeeSalary;

                    return true;
                }

            }
            return false;//returns as false if no employee existed in db
        }//end UpdateEmployee

        //delete an employee
        public static bool DeleteEmployee(int employeeId)
        {
            var employee = employees.FirstOrDefault(emp => emp.EmployeeId == employeeId); //get employee based on param
            if (employee != null)//check if employee is valid
            {
                employees.Remove(employee);
                return true;
            }
            return false;//else if not employee found return false to trigger http error 404
        }
    }//end EmployeesRepository class and its CRUD operations
}

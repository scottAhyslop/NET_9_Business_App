
//initializes the web application and creates a web host
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Run(async (HttpContext context) =>
{
    if (context.Request.Method =="GET")
    {
        if (context.Request.Path.StartsWithSegments("/"))
        {
            await context.Response.WriteAsync($"The method is: {context.Request.Method}\r\n");
            await context.Response.WriteAsync($"The URL is: {context.Request.Path}\r\n");

            await context.Response.WriteAsync($"\r\nHeaders: \r\n");
            foreach (var key in context.Request.Headers.Keys)
            {
                await context.Response.WriteAsync($"{key}: {context.Request.Headers[key]}\r\n");
            }
        }
        else if (context.Request.Path.StartsWithSegments("/employees"))
        {
            var employees = EmployeesRepository.GetEmployees();

            await context.Response.WriteAsync($"\r\nEmployee List: \r\n\n");
            foreach (var employee in employees)
            {
                await context.Response.WriteAsync($"{employee.EmployeeFirstName} {employee.EmployeeLastName}: \t{employee.EmployeePosition}\r\n");
            }
        }
    }
    else if (context.Request.Method == "POST")
    {
        if (context.Request.Path.StartsWithSegments("/employees"))
        {
           /* var employee = new Employee(5, "Ronnie James", "Dio", "Membranophone Experimentalist", 500000);
            await context.Response.WriteAsync($"Employee, {employee.EmployeeFirstName} {employee.EmployeeLastName}, added to the list");*/

            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            var employee = JsonSerializer.Deserialize<Employee>(body);

            EmployeesRepository.AddEmployee(employee);
        }
    }
    else if (context.Request.Method == "PUT")
    {
        if (context.Request.Path.StartsWithSegments("/employees"))
        {
            /* var employee = new Employee(5, "Ronnie James", "Dio", "Membranophone Experimentalist", 500000);
             await context.Response.WriteAsync($"Employee, {employee.EmployeeFirstName} {employee.EmployeeLastName}, added to the list");*/

            //get a list of employees
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            var employee = JsonSerializer.Deserialize<Employee>(body);

            //Update employee with "PUT" info from request
            var result = EmployeesRepository.UpdateEmployee(employee);
            if (result)
            {
                await context.Response.WriteAsync("Employee updated successfully");
                
            }
            else
            {
                await context.Response.WriteAsync("No employee found");
            }
        }
    }
    else
    {
        context.Response.StatusCode = 405;
        await context.Response.WriteAsync("Method not allowed");
    }
});
app.Run();//runs the application in an infinite loop and starts the Kestrel server to listen for http requests

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
}
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

    //add an employee
    public static void AddEmployee(Employee? employee)
    {
        if (employee is not null) { 

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

}
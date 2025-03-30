
//initializes the web application and creates a web host
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Run(handler: static async (HttpContext context) =>
{
    if (context.Request.Path.StartsWithSegments("/"))//default landing page, currently with test data
    {
        await context.Response.WriteAsync($"The Method is: {context.Request.Method}\r\n");
        await context.Response.WriteAsync($"The URL is: {context.Request.Path}\r\n");
        await context.Response.WriteAsync($"\r\nHeaders: \r\n");
        foreach (var key in context.Request.Headers.Keys)
        {
            await context.Response.WriteAsync($"{key}: {context.Request.Headers[key]}\r\n");
        }

    }//end landing page
    else if (context.Request.Path.StartsWithSegments("/employees"))
    {
        //HTTP Methods
        if (context.Request.Method == "GET")//landing page default, currently shows test data to conmfirm server is running
        {
            var employees = EmployeesRepository.GetEmployees();//get a list of employees

            await context.Response.WriteAsync($"\r\nEmployee List: \r\n\n");
            foreach (var employee in employees)//display each employee in the list
            {
                await context.Response.WriteAsync($"{employee.EmployeeFirstName} {employee.EmployeeLastName}: \t\t{employee.EmployeePosition}\r\n");//display each employee's info
            }
            context.Response.StatusCode = 201;
        }
        else if (context.Request.Method == "POST")//POST method to add an employee to the list
        {
            if (context.Request.Path.StartsWithSegments("/employees"))
            {
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                var employee = JsonSerializer.Deserialize<Employee>(body);

                if (employee is not null)
                {
                    EmployeesRepository.AddEmployee(employee);
                    context.Response.StatusCode = 201;
                    await context.Response.WriteAsync($"Employee: {employee.EmployeeFirstName} {employee.EmployeeLastName} added. Records updated.");
                    
                }
                
            }
        }//end POST
        else if (context.Request.Method == "PUT")//PUT method to update an employee in the list
        {
            if (context.Request.Path.StartsWithSegments("/employees"))
            {
                //get a list of employees
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                var employee = JsonSerializer.Deserialize<Employee>(body);
                //Update employee with "PUT" info from request
                var result = EmployeesRepository.UpdateEmployee(employee);
                if (result)
                {
                    context.Response.StatusCode = 204;
                    await context.Response.WriteAsync("Employee updated successfully");
                    return;
                }
                else
                {
                    await context.Response.WriteAsync("No employee found");
                }
            }
        }//end PUT
        else if (context.Request.Method == "DELETE")//DELETE method to remove an employee from the list
        {
            if (context.Request.Path.StartsWithSegments("/employees"))
            {
                if (context.Request.Query.ContainsKey("EmployeeId"))
                {
                    var id = context.Request.Query["EmployeeId"];
                    if (int.TryParse(id, out int employeeId))
                    {
                        if (context.Request.Headers["Authorization"] == "dredge")//auth check
                        {
                            var employee = EmployeesRepository.DeleteEmployee(employeeId);
                            if (employee)
                            {
                                await context.Response.WriteAsync($"Employee deleted. Records updated.");
                            }
                            else
                            {
                                await context.Response.WriteAsync("Employee not found.  Records unchanged.");
                            }
                        }//end auth check
                        else//if not authorized, tell user
                        {
                            context.Response.StatusCode = 401;
                            await context.Response.WriteAsync("User Unauthorized to delete...");
                        }
                    }
                }
            }
        }//end DELETE 
        else//if any requests are made outside the allowed above (fallback)
        {
            context.Response.StatusCode = 405;
            await context.Response.WriteAsync("Method not allowed");
        }//end fallback.
    }//end employees
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
}//endf Employee class

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

    //delete an employee
    public static bool DeleteEmployee(int employeeId)
    {
        var employee = employees.FirstOrDefault(emp => emp.EmployeeId == employeeId); //get employee based on param
        if (employee is not null)//check if employee is valid
        {
            employees.Remove(employee);
            return true;
        }
        return false;//else if not employee found return false to trigger http error 404
    }
}//end EmployeesRepository class and its CRUD operations
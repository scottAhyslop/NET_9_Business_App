
//initializes the web application and creates a web host
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using NET_9_Business_App.Classes;
using System.Security.AccessControl;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Run(handler: static async (HttpContext context) =>
{
    if (context.Request.Path.StartsWithSegments("/"))//default landing page, currently with test data
    {
        context.Response.Headers["Content-Type"] = "text/html";
        await context.Response.WriteAsync($"The Method is: {context.Request.Method}<br/>");
        await context.Response.WriteAsync($"The URL is: {context.Request.Path}<br/>");
        await context.Response.WriteAsync($"<br/><b>Headers</b>: <br/>");
        await context.Response.WriteAsync($"<ul>");
        foreach (var key in context.Request.Headers.Keys)
        {
            await context.Response.WriteAsync($"<li><b>{key}</b>: {context.Request.Headers[key]}</li>");
        }
        await context.Response.WriteAsync($"</ul>");
    }//end landing page
    else if (context.Request.Path.StartsWithSegments("/employees"))//Employees route
    {
        //HTTP Methods

        //GET
        if (context.Request.Method == "GET")//landing page default, currently shows test data to conmfirm server is running
        { 
            context.Response.Headers["Content-Type"] = "text/html";//set display text to html

            if (context.Request.Query.ContainsKey("EmployeeId"))//if query params contain an Id, search for that employee
            {
                var id = context.Request.Query["EmployeeId"];
                if (int.TryParse(id, out int employeeId))//parse the query param to an int
                {
                    //get a specific employee, from passed in Id
                    if (employeeId != 0)
                    {
                        var employee = EmployeesRepository.GetEmployeeById(employeeId);
                        if (employee is not null)
                        {
                            await context.Response.WriteAsync($"<table>");

                            await context.Response.WriteAsync($"<tr><header><td><b>Name</b>: </td><td> {employee.EmployeeFirstName} {employee.EmployeeLastName}</td></header></tr>");
                            await context.Response.WriteAsync($"<tr><td><b>Position</b>:</td><td>{employee.EmployeePosition}</td></tr>");
                            await context.Response.WriteAsync($"<tr><td><b>Salary</b>:</td><td> {employee.EmployeeSalary}</td></tr>");
                            await context.Response.WriteAsync($"</table>");
                        }
                        else if (employee is null)
                        {
                            context.Response.StatusCode = 404;
                            await context.Response.WriteAsync("<b>Employee not found</b>");
                            //TODO display message for timed period, log fail, return to search by Id screen
                            return;
                        }
                    }
                }
            }//end GetByEmployeeId
            else if (context.Request.Path.StartsWithSegments("/employees"))//else display all employees
            {
                //get all employees
                var employees = EmployeesRepository.GetEmployees();//get a list of employees
                await context.Response.WriteAsync($"<table>");
                await context.Response.WriteAsync($"<tr><header><b>Employee List</b>: </tr></header><br/>");
                await context.Response.WriteAsync($"<tr><header><td><b>Name</b></td><td><b>Position</b><td><b>Salary</b></td></tr></header>");
                foreach (var employee in employees)//display each employee in the list
                {
                    await context.Response.WriteAsync($"<tr><td>{employee.EmployeeFirstName} {employee.EmployeeLastName}:</td><td>{employee.EmployeePosition}</td><td>${employee.EmployeeSalary}</td></tr>");//display each employee's info
                }
                await context.Response.WriteAsync($"</table>");
                context.Response.StatusCode = 201;
            }//end GetEmployees
        }//end GET
        //POST
        else if (context.Request.Method == "POST")//POST method to add an employee to the list
        {
            if (context.Request.Path.StartsWithSegments("/employees"))
            {
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                var employee = JsonSerializer.Deserialize<Employee>(body);
                try
                {
                    if (employee is not null)
                    {
                        EmployeesRepository.AddEmployee(employee);
                        context.Response.StatusCode = 201;
                        await context.Response.WriteAsync($"Employee: {employee.EmployeeFirstName} {employee.EmployeeLastName} added. Records updated.");

                    }
                    else if (employee is null || employee.EmployeeId <= 0)
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("Bad response to your request");
                        //TODO redirect home after displaying timed error message and log error deets
                        return;
                    }
                }
                catch (Exception ex)
                {
                    await context.Response.WriteAsync(ex.ToString());
                    context.Response.StatusCode = 400;
                }//end try/catch                
            }
        }//end POST
        //PUT
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
        //DELETE
        else if (context.Request.Method == "DELETE")//DELETE method to remove an employee from the list
        {
             if (context.Request.Query.ContainsKey("EmployeeId"))
                {
                    var id = context.Request.Query["EmployeeId"];
                    if (int.TryParse(id, out int employeeId))//take in param, attempt to parse into int
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
                                context.Response.StatusCode = 404;//not found
                                await context.Response.WriteAsync("Employee not found.  Records unchanged.");
                            }
                        }//end auth check
                        else//if not authorized, tell user
                        {
                            context.Response.StatusCode = 401;//not authorized
                            await context.Response.WriteAsync("User Unauthorized to delete...");
                        }
                    }
            }
        }//end DELETE 
        //FALLBACK
        else//if any requests are made outside the allowed above (fallback)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Can't be found, sorry...");
            //TODO: should re-direct home after timed display of message 
        }//end fallback.

    }//end Employee route 
    
   
});
app.Run();//runs the application in an infinite loop and starts the Kestrel server to listen for http requests


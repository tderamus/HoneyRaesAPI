using HoneyRaesAPI.Models;

List<Customer> customers = new List<Customer>
{
    new Customer { Id = 1, Name = "Oshay Jackson", Address = "123 Elm St" },
    new Customer { Id = 2, Name = "Calvin Brodus", Address = "456 Oak St" },
    new Customer { Id = 3, Name = "Greg Street", Address = "789 Pine St" }
};

List<Employee> employees = new List<Employee>
{
    new Employee { Id = 1, Name = "Brodie Quinn", Specialty = "Electrical" },
    new Employee { Id = 2, Name = "Orlando Smith", Specialty = "Plumbing" },
    new Employee { Id = 3, Name = "Jasmine Green", Specialty = "HVAC" },
    new Employee { Id = 4, Name = "Jackie Brown", Specialty = "Drywall" },
    new Employee { Id = 5, Name = "Morgan White", Specialty = "Painting" },
    new Employee { Id = 6, Name = "Terry Black", Specialty = "Carpentry" }
};

List<ServiceTicket> serviceTickets = new List<ServiceTicket>
{
    new ServiceTicket { Id = 1, CustomerId = 1, Description = "My AC is broken", Emergency = true, DateCompleted = new DateTime(2022, 7, 1) },
    new ServiceTicket { Id = 2, CustomerId = 2, EmployeeId = 2, Description = "My toilet is clogged", Emergency = false, DateCompleted = new DateTime(2021, 5, 2) },
    new ServiceTicket { Id = 3, CustomerId = 3, EmployeeId = 1, Description = "My lights won't turn on", Emergency = true, DateCompleted = new DateTime(2024, 6, 3) },
    new ServiceTicket { Id = 4, CustomerId = 1, Description = "My AC is broken", Emergency = true },
    new ServiceTicket { Id = 5, CustomerId = 2, EmployeeId = 2, Description = "My toilet is clogged", Emergency = false },
    new ServiceTicket { Id = 6, CustomerId = 3, Description = "My lights won't turn on", Emergency = true, },
    new ServiceTicket { Id = 7, CustomerId = 3, Description = "Stain in front carpet", Emergency = false, },
    new ServiceTicket { Id = 8, CustomerId = 1, Description = "My lights won't turn on", Emergency = true, },
    new ServiceTicket { Id = 8, CustomerId = 1, Description = "Stain in front carpet", Emergency = false, },
    new ServiceTicket { Id = 10, CustomerId = 2, Description = "My lights won't turn on", Emergency = true, },
    new ServiceTicket { Id = 11, CustomerId = 2, Description = "Window is broken", Emergency = true, DateCompleted = DateTime.MinValue},
};


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Get all service tickets
app.MapGet("/servicetickets", () =>
{
    return serviceTickets;
});

// Get service ticket by id
app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTicket.Employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    serviceTicket.Customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);
    return Results.Ok(serviceTicket);
});

// Delete a service ticket
app.MapDelete("servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTickets.Remove(serviceTicket);
    return Results.NoContent();
});

// Get all employees
app.MapGet("/employees", () =>
{
    return employees;
});

// Get employee by id
app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    employee.ServiceTickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(employee);
});

// Get all customers
app.MapGet("/customers", () =>
{
    return customers;
});

// Get customer by id
app.MapGet("customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    { 
        return Results.NotFound(); 
    }
    customer.ServiceTickets = serviceTickets.Where(st => st.CustomerId == id).ToList();
    return Results.Ok(customer);
});

// Enter service ticket
app.MapPost("serviceTickets", (ServiceTicket serviceTicket) =>
{
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return  serviceTicket;
});

// Update a service ticket
app.MapPut("servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    //the id in the request route doesn't match the id from the ticket in the request body. That's a bad request!
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    serviceTickets[ticketIndex] = serviceTicket;
    return Results.Ok();
});

// Post a time stamp for a service ticket completion
app.MapPost("serviceTickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (ticketToComplete == null)
    {
        return Results.NotFound();
    }
    ticketToComplete.DateCompleted = DateTime.Now;
    return Results.Ok(ticketToComplete);
});


// Return all emergency and incomplete service tickets
app.MapGet("serviceTickets/emergency", () =>
{
    return serviceTickets.Where(st => st.Emergency == true && st.DateCompleted == DateTime.MinValue );
});

// Return all service tickets that are not assigned to an employee
app.MapGet("serviceTickets/unassigned", () =>
{
    return serviceTickets.Where(st => st.EmployeeId == null);
});

// Return all customers with service tickets not closed in over a year
app.MapGet("customers/overdue", () =>
{
    return customers.Where(c => serviceTickets.Any(st => st.CustomerId == c.Id && st.DateCompleted == DateTime.MinValue && DateTime.Now.Year - st.DateCompleted.Year > 1));
});

app.Run();



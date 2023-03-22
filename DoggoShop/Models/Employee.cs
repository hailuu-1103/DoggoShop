using System;
using System.Collections.Generic;

namespace DoggoShopAPI.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public int? DepartmentId { get; set; }

    public string? Title { get; set; }

    public string? TitleOfCourtesy { get; set; }

    public DateTime? BirthDate { get; set; }

    public DateTime? HireDate { get; set; }

    public string? Address { get; set; }

    public bool Active { get; set; }

    public virtual ICollection<Account> Accounts { get; } = new List<Account>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<Order> Orders { get; } = new List<Order>();
}

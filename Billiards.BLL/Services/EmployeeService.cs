using Billiards.DAL.Models;
using Billiards.DAL.Repositories;

namespace Billiards.BLL.Services;

public class EmployeeService
{
    private readonly EmployeeRepository _employeeRepository;

    public EmployeeService()
    {
        _employeeRepository = new EmployeeRepository();
    }

    public EmployeeService(EmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    // Employee methods
    public List<Employee> GetAllEmployees()
    {
        return _employeeRepository.GetAll();
    }

    public Employee? GetEmployeeById(int employeeId)
    {
        return _employeeRepository.GetById(employeeId);
    }

    public void AddEmployee(Employee employee)
    {
        _employeeRepository.Add(employee);
    }

    public void UpdateEmployee(Employee employee)
    {
        _employeeRepository.Update(employee);
    }

    public void DeleteEmployee(int employeeId)
    {
        _employeeRepository.Delete(employeeId);
    }

    // Account methods
    public List<Account> GetAllAccounts()
    {
        return _employeeRepository.GetAllAccounts();
    }

    public Account? GetAccountByUsername(string username)
    {
        return _employeeRepository.GetAccountByUsername(username);
    }

    public Account? GetAccountByEmployeeId(int employeeId)
    {
        return _employeeRepository.GetAccountByEmployeeId(employeeId);
    }

    public void AddAccount(Account account)
    {
        _employeeRepository.AddAccount(account);
    }

    public void UpdateAccount(Account account)
    {
        _employeeRepository.UpdateAccount(account);
    }

    public void DeleteAccount(string username)
    {
        _employeeRepository.DeleteAccount(username);
    }
}


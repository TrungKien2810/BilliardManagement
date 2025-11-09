using Billiards.DAL.Models;
using Billiards.DAL.Repositories;

namespace Billiards.BLL.Services;

public class CustomerService
{
    private readonly CustomerRepository _customerRepository;

    public CustomerService()
    {
        _customerRepository = new CustomerRepository();
    }

    public CustomerService(CustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public List<Customer> GetAllCustomers()
    {
        return _customerRepository.GetAll();
    }

    public Customer? GetCustomerById(int customerId)
    {
        return _customerRepository.GetById(customerId);
    }

    public Customer? GetCustomerByPhoneNumber(string phoneNumber)
    {
        return _customerRepository.GetByPhoneNumber(phoneNumber);
    }

    public void AddCustomer(Customer customer)
    {
        _customerRepository.Add(customer);
    }

    public void UpdateCustomer(Customer customer)
    {
        _customerRepository.Update(customer);
    }

    public void DeleteCustomer(int customerId)
    {
        _customerRepository.Delete(customerId);
    }
}



using DriveNow.Domain.Entities;

namespace DriveNow.Application.Abstractions
{
    public interface ICustomerRepository
    {

        Customer? GetById(int id);

        List<Customer> GetAll();

        void Update(Customer customer);

    }
}

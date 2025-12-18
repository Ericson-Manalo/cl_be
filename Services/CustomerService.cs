using cl_be.Models;
using cl_be.Models.Dto.AddressDto;
using cl_be.Models.Dto.CustomerDto;
using cl_be.Models.Dto.OrderDto;
using cl_be.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using cl_be.Models.Pagination;

namespace cl_be.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AdventureWorksLt2019Context _context;

        public CustomerService(AdventureWorksLt2019Context context)
        {
            _context = context;
        }

        public async Task<CustomerProfileDto> GetCustomerProfileAsync(int customerId)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
                throw new KeyNotFoundException("Customer not found");

            var addresses = await _context.CustomerAddresses
                .Where(ca => ca.CustomerId == customerId)
                .Include(ca => ca.Address)
                .Select(ca => new AddressCustomerDto
                {
                    AddressId = ca.Address.AddressId,
                    AddressLine1 = ca.Address.AddressLine1,
                    City = ca.Address.City,
                    PostalCode = ca.Address.PostalCode
                })
                .ToListAsync();

            var orders = await _context.SalesOrderHeaders
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderCustomerDto
                {
                    SalesOrderId = o.SalesOrderId,
                    SalesOrderNumber = o.SalesOrderNumber!,
                    OrderDate = o.OrderDate,
                    TotalDue = o.TotalDue,
                    Status = o.Status,
                    StatusDescription = ""
                })
                .ToListAsync();

            foreach (var o in orders)
            {
                o.StatusDescription = o.Status switch
                {
                    1 => "In Process",
                    2 => "Approved",
                    3 => "Backordered",
                    4 => "Cancelled",
                    5 => "Shipped",
                    6 => "Closed",
                    _ => "Unknown"
                };
            }

            return new CustomerProfileDto
            {
                CustomerId = customer.CustomerId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                EmailAddress = customer.EmailAddress,
                Phone = customer.Phone,
                Addresses = addresses,
                Orders = orders
            };
        }

        public async Task UpdateCustomerAsync(int customerId, CustomerUpdateDto dto)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
                throw new KeyNotFoundException("Customer not found");

            customer.FirstName = dto.FirstName;
            customer.LastName = dto.LastName;
            customer.EmailAddress = dto.EmailAddress;
            customer.Phone = dto.Phone;
            customer.ModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }



        //admin
        public async Task<List<CustomerListDto>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .OrderBy(c => c.CustomerId)
                .Select(c => new CustomerListDto
                {
                    CustomerId = c.CustomerId,
                    FirstName = c.FirstName,
                    LastName = c.LastName
                })
                .ToListAsync();
        }


        //admin
        public async Task<Page<CustomerListDto>> GetCustomersPagedAsync(int page, int pageSize, string? search = null)
        {
            var query = _context.Customers.AsNoTracking(); // AsNoTracking migliora le performance in lettura

            if (!string.IsNullOrWhiteSpace(search))
            {
                // Rimuoviamo ToLower() per testare se il DB gestisce l'insensibilità di default
                query = query.Where(c => c.FirstName.Contains(search) || c.LastName.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .OrderBy(c => c.CustomerId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CustomerListDto
                {
                    CustomerId = c.CustomerId,
                    FirstName = c.FirstName,
                    LastName = c.LastName
                })
                .ToListAsync();

            return new Page<CustomerListDto>
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Items = items
            };
        }


        public async Task<CustomerDetailDto> GetCustomerDetailAsync(int customerId)
        {
            var customer = await _context.Customers
                .Include(c => c.CustomerAddresses)
                    .ThenInclude(ca => ca.Address)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
                throw new KeyNotFoundException("Customer not found");

            return new CustomerDetailDto
            {
                CustomerId = customer.CustomerId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                EmailAddress = customer.EmailAddress,
                Phone = customer.Phone,
                Addresses = customer.CustomerAddresses
                    .Select(ca => new AddressCustomerDto
                    {
                        AddressId = ca.Address.AddressId,
                        AddressLine1 = ca.Address.AddressLine1,
                        City = ca.Address.City,
                        PostalCode = ca.Address.PostalCode
                    })
                    .ToList()
            };
        }

        public async Task DeleteCustomerAsync(int customerId)
        {
            // 1. Cerchiamo il cliente
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            // 2. Se non esiste, lanciamo un'eccezione
            if (customer == null)
                throw new KeyNotFoundException("Customer not found");

            // 3. Rimuoviamo il cliente dal contesto
            // Motivazione: Entity Framework segnerà lo stato come 'Deleted'
            _context.Customers.Remove(customer);

            // 4. Salviamo le modifiche nel database
            // Motivazione: Solo ora l'operazione di cancellazione viene eseguita fisicamente sul DB
            await _context.SaveChangesAsync();
        }
    }
}

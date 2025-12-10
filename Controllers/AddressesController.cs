using cl_be.Models;
using cl_be.Models.Dto.AddressDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cl_be.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressesController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;

        public AddressesController(AdventureWorksLt2019Context context)
        {
            _context = context;
        }

        //recupero l'id dal jwt
        private int GetCustomerId()
        {
            var claim = User.FindFirst("CustomerId")?.Value;
            if (claim == null)
                throw new UnauthorizedAccessException();

            return int.Parse(claim);
        }


        //recupera gli indirizzi del customer autenticato
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AddressCustomerDto>>> GetMyAddresses()
        {
            int customerId = GetCustomerId();

            var addresses = await _context.CustomerAddresses
                .Where(ca => ca.CustomerId == customerId)
                .Include(ca => ca.Address)
                .Select(ca => new AddressCustomerDto
                {
                    AddressId = ca.Address.AddressId,
                    AddressLine1 = ca.Address.AddressLine1,
                    City = ca.Address.City,
                    PostalCode = ca.Address.PostalCode,
                    StateProvince = ca.Address.StateProvince,
                    CountryRegion = ca.Address.CountryRegion,
                    AddressType = ca.AddressType
                })
                .ToListAsync();

            return Ok(addresses);
        }

        //recupera un indirizzo specifico del customer autenticato
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<AddressCustomerDto>> GetAddressById(int id)
        {
            int customerId = GetCustomerId();

            var address = await _context.CustomerAddresses
                .Where(ca => ca.CustomerId == customerId && ca.AddressId == id)
                .Include(ca => ca.Address)
                .Select(ca => new AddressCustomerDto
                {
                    AddressId = ca.Address.AddressId,
                    AddressLine1 = ca.Address.AddressLine1,
                    City = ca.Address.City,
                    PostalCode = ca.Address.PostalCode,
                    StateProvince = ca.Address.StateProvince,
                    CountryRegion = ca.Address.CountryRegion,
                    AddressType = ca.AddressType
                })
                .FirstOrDefaultAsync();

            if (address == null)
                return NotFound("Address not found or not owned.");

            return Ok(address);
        }

        // crea un nuovo indirizzo per il customer autenticato
        // POST api/address
        // =======================================
        [Authorize]

        [HttpPost]
        public async Task<ActionResult> AddAddress(AddressCreateDto dto)
        {
            int customerId = GetCustomerId();

            var address = new Address
            {
                AddressLine1 = dto.AddressLine1,
                City = dto.City,
                CountryRegion = dto.CountryRegion,
                StateProvince = dto.StateProvince,
                PostalCode = dto.PostalCode,
                ModifiedDate = DateTime.Now,
                Rowguid = Guid.NewGuid()
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            var link = new CustomerAddress
            {
                CustomerId = customerId,
                AddressId = address.AddressId,
                AddressType = string.IsNullOrWhiteSpace(dto.AddressType) ? "Home" : dto.AddressType,
                ModifiedDate = DateTime.Now,
                Rowguid = Guid.NewGuid()
            };

            _context.CustomerAddresses.Add(link);
            await _context.SaveChangesAsync();

            return Ok(new { AddressId = address.AddressId });
        }


        //modifica un indirizzo esistente del customer autenticato
        // =======================================
        // PUT api/address/{id}
        // =======================================
        [Authorize]

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAddress(int id, AddressUpdateDto dto)
        {

            int customerId = GetCustomerId();

            var customerAddress = await _context.CustomerAddresses
                .Include(ca => ca.Address)
                .FirstOrDefaultAsync(ca => ca.CustomerId == customerId && ca.AddressId == id);

            if (customerAddress == null)
                return NotFound("Address not found or not owned.");

            customerAddress.Address.AddressLine1 = dto.AddressLine1;
            customerAddress.Address.City = dto.City;
            customerAddress.Address.PostalCode = dto.PostalCode;
            customerAddress.Address.StateProvince = dto.StateProvince;
            customerAddress.Address.CountryRegion = dto.CountryRegion;
            customerAddress.Address.ModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        //elimina un indirizzo esistente del customer autenticato
        // =======================================
        // DELETE api/address/{id}
        // =======================================
        [Authorize]

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAddress(int id)
        {
            int customerId = GetCustomerId();

            var customerAddress = await _context.CustomerAddresses
                .FirstOrDefaultAsync(ca => ca.CustomerId == customerId && ca.AddressId == id);

            if (customerAddress == null)
                return NotFound("Address not found or not owned.");

            _context.CustomerAddresses.Remove(customerAddress);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    
    }
}

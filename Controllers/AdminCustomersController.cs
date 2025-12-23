using cl_be.Models;
using cl_be.Models.Dto.CustomerDto;
using cl_be.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cl_be.Models.Pagination;

namespace cl_be.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/customers")]
    [ApiController]
    public class AdminCustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public AdminCustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet("{customerId}")]
        public async Task<ActionResult<CustomerDetailDto>> GetCustomer(int customerId)
        {
            var dto = await _customerService.GetCustomerDetailAsync(customerId);
            return Ok(dto);
        }

        [HttpPut("{customerId}")]
        public async Task<IActionResult> UpdateCustomer(int customerId, CustomerUpdateDto dto)
        {
            await _customerService.UpdateCustomerAsync(customerId, dto);
            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<Page<CustomerListDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? search = null)
        {
            var result = await _customerService.GetCustomersPagedAsync(page, pageSize, search);
            return Ok(result);
        }

        [HttpDelete("{customerId}")]
        // Motivazione: [HttpDelete("{id}")] permette di chiamare l'URL /api/admin/customers/30044. 
        // La parte {id} mappa automaticamente il numero dell'URL alla variabile 'int id' del metodo.
        public async Task<IActionResult> Delete(int customerId)
        {
            try
            {
                // Motivazione: Chiamiamo il servizio che abbiamo appena aggiornato per rimuovere il record dal database.
                await _customerService.DeleteCustomerAsync(customerId);

                // Motivazione: Restituiamo 'NoContent' (Status 204) perché l'operazione è riuscita 
                // ma non abbiamo dati da restituire indietro, confermando che la risorsa non esiste più.
                return NoContent();
            }
            catch (Exception ex)
            {
                // Motivazione: Se ci sono vincoli nel database (es. il cliente ha degli ordini), 
                // l'eliminazione fallirà. Restituiamo l'errore per farlo vedere all'admin.
                return BadRequest(new { message = "Errore durante l'eliminazione: " + ex.Message });
            }
        }

    }
}

using cl_be.Models;
using cl_be.Models.Dto.AddressDto;
using cl_be.Models.Dto.CustomerDto;
using cl_be.Models.Dto.OrderDto;
using cl_be.Models.Pagination;
using cl_be.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace cl_be.Services
{
    public class OrderService : IOrderService
    {
        private readonly AdventureWorksLt2019Context _context;

        public OrderService(AdventureWorksLt2019Context context)
        {
            _context = context;
        }

        public async Task<List<OrderListDto>> GetAllOrdersAsync()
        {
            var orders = _context.SalesOrderHeaders
                .OrderBy(so => so.OrderDate)
                .Include(so => so.Customer)
                .Select(so => new OrderListDto {
                    SalesOrderId = so.SalesOrderId,
                    OrderDate = so.OrderDate.ToString("dd/MM/yyyy"),
                    CustomerId = so.CustomerId,
                    CustomerName = so.Customer.FirstName + " " + so.Customer.LastName,
                    TotalDue = Math.Round(so.TotalDue, 2)
                }).ToListAsync();

            return await orders;
        }


        public async Task<Page<OrderListDto>> GetOrdersPagedAsync(int page, int pageSize, string? search = null)
        {
            var query = _context.SalesOrderHeaders.AsNoTracking(); // AsNoTracking migliora le performance in lettura

            if (!string.IsNullOrWhiteSpace(search))
            {
                // Rimuoviamo ToLower() per testare se il DB gestisce l'insensibilità di default
                query = query.Where(o => o.Customer.FirstName.Contains(search) || o.Customer.LastName.Contains(search) || o.SalesOrderId.ToString().Contains(search)) ;
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderListDto
                {
                    SalesOrderId = o.SalesOrderId,
                    OrderDate = o.OrderDate.ToString("dd/MM/yyyy"),
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer.FirstName + " " + o.Customer.LastName,
                    TotalDue = Math.Round(o.TotalDue, 2)
                })
                .ToListAsync();

            return new Page<OrderListDto>
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Items = items
            };
        }

        public async Task<OrderDetailDto?> GetDetailOrder(int orderId)
        {
            // 1. Recupero la testata dell'ordine con gli indirizzi
            var order = await _context.SalesOrderHeaders
                .Include(o => o.BillToAddress)
                .Include(o => o.ShipToAddress)
                .FirstOrDefaultAsync(o => o.SalesOrderId == orderId);

            if (order == null) return null;

            // 2. Recupero i dettagli (righe ordine) e i relativi prodotti
            var items = await _context.SalesOrderDetails
                .Where(i => i.SalesOrderId == orderId)
                .Include(i => i.Product)
                .Select(i => new OrderDetailItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product!.Name,
                    OrderQty = i.OrderQty,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.LineTotal
                })
                .ToListAsync();

            // 3. Mappatura nel DTO
            return new OrderDetailDto
            {
                SalesOrderId = order.SalesOrderId,
                SalesOrderNumber = order.SalesOrderNumber!,
                OrderDate = order.OrderDate,
                ShipDate = order.ShipDate,
                Status = order.Status,
                ShipMethod = order.ShipMethod!,
                SubTotal = order.SubTotal,
                TaxAmt = order.TaxAmt,
                Freight = order.Freight,
                TotalDue = order.TotalDue,
                BillToAddress = order.BillToAddress != null ? new AddressesDetailDto
                {
                    AddressId = order.BillToAddress.AddressId,
                    AddressLine1 = order.BillToAddress.AddressLine1!,
                    City = order.BillToAddress.City!,
                    PostalCode = order.BillToAddress.PostalCode!
                } : null,
                ShipToAddress = order.ShipToAddress != null ? new AddressesDetailDto
                {
                    AddressId = order.ShipToAddress.AddressId,
                    AddressLine1 = order.ShipToAddress.AddressLine1!,
                    City = order.ShipToAddress.City!,
                    PostalCode = order.ShipToAddress.PostalCode!
                } : null,
                Items = items
            };
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            // Usiamo una transazione per essere sicuri che o eliminiamo tutto o niente
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Cerchiamo l'ordine
                var order = await _context.SalesOrderHeaders.FindAsync(orderId);
                if (order == null) return false;

                // 2. Troviamo ed eliminiamo tutti i dettagli associati (i figli)
                var details = _context.SalesOrderDetails.Where(d => d.SalesOrderId == orderId);
                _context.SalesOrderDetails.RemoveRange(details);

                // 3. Eliminiamo l'ordine (il padre)
                _context.SalesOrderHeaders.Remove(order);

                // 4. Salviamo le modifiche e confermiamo la transazione
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                // In caso di errore, annulliamo tutto
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, byte newStatus)
        {
            var order = await _context.SalesOrderHeaders.FindAsync(orderId);

            if (order == null) return false;

            // Aggiorniamo solo lo status
            order.Status = newStatus;
            order.ModifiedDate = DateTime.Now; // Buona pratica aggiornare sempre la data di modifica

            await _context.SaveChangesAsync();
            return true;
        }




        //customer
        public async Task<OrderDetailDto?> GetCustomerOrderDetailAsync(int orderId, int customerId)
        {
            // Recupero ordine filtrando già per CustomerId (Sicurezza alla base)
            var order = await _context.SalesOrderHeaders
                .Include(o => o.BillToAddress)
                .Include(o => o.ShipToAddress)
                .FirstOrDefaultAsync(o => o.SalesOrderId == orderId && o.CustomerId == customerId);

            if (order == null) return null;

            var items = await _context.SalesOrderDetails
                .Where(i => i.SalesOrderId == orderId)
                .Include(i => i.Product)
                .Select(i => new OrderDetailItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product!.Name,
                    OrderQty = i.OrderQty,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.LineTotal
                }).ToListAsync();

            return new OrderDetailDto
            {
                SalesOrderId = order.SalesOrderId,
                SalesOrderNumber = order.SalesOrderNumber!,
                OrderDate = order.OrderDate,
                ShipDate = order.ShipDate,
                Status = order.Status,
                ShipMethod = order.ShipMethod!,
                SubTotal = order.SubTotal,
                TaxAmt = order.TaxAmt,
                Freight = order.Freight,
                TotalDue = order.TotalDue,
                BillToAddress = order.BillToAddress != null ? new AddressesDetailDto { /* mappatura... */ } : null,
                ShipToAddress = order.ShipToAddress != null ? new AddressesDetailDto { /* mappatura... */ } : null,
                Items = items
            };
        }
    }
}

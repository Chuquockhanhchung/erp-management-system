using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin,Staff")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpPost("advanced")]
    public async Task<IActionResult> CreateAdvanced(CreateOrderDto dto)
    {
        var orderId = await _service.CreateAdvancedAsync(dto);
        return Ok(new { OrderId = orderId, Message = "Order created successfully" });
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")] // hoặc policy PERM:orders.approve
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusDto dto)
    {
        var ok = await _service.UpdateStatusAsync(id, dto);
        return ok ? NoContent() : NotFound();
    }
}
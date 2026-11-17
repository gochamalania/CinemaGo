using CinemaGo.Application.DTOs.Tickets;

namespace CinemaGo.Application.Interfaces;

public interface ITicketService
{
    Task<TicketResponseDto> CreateAsync(
        int userId,
        CreateTicketDto dto);

    Task<TicketResponseDto?> GetByIdAsync(
        int ticketId,
        int userId);

    Task<List<TicketResponseDto>> GetMyTicketsAsync(
        int userId);
}
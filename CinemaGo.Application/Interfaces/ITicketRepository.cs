using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Interfaces;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(int id);

    Task<Ticket?> GetByReservationIdAsync(int reservationId);

    Task<List<Ticket>> GetByUserIdAsync(int userId);

    Task<bool> TicketNumberExistsAsync(string ticketNumber);

    Task AddAsync(Ticket ticket);
}
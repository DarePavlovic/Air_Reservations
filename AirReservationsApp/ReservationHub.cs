using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class ReservationHub : Hub
{
    public async Task NotifyUpdate()
    {
        await Clients.All.SendAsync("ReceiveReservationUpdate");
    }

    public async Task NotifyNewReservation()
    {
        await Clients.All.SendAsync("ReceiveNewReservation");
    }
    public async Task NotifyReservationUpdate()
    {
        await Clients.All.SendAsync("ReceiveReservationStatusChange");
    }
}

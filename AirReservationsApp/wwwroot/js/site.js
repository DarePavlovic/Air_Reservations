// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/reservationHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.start().catch(err => console.error(err.toString()));

// Kada stigne nova rezervacija
connection.on("ReceiveNewReservation", (flightId) => {
    alert("Nova rezervacija za let ID: " + flightId);
    location.reload(); // Osvetli prikaz na stranici
});

// Kada se status rezervacije promeni
connection.on("ReceiveReservationStatusChange", (reservationId, status) => {
    alert("Rezervacija ID " + reservationId + " sada ima status: " + status);
    document.getElementById(`reservation-status-${reservationId}`).innerText = status;
});

package com.listings.catalog;

import java.util.UUID;
import java.util.List;
import java.util.NoSuchElementException;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.web.bind.annotation.*;


@RestController
@RequestMapping("/api/tickets")
public class TicketRestController {
    
    @Autowired
    private TicketRepository ticketRepository;

    @GetMapping(path="")
    @ResponseStatus(code=HttpStatus.OK)
    public List<Ticket> findAllTickets() {
        return ticketRepository.findAll();
    }

    @GetMapping(path="/{id}")
    @ResponseStatus(code=HttpStatus.OK)
    public Ticket findTicketById(@PathVariable("id") UUID id) {
        return ticketRepository.findById(id).orElseThrow(() -> new NoSuchElementException("Ticket not found"));
    }

    @GetMapping(path = "/search/{searchText}")
    @ResponseStatus(code = HttpStatus.OK)
    public List<Ticket> searchItems(@PathVariable(required = true) String searchText) {
        return ticketRepository.findByEventContainingOrDescriptionContainingIgnoreCase(searchText, searchText);
    }

    @PostMapping(path="")
    @ResponseStatus(code=HttpStatus.CREATED)
    public Ticket createTicket(@RequestBody Ticket ticket) {
        ticket.setId(UUID.randomUUID());
        return ticketRepository.save(ticket);
    }

    @PutMapping(path = "/{ticketGuid}")
    @ResponseStatus(HttpStatus.OK)
    public Ticket updateTicket(@PathVariable(required = true) UUID ticketGuid, @RequestBody Ticket ticket) {
        Ticket existing = ticketRepository.findById(ticketGuid).orElseThrow(() -> new NoSuchElementException("Ticket not found"));
        ticket.setId(ticketGuid);
        existing = ticket;
        return ticketRepository.save(existing);
    }

    @DeleteMapping(path="/{id}")
    @ResponseStatus(code=HttpStatus.NO_CONTENT)
    public void deleteTicket(@PathVariable("id") UUID id) {
        ticketRepository.deleteById(id);
    }

}

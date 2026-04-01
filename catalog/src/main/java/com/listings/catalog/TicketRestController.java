package com.listings.catalog;

import java.util.UUID;
import java.util.List;
import java.util.NoSuchElementException;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.PathVariable;


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

    @PostMapping(path="")
    @ResponseStatus(code=HttpStatus.CREATED)
    public Ticket createTicket(@RequestBody Ticket ticket) {
        ticket.setId(UUID.randomUUID());
        return ticketRepository.save(ticket);
    }

    @PutMapping("/{id}")
    public Ticket updateTicketbyId(@PathVariable String id, @RequestBody Ticket entity) {
        Ticket ticket = ticketRepository.findById(UUID.fromString(id)).orElseThrow(() -> new NoSuchElementException("Ticket not found"));
        ticket = entity;
        return ticketRepository.save(ticket);
    }

    @DeleteMapping(path="/{id}")
    @ResponseStatus(code=HttpStatus.NO_CONTENT)
    public void deleteTicket(@PathVariable("id") UUID id) {
        ticketRepository.deleteById(id);
    }

}

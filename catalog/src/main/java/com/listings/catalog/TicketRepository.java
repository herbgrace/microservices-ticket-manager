package com.listings.catalog;

import java.util.List;
import java.util.UUID;
import java.time.LocalDate;

import org.springframework.data.mongodb.repository.MongoRepository;

public interface TicketRepository extends MongoRepository<Ticket, UUID> {

    public List<Ticket> findByEventContainingOrDescriptionContaining(String txt, String txt2); 

    List<Ticket> findByEventContainingIgnoreCase(String event);

    List<Ticket> findByEventDate(LocalDate eventDate);
}
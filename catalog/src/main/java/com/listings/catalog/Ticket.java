package com.listings.catalog;

import java.time.LocalDate;
import java.util.UUID;
import org.springframework.data.annotation.Id;
import org.springframework.data.mongodb.core.mapping.Document;

@Document(collection = "tickets")
public class Ticket{
    @Id
    private UUID id;
    private String event;
    private double price;
    private String description;
    private LocalDate eventDate;

    public Ticket(UUID id, String event, double price, String description, LocalDate eventDate) {
        setId(id);
        setEvent(event);
        setPrice(price);
        setDescription(description);
        setEventDate(eventDate);
    }

    public UUID getId() {
        return id;
    }

    public void setId(UUID id) {
        this.id = id;
    }

    public double getPrice() {
        return price;
    }

    public void setPrice(double price) {
        this.price = price;
    }

    public String getEvent() {
        return event;
    }

    public void setEvent(String title) {
        this.event = title;
    }

    public String getDescription() {
        return description;
    }

    public void setDescription(String description) {
        this.description = description;
    }

    public LocalDate getEventDate() {
        return eventDate;
    }

    public void setEventDate(LocalDate eventDate) {
        this.eventDate = eventDate;
    }
}

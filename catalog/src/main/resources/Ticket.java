package com.listings.catalog;

public class Ticket{
    private int id;
    private String title;
    private String description;

    public Ticket(int id, String title, String description) {
        setId(id);
        setTitle(title);
        setDescription(description);
    }

    public int getId() {
        return id;
    }

    public void setId(int id) {
        this.id = id;
    }

    public String getTitle() {
        return title;
    }

    public void setTitle(String title) {
        this.title = title;
    }

    public String getDescription() {
        return description;
    }

    public void setDescription(String description) {
        this.description = description;
    }
}

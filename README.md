# PeakLabs

> Modern software solutions built with clean architecture, scalable technologies, and a user-focused experience.

PeakLabs is a software development project focused on building modern, scalable, and maintainable web applications.

The project is being developed as a team using ASP.NET Core MVC and modern web development technologies.

---

## Project Overview

PeakLabs is designed to provide a professional platform for showcasing software projects, services, company information, and managing internal operations through a secure dashboard.

The project consists of two main parts:

- Public Website
- Admin Dashboard

### Public Website

The public section of the website provides:

- Home page
- About Us
- Services
- Portfolio / Projects
- Project Details
- Contact
- Multi-language support
- Light / Dark mode

### Admin Dashboard

The dashboard is designed for internal management and may include:

- Dashboard overview
- User management
- Project management
- Contact message management
- Content management
- Reports
- Settings
- Authentication and authorization

---

# Technologies

The project is built using the following technologies:

### Backend

- ASP.NET Core
- ASP.NET Core MVC
- C#
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity

### Frontend

- HTML5
- CSS3
- JavaScript
- Razor Views

### Development Tools

- Visual Studio Code
- Git
- GitHub
- Trello

---

# Architecture

The project follows a structured ASP.NET Core MVC architecture.

```text
PeakLabs
│
├── Areas
│   └── Admin
│       ├── Controllers
│       ├── Views
│       └── Models
│
├── Controllers
│
├── Models
│   ├── Entities
│   ├── ViewModels
│   └── DTOs
│
├── Data
│   ├── ApplicationDbContext.cs
│   ├── Configurations
│   └── Migrations
│
├── Services
│   ├── Interfaces
│   └── Implementations
│
├── Views
│   ├── Home
│   ├── Portfolio
│   ├── Contact
│   ├── Account
│   └── Shared
│
├── wwwroot
│   ├── css
│   ├── js
│   ├── images
│   └── fonts
│
├── Program.cs
└── appsettings.json
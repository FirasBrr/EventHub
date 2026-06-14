# 🎯 EventHub - Event Management Platform

EventHub is a full-featured event management web application that allows users to discover, register for, and manage events. Built with ASP.NET Core MVC and featuring a modern, responsive design with a clean Red/Blue/White theme.

## ✨ Features

### 🔐 Authentication & User Management
- User registration with validation
- Secure login/logout (email or username)
- Role-based access (Admin & Regular User)
- User profile management (edit info + change password)
- Session management with "Remember Me" functionality
- Admin can manage users (view, edit, enable/disable, delete)

### 📅 Event Management (Admin)
- Create events with interactive map location picker
- Edit and update event details with map
- Delete events (soft delete - keeps data for analytics)
- View event analytics and statistics
- Manage all events from admin panel

### 🎫 Event Registration (Users)
- Browse and search events
- Register for events with automatic capacity checking
- Cancel registration anytime
- View "My Events" dashboard
- Prevent double registration
- Admin cannot register (proper role separation)

### 🔍 Advanced Search & Filters
- Keyword search (name, description, location)
- Category filtering
- Price range (min/max)
- Free events toggle
- Date range filtering
- Sort by date, price, or popularity
- Real-time debounced search (500ms delay)
- Results count display

### 📊 Admin Dashboard
- Statistics overview (events, users, registrations, revenue)
- Registration trends chart (last 6 months)
- Events by category pie chart
- Recent events feed (last 5 events)
- Recent registrations feed (last 10 registrations)
- Popular categories with progress bars
- Quick action buttons for common tasks

### 🗺️ Interactive Maps
- Event location selection on create/edit
- Mini maps on event cards
- Full-size map on event details page
- Reverse geocoding (address ↔ coordinates)
- Click-to-select location
- Powered by Leaflet.js + OpenStreetMap (free, no API key needed)

### 👤 User Profile
- View profile information
- Edit name, username, and email
- Change password
- User statistics (events attended, upcoming events, member since)
- Clean avatar and layout

### 🎨 UI/UX
- Clean Red/Blue/White theme
- Fully responsive design (mobile-friendly)
- Glass morphism effects
- Smooth animations and hover effects
- Professional card layouts
- Toast notifications for user actions
- Scroll to top button
- Custom scrollbar styling

## 🛠️ Tech Stack

| Layer | Technology |
|-------|------------|
| **Backend** | ASP.NET Core MVC (.NET 8) |
| **Frontend** | HTML5, CSS3, JavaScript, Bootstrap 5 |
| **Database** | MySQL (XAMPP) |
| **ORM** | Entity Framework Core |
| **Authentication** | ASP.NET Core Identity |
| **Maps** | Leaflet.js + OpenStreetMap |
| **Charts** | Chart.js |
| **Icons** | Bootstrap Icons |
| **Fonts** | Google Fonts (Inter, Syne, DM Sans) |


# RegentHealth 🏥

A clinic management desktop application built with WPF and C#. Designed as a portfolio project to demonstrate real-world application architecture with role-based access, appointment scheduling, and SQLite persistence.

---

## Features

### 👤 Roles
The system supports three user roles — each with its own dashboard and permissions.

| Role | Access |
|------|--------|
| **Admin** | Manage doctors, set weekly rotation, view today's appointments |
| **Doctor** | View assigned appointments, complete or track missed ones |
| **Patient** | Book appointments, cancel, view history |

### 📅 Appointment Booking
- Patients can book appointments up to 14 days ahead
- Available time slots are generated based on the doctor's active rotation
- Lunch break (12:00–13:00) is automatically excluded
- Weekends only allow emergency appointments

### 🚨 Emergency System
- Patients can request an emergency appointment at any time
- If an emergency doctor is free — booked immediately
- If all emergency doctors are busy — patient is placed in a queue
- When a doctor completes an appointment, the next patient from the queue is automatically assigned

### 📋 Weekly Rotation
- Admin sets working schedules week by week
- Each day can have a doctor marked as **Working** and/or **Emergency (on-call)**
- Rotation data is tied to specific dates — different weeks hold independent schedules
- Saving rotation updates doctor shift flags used by the booking system

### ⏰ Shift & Missed Logic
- Doctor shift status updates automatically based on work hours (no manual toggle needed)
- Appointments not completed by end of working day are automatically marked as **Missed**

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| UI | WPF (Windows Presentation Foundation) |
| Language | C# / .NET 10 |
| Database | SQLite |
| ORM | Entity Framework Core |
| Architecture | Service layer + ViewModel pattern |

### Project Structure

```
RegentHealth/
├── Data/               # AppDbContext, DesignTimeDbContextFactory
├── Enums/              # AppointmentType, AppointmentRules
├── Helpers/            # DoctorScheduler, PasswordHelper
├── Models/             # User, Doctor, Appointment, DoctorRotation
├── Services/           # AuthService, AppointmentService, AdminService, DataService
├── ViewModels/         # AppointmentsViewModel, DoctorViewModel
├── Views/              # All pages (Login, Dashboard, Patient, Doctor, Admin)
│   └── Admin/          # Admin-specific pages (Rotation, DoctorsList, etc.)
└── Migrations/         # EF Core database migrations
```

---

## Getting Started

### Requirements
- Windows 10 or later
- .NET 10 SDK
- Visual Studio 2022+

### NuGet Packages
```
Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.Sqlite
Microsoft.EntityFrameworkCore.Tools
```

### Setup

1. Clone the repository
```bash
git clone https://github.com/enelthegod/RegentHealth.git
```

2. Open `RegentHealth.slnx` in Visual Studio

3. Open **Package Manager Console** and run:
```
Update-Database
```

4. Press **F5** to run

### Default Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin` | `admin` |
| Doctor | `doctor@test.com` | `123` |

> Patients can register through the app. Password must be at least 6 characters and contain at least one number.

---

## How It Works

### Booking Flow
```
Patient selects date + type → 
System checks rotation for that date → 
Available slots generated → 
Patient picks time → 
Least busy doctor assigned automatically
```

### Emergency Flow
```
Patient requests emergency →
System finds free emergency doctor →
  ✅ Doctor free → booked immediately
  ❌ All busy → added to queue →
     Doctor completes appointment → queue processed automatically
```

### Rotation → Booking Connection
The weekly rotation page is the key link between admin and patients. When admin saves a rotation:
- Doctor's `IsOnShift` flag is updated
- `WorkingDays` list is rebuilt
- Patient's booking page will show time slots only for days where a doctor is scheduled

---

## Password Requirements
- Minimum 6 characters
- At least one digit

---

## Notes
- The SQLite database file (`regenthealth.db`) is created automatically on first launch next to the `.exe`
- Database is not included in the repository — each installation creates its own
- Emergency queue is in-memory only and resets on app restart (by design)

---

## 📸 Application Screenshots

### 🔐 Login Page
![Login Page](screenshots/loginpage.png)

### 📝 Register Page
![Register Page](screenshots/registerpage.png)

### 👨‍⚕️ Doctor List Page
![Doctor List](screenshots/doctorlistpage.png)

### 👤 Patient Page
![Patient Page](screenshots/patientpage.png)

### 🔄 Rotation Page
![Rotation Page](screenshots/rotationpage.png)

### 🛠️ Admin Dashboard
![Admin Dashboard](screenshots/admindashboard.png)

---

*Built as a portfolio project — open to feedback and contributions.*

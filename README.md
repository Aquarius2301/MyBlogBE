# MyBlogBE

A modern blog backend API built with ASP.NET Core, featuring JWT authentication, image management with Cloudinary, and comprehensive blog management capabilities.

## Table of Contents

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [Database Setup](#database-setup)
- [Running the Application](#running-the-application)
- [Technologies Used](#technologies-used)
- [Support](#support)

## Features

- 🔐 JWT-based authentication with refresh tokens
- 📝 Complete blog post CRUD operations
- 🖼️ Image upload and management with Cloudinary
- 📧 Email notifications
- 👥 User management system
- 🔍 Pagination support
- 🎲 Database seeding for development
- 📊 RESTful API architecture

## Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or later
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB, Express, or full version)
- [Entity Framework Core CLI tools](https://docs.microsoft.com/en-us/ef/core/cli/dotnet)

## Installation

1. **Clone the repository**

```bash
git clone https://github.com/Aquarius2301/MyBlogBE.git
cd MyBlogBE
```

2. **Restore dependencies**

```bash
dotnet restore

```

## Project Structure

```
MyBlog/
├── BusinessObject/               # Domain models and DbContext
│   ├── Enums                     # Enum types
│   ├── Models                    # Entity models
│   ├── Seeds                     # Seed Data
│   └── MyBlogContext.cs          # EF Core DbContext
├── DataAccess/                   # Data access layer
│   ├── Repositories              # Repository implementations
│   └── UnitOfWork                # Unit of Work
├── WebApi/                       # WebApi Project (Startup)
│   ├── Controllers               # API endpoints
│   ├── Dtos                      # Data Transfer Objects
│   ├── Helpers                   # Helpers/Utilities (Validation, Email, JWT, ...)
│   ├── Properties/               # Project properties and launch configuration
│   │   └── launchSettings.json   # Local launch profiles (URLs)
│   ├── Resources                 # Multi-language (English, Vietnamese)
│   ├── Services                  # Service implementations
│   ├── Settings                  # Setting files (Email, JWT, Cloudinary)
│   ├── appsettings.json          # Configuration file
│   └── Program.cs                # Application entry point
├── .gitignore
└── README.md
```

## Configuration

### 1. Database Connection String

Open `appsettings.json` in the WebApi project and configure your connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MyBlogDB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

**Connection String Examples:**

- **LocalDB:** `Server=(localdb)\\mssqllocaldb;Database=MyBlogDB;Trusted_Connection=true;TrustServerCertificate=true;`
- **SQL Server Express:** `Server=.\\SQLEXPRESS;Database=MyBlogDB;Trusted_Connection=true;TrustServerCertificate=true;`
- **SQL Server with credentials:** `Server=your-server;Database=MyBlogDB;User Id=your-username;Password=your-password;TrustServerCertificate=true;`

### 2. Base Settings Configuration

Configure the following settings in `appsettings.json`:

```json
{
  "BaseSettings": {
    "PageSize": 10,
    "MaxPageSize": 50,
    "TokenLength": 10,
    "TokenExpiryMinutes": 30,
    "JwtSettings": {
      "Key": "your-super-secret-key-min-32-characters-long",
      "Issuer": "MyBlogAPI",
      "Audience": "MyBlogClient",
      "AccessTokenDurationMinutes": 60,
      "RefreshTokenDurationDays": 15
    },
    "EmailSettings": {
      "SmtpServer": "smtp.gmail.com",
      "Port": 587,
      "SenderName": "MyBlog Admin",
      "SenderEmail": "your-email@gmail.com",
      "Username": "your-email@gmail.com",
      "Password": "your-app-specific-password"
    },
    "CloudinarySettings": {
      "CloudName": "your-cloudinary-cloud-name",
      "ApiKey": "your-cloudinary-api-key",
      "ApiSecret": "your-cloudinary-api-secret"
    }
  }
}
```

### Configuration Details

#### JWT Settings

- **Key:** Generate a secure random string (minimum 32 characters). Use a generator like [RandomKeygen](https://randomkeygen.com/)
- **Issuer:** Your API identifier (e.g., "MyBlogAPI")
- **Audience:** Your client application identifier (e.g., "MyBlogClient")

#### Email Settings (Gmail Example)

1. Enable 2-factor authentication on your Gmail account
2. Generate an [App Password](https://support.google.com/accounts/answer/185833)
3. Use the app password in the configuration

#### Cloudinary Settings

1. Sign up for a free account at [Cloudinary](https://cloudinary.com/)
2. Get your credentials from the Cloudinary dashboard
3. Add them to the configuration

## Database Setup

### 1. Create Initial Migration

From the WebApi project directory, run:

```bash
dotnet ef migrations add InitialProject --project ..\BusinessObject\ --startup-project .
```

### 2. Update Database

Apply the migration to create the database:

```bash
dotnet ef database update
```

### 3. Data Seeding (Optional)

The application includes a database seeder for development purposes.

**To enable seeding:**

In `Program.cs` (WebApi project), ensure these lines are **uncommented**:

```csharp
// Comment this line to prevent automatic database migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyBlogContext>();
    Seeder.Seed(db);
}
```

**To disable seeding:**

Comment out the above lines to prevent automatic data generation on each startup.

**Note:** The seeder creates sample users, blog posts, and other test data. Only use this in development environments.

## Running the Application

### Development Mode

Run with hot reload:

```bash
cd WebApi
dotnet watch
```

### Standard Run

```bash
cd WebApi
dotnet run
```

The API will be available at:

- HTTP: `http://localhost:5250`

You can change the URL in Properties/launchSettings.json

### Swagger Documentation

Once running, access the API documentation at:

- `http://localhost:5250/swagger`

## Technologies Used

- **Framework:** ASP.NET Core 8.0+
- **Database:** SQL Server with Entity Framework Core
- **Authentication:** JWT (JSON Web Tokens)
- **Image Storage:** Cloudinary
- **Email:** SMTP (configurable)
- **Documentation:** Swagger/OpenAPI
- **Architecture:** Layered Architecture Pattern with Repository, Unit Of Work, Service Layer Pattern

## Support

For issues and questions:

- Open an issue on GitHub
- Contact: khangta67@gmail.com

---

**Happy Coding! 🚀**

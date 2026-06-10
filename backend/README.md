# Job Application Tracker Backend

ASP.NET Core 8 backend API with web scraping capabilities for job information extraction.

## Features

- RESTful API for job application management
- Automatic job information extraction from LinkedIn, Indeed, Glassdoor, and other job sites
- Job work type detection (Remote, On-site, Hybrid)
- Entity Framework Core with SQL Server
- CORS enabled for frontend communication
- Swagger/OpenAPI documentation

## Prerequisites

- .NET 8 SDK
- SQL Server LocalDB or SQL Server instance
- Visual Studio Code or Visual Studio

## Installation & Setup

### 1. Install .NET 8 SDK

If you haven't installed .NET 8 SDK, download it from: https://dotnet.microsoft.com/download/dotnet/8.0

Verify installation:
```bash
dotnet --version
```

### 2. Restore Dependencies

```bash
cd backend
dotnet restore
```

### 3. Create & Apply Database Migrations

```bash
dotnet ef database update
```

This will create the local SQL Server database automatically.

### 4. Run the Backend

```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger/index.html`

## API Endpoints

### Jobs

- `GET /api/jobs` - Get all job applications
- `GET /api/jobs/{id}` - Get a specific job application
- `POST /api/jobs` - Create a new job application
- `PUT /api/jobs/{id}` - Update a job application
- `DELETE /api/jobs/{id}` - Delete a job application
- `POST /api/jobs/extract` - Extract job information from a URL

### Extract Job Info Request

```json
{
  "url": "https://www.linkedin.com/jobs/view/1234567890/"
}
```

### Extract Job Info Response

```json
{
  "jobTitle": "Senior Software Engineer",
  "companyName": "Tech Company Inc",
  "description": "We are looking for a senior software engineer...",
  "location": "San Francisco, CA",
  "workType": "Hybrid"
}
```

## Development

### Edit and Continue

The backend supports hot reload during development. Simply save your changes and the server will automatically rebuild.

### Debugging

Use Visual Studio Code with the C# extension or Visual Studio for debugging.

## Database

The application uses SQL Server LocalDB by default. To use a different database:

1. Update the connection string in `appsettings.json`
2. Ensure SQL Server is running
3. Run `dotnet ef database update`

## Troubleshooting

### Database Connection Issues

If you get connection errors:
1. Verify SQL Server LocalDB is installed
2. Check the connection string in `appsettings.json`
3. Run: `sqllocaldb info mssqllocaldb`

### Port Already in Use

If port 5000 is in use, you can specify a different port:
```bash
dotnet run --urls "http://localhost:5001"
```

## NuGet Packages

- **HtmlAgilityPack**: HTML parsing and web scraping
- **EntityFrameworkCore**: ORM and database management
- **EntityFrameworkCore.SqlServer**: SQL Server provider
- **Swashbuckle.AspNetCore**: Swagger documentation

## CORS Configuration

The backend allows requests from:
- `http://localhost:4200` (Angular frontend)
- `http://localhost:3000` (Alternative frontend)

To add more origins, update the CORS policy in `Program.cs`.

# Job Application Tracker

A modern web application to streamline your job search by automatically extracting job information from URLs and tracking your applications.

## Features

✨ **Smart Job Extraction**: Paste a job link from LinkedIn, Indeed, Glassdoor, or any company website, and the app automatically extracts:
- Job title
- Company name
- Job description
- Location
- Work type (Remote, On-site, Hybrid)

📊 **Application Tracking**: Manage all your job applications with:
- Track application status (Applied, In Progress, Interview, Offer, Rejected)
- Store original job links for reference
- Add personal notes for each application
- Filter jobs by title/company and status

🎯 **User-Friendly Interface**: Modern, responsive UI with:
- One-click job extraction from URLs
- Easy-to-edit job details
- Sortable and filterable job table
- Mobile-friendly design

## Technology Stack

- **Frontend**: Angular 18+ with TypeScript
- **Backend**: ASP.NET Core 8 with C#
- **Database**: SQL Server (LocalDB or cloud)
- **Web Scraping**: HtmlAgilityPack for intelligent job data extraction
- **API**: RESTful API with automatic CORS handling

## How It Works

1. **Paste a Job Link**: Copy any job posting URL and paste it into the app
2. **Auto-Extract**: The backend scrapes the page and extracts relevant information
3. **Review & Edit**: Verify the extracted data and make any manual adjustments
4. **Save**: The job is saved with today's date and "Applied" status
5. **Track**: Monitor your applications and update status as you progress

## Project Structure

```
Job-Application-Tracker/
├── frontend/               # Angular application
│   ├── src/
│   │   ├── app/
│   │   │   ├── models/    # TypeScript interfaces
│   │   │   ├── services/  # API communication
│   │   │   └── components/ # Angular components
│   │   └── main.ts        # Entry point
│   └── package.json
│
├── backend/               # ASP.NET Core API
│   ├── Controllers/      # API endpoints
│   ├── Models/           # Data models
│   ├── Services/         # Business logic (web scraping)
│   ├── Data/            # Database context
│   └── Program.cs        # Startup configuration
│
└── README.md
```

## Quick Start

### Prerequisites
- Node.js 18+ (for Angular frontend)
- .NET 8 SDK (for C# backend)
- SQL Server or SQL Server LocalDB

### Frontend Setup

```bash
cd frontend
npm install
npm start
```

The frontend will be available at `http://localhost:4200`

### Backend Setup

```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run
```

The backend API will be available at `https://localhost:5001`

The Swagger documentation will be at `https://localhost:5001/swagger`

## Example Table Structure

| Job Title | Company | Location | Work Type | Status | Job Link | Notes |
|-----------|---------|----------|-----------|--------|----------|-------|
| Senior Software Engineer | TechCorp | San Francisco, CA | Hybrid | Interview | [Link](url) | Great team, second round |
| Data Analyst | DataInc | Remote | Remote | Applied | [Link](url) | - |
| Full Stack Developer | StartupXYZ | Austin, TX | On-site | Offer | [Link](url) | Negotiating salary |

## Supported Job Sites

The app extracts information from:
- ✅ LinkedIn
- ✅ Indeed
- ✅ Glassdoor
- ✅ Company career pages
- ✅ Any HTML-based job posting

## API Endpoints

### Jobs Management
- `GET /api/jobs` - List all job applications
- `GET /api/jobs/{id}` - Get a specific application
- `POST /api/jobs` - Create new application
- `PUT /api/jobs/{id}` - Update application
- `DELETE /api/jobs/{id}` - Delete application

### Job Extraction
- `POST /api/jobs/extract` - Extract info from URL

**Request:**
```json
{
  "url": "https://www.linkedin.com/jobs/view/3123456789/"
}
```

**Response:**
```json
{
  "jobTitle": "Senior Software Engineer",
  "companyName": "TechCorp Inc",
  "description": "We are seeking a talented senior engineer...",
  "location": "San Francisco, CA",
  "workType": "Hybrid"
}
```

## Database Schema

The app stores the following information for each job application:

- **ID**: Unique identifier (UUID)
- **JobTitle**: Position name
- **CompanyName**: Employer name
- **JobLink**: URL to original posting
- **Description**: Job description
- **Location**: Job location
- **WorkType**: Remote/On-site/Hybrid
- **ApplicationStatus**: Current status in the hiring process
- **Notes**: Personal notes
- **CreatedDate**: When the record was created
- **UpdatedDate**: Last modification date

## Environment Configuration

### Frontend
The frontend connects to the backend at `http://localhost:5000`. Update the API URL in `frontend/src/app/services/job-application.service.ts` if needed.

### Backend
Configure the database connection in `backend/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=JobApplicationTrackerDb;Trusted_Connection=true;"
  }
}
```

## Troubleshooting

### Port Already in Use
If port 4200 or 5000 is in use, specify a different port:

**Frontend:**
```bash
npm start -- --port 4201
```

**Backend:**
```bash
dotnet run --urls "https://localhost:5002"
```

### Database Connection Error
1. Ensure SQL Server or LocalDB is running
2. Update the connection string in `backend/appsettings.json`
3. Run `dotnet ef database update`

### CORS Error
The backend should be configured to allow requests from your frontend URL. Check `backend/Program.cs` for CORS policy configuration.

## Future Enhancements

- 📈 Interview progress analytics
- 📧 Email integration for job posting links
- 📅 Calendar integration for interview dates
- 🤖 AI-powered cover letter suggestions
- 💾 Export to PDF or Excel
- 🔐 User authentication and cloud storage
- 📱 Mobile app

## Contributing

Feel free to submit issues or pull requests to improve the application.

## License

This project is open source and available under the MIT License.

---

**Happy Job Hunting!** 🚀
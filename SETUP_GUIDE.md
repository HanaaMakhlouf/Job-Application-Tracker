# Job Application Tracker - Setup & Implementation Guide

## Overview

You now have a complete Job Application Tracker application with:

### ✅ Frontend (Angular 18+)
- Modern UI with job URL extraction feature
- Job application form with manual editing
- Job applications table with filtering
- Real-time form validation
- Beautiful gradient extraction section
- Responsive design

### ✅ Backend (ASP.NET Core 8)
- RESTful API for CRUD operations
- Intelligent web scraper for job extraction
- Support for LinkedIn, Indeed, Glassdoor, and company career pages
- Database layer with Entity Framework Core
- CORS enabled for frontend communication

## Project Structure

```
Job-Application-Tracker/
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/
│   │   │   │   └── job-applications/
│   │   │   │       ├── job-applications.component.ts
│   │   │   │       ├── job-applications.component.html
│   │   │   │       └── job-applications.component.scss
│   │   │   ├── models/
│   │   │   │   └── job-application.model.ts
│   │   │   ├── services/
│   │   │   │   └── job-application.service.ts
│   │   │   ├── app.ts
│   │   │   └── app.config.ts
│   │   └── main.ts
│   ├── package.json
│   ├── angular.json
│   └── tsconfig.json
│
├── backend/
│   ├── Controllers/
│   │   └── JobsController.cs
│   ├── Models/
│   │   └── JobApplication.cs
│   ├── Services/
│   │   └── JobScraperService.cs
│   ├── Data/
│   │   └── ApplicationDbContext.cs
│   ├── Program.cs
│   ├── appsettings.json
│   ├── JobApplicationTracker.csproj
│   └── README.md
│
├── README.md
└── technical_plan.md
```

## Running the Application

### Step 1: Start the Frontend

```bash
cd frontend
npm install  # Only needed the first time
npm start
```

The frontend will be available at: **http://localhost:4200**

You should see:
- ✨ "Quick Add - Paste Job Link" section at the top
- 📝 "Add New Job Application" form section
- 📊 Job applications table (empty initially)

### Step 2: Set Up and Start the Backend

#### Prerequisites
- .NET 8 SDK installed
- SQL Server or SQL Server LocalDB installed

#### Installation Steps

```bash
cd backend
dotnet restore
```

#### Create Database

```bash
dotnet ef database update
```

This creates the local SQL Server database automatically.

#### Start the Backend

```bash
dotnet run
```

The backend will be available at: **https://localhost:5001**
Swagger documentation: **https://localhost:5001/swagger/index.html**

## How to Use the Application

### Method 1: Auto-Extract Job Information

1. **Find a Job**: Go to LinkedIn, Indeed, Glassdoor, or any company career page
2. **Copy Job Link**: Copy the URL of the job posting
3. **Paste in App**: Paste the link in the "Quick Add - Paste Job Link" section
4. **Click Extract**: Click the "Extract Job Info" button
5. **Review**: The form will auto-fill with extracted information:
   - Job Title
   - Company Name
   - Description
   - Location
   - Work Type (auto-detected from description)
6. **Edit if Needed**: Manually adjust any fields as needed
7. **Save**: Click "Add Job Application" to save

### Method 2: Manual Entry

1. Fill in the form manually with all job details
2. Make sure to fill required fields (Job Title, Company Name)
3. Click "Add Job Application"

### Managing Applications

- **Edit**: Click the "Edit" button in the table row
- **Delete**: Click the "Delete" button and confirm
- **Filter**: Use the filter section to search by:
  - Job title/company name
  - Application status

## Key Features

### 1. Smart Job Extraction
- Automatically detects job source (LinkedIn, Indeed, etc.)
- Uses appropriate selectors for each platform
- Falls back to generic extraction for unknown sites
- Detects work type from description

### 2. Application Tracking
- Track status: Applied, In Progress, Interview, Offer, Rejected
- Automatic creation date and update tracking
- Store original job links for reference
- Add personal notes for each application

### 3. Work Type Detection
Automatically identifies from job description:
- **Hybrid**: If description contains "hybrid"
- **On-site**: If contains "on-site", "office", etc.
- **Remote**: If contains "remote", "work from home", etc.

### 4. Search & Filter
- Filter by job title or company name
- Filter by application status
- Real-time filtering as you type

## API Endpoints

### Jobs Management

```
GET    /api/jobs              - Get all jobs
GET    /api/jobs/{id}         - Get specific job
POST   /api/jobs              - Create new job
PUT    /api/jobs/{id}         - Update job
DELETE /api/jobs/{id}         - Delete job
```

### Job Extraction

```
POST   /api/jobs/extract      - Extract from URL
```

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
  "description": "We are seeking...",
  "location": "San Francisco, CA",
  "workType": "Hybrid"
}
```

## Database Schema

Each job application stores:

| Field | Type | Description |
|-------|------|-------------|
| Id | string | Unique identifier |
| JobTitle | string | Position name |
| CompanyName | string | Employer name |
| JobLink | string | URL to posting |
| Description | string | Job description |
| Location | string | Job location |
| WorkType | string | Remote/On-site/Hybrid |
| ApplicationStatus | string | Current status |
| Notes | string | Personal notes |
| CreatedDate | DateTime | Record creation date |
| UpdatedDate | DateTime | Last modification |

## Configuration

### Frontend

The frontend API URL is configured in:
`frontend/src/app/services/job-application.service.ts`

```typescript
private apiUrl = 'http://localhost:5000/api/jobs';
```

To change the backend URL (e.g., for production), update this value.

### Backend

Database connection is configured in:
`backend/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=JobApplicationTrackerDb;Trusted_Connection=true;"
  }
}
```

## Troubleshooting

### Frontend Errors

**Issue**: "Cannot GET /"
- Solution: Make sure you're accessing http://localhost:4200

**Issue**: "Http failure response for http://localhost:5000/api/jobs: 0 Unknown Error"
- Solution: Backend is not running. Start it with `dotnet run` in the backend folder

### Backend Errors

**Issue**: "No .NET SDKs were found"
- Solution: Install .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0

**Issue**: "Login failed for user 'username'"
- Solution: Check SQL Server connection string in appsettings.json

**Issue**: "Cannot find the specified path" for migrations
- Solution: Make sure you're in the backend directory when running `dotnet ef` commands

### Port Already in Use

**Port 4200 in use:**
```bash
npm start -- --port 4201
```

**Port 5000/5001 in use:**
```bash
dotnet run --urls "https://localhost:5002"
```

## Next Steps

1. ✅ Start frontend: `npm start` in frontend folder
2. ✅ Start backend: `dotnet run` in backend folder
3. ✅ Open http://localhost:4200
4. ✅ Try extracting a job from LinkedIn or Indeed
5. ✅ Test adding and managing applications

## Advanced Features (Future)

- User authentication and cloud storage
- Email integration for job posting links
- Calendar integration for interviews
- AI-powered cover letter suggestions
- Export to PDF/Excel
- Interview progress analytics
- Mobile app

## Support

For detailed information:
- Frontend README: See `frontend/README.md`
- Backend README: See `backend/README.md`
- Main README: See `README.md`

---

**Happy Job Hunting!** 🚀

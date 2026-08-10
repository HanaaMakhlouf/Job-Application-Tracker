import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JobApplication } from '../../models/job-application.model';
import { JobApplicationService } from '../../services/job-application.service';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'app-job-applications',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './job-applications.component.html',
    styleUrls: ['./job-applications.component.scss']
})
export class JobApplicationsComponent implements OnInit {
    jobs: JobApplication[] = [];
    filteredJobs: JobApplication[] = [];
    newJob: JobApplication = this.initializeNewJob();
    isEditing = false;
    editingJobId: string | null = null;
    filterText = '';
    filterStatus = '';
    jobUrl = '';
    isExtracting = false;
    extractionError = '';
    currentUserName = '';

    workTypes = ['On-site', 'Remote', 'Hybrid'];
    applicationStatuses = ['Applied', 'In Progress', 'Interview', 'Offer', 'Rejected'];

    constructor(private jobService: JobApplicationService, public auth: AuthService) { }

    ngOnInit(): void {
        this.loadJobs();
        this.auth.getCurrentUser().then(user => this.currentUserName = user?.name ?? '');
    }

    loadJobs(): void {
        this.jobService.getAll().subscribe({
            next: (data: JobApplication[]) => {
                this.jobs = data;
                this.filterJobs();
            },
            error: (error: unknown) => console.error('Error loading jobs:', error)
        });
    }

    initializeNewJob(): JobApplication {
        return {
            jobTitle: '',
            companyName: '',
            jobLink: '',
            description: '',
            location: '',
            workType: 'Remote',
            applicationStatus: 'Applied',
            notes: ''
        };
    }

    addJob(): void {
        if (!this.newJob.jobTitle || !this.newJob.companyName) {
            alert('Please fill in required fields');
            return;
        }

        if (this.isEditing && this.editingJobId) {
            this.jobService.update(this.editingJobId, this.newJob).subscribe({
                next: () => {
                    this.loadJobs();
                    this.resetForm();
                },
                error: (error: unknown) => console.error('Error updating job:', error)
            });
        } else {
            this.jobService.create(this.newJob).subscribe({
                next: () => {
                    this.loadJobs();
                    this.resetForm();
                },
                error: (error: unknown) => console.error('Error creating job:', error)
            });
        }
    }

    editJob(job: JobApplication): void {
        this.newJob = { ...job };
        this.editingJobId = job.id || null;
        this.isEditing = true;
    }

    deleteJob(id: string | undefined): void {
        if (!id) return;
        if (confirm('Are you sure you want to delete this job?')) {
            this.jobService.delete(id).subscribe({
                next: () => this.loadJobs(),
                error: (error: unknown) => console.error('Error deleting job:', error)
            });
        }
    }

    resetForm(): void {
        this.newJob = this.initializeNewJob();
        this.isEditing = false;
        this.editingJobId = null;
        this.jobUrl = '';
        this.extractionError = '';
    }

    extractJobInfo(): void {
        if (!this.jobUrl.trim()) {
            this.extractionError = 'Please enter a valid job URL';
            return;
        }

        const clientParsed = this.parseJobUrl(this.jobUrl);
        const cleanLink = clientParsed.jobLink ?? this.jobUrl;

        // Client-side parsing got a title — use it instantly, no backend call needed
        if (clientParsed.jobTitle) {
            this.applyExtracted({ ...clientParsed, jobLink: cleanLink });
            return;
        }

        // LinkedIn blocks server-side scraping — skip the backend call entirely
        if (cleanLink.includes('linkedin.com')) {
            this.applyExtracted({ ...clientParsed, jobLink: cleanLink });
            return;
        }

        // For other sites, ask the backend to scrape the page
        this.isExtracting = true;
        this.extractionError = '';

        this.jobService.extractJobInfo(cleanLink).subscribe({
            next: (data) => {
                this.applyExtracted({
                    jobTitle: data.jobTitle ?? '',
                    companyName: clientParsed.companyName || data.companyName || '',
                    description: data.description ?? '',
                    location: data.location ?? '',
                    workType: data.workType ?? 'Remote',
                    jobLink: cleanLink
                });
            },
            error: () => {
                this.applyExtracted({ ...clientParsed, jobLink: cleanLink });
            }
        });
    }

    private applyExtracted(data: Partial<JobApplication>): void {
        this.newJob = {
            ...this.initializeNewJob(),
            ...data,
            applicationStatus: 'Applied',
        };
        this.jobUrl = '';
        this.isExtracting = false;
        this.extractionError = data.jobTitle
            ? 'Form prepared! Review the extracted details and fill in anything missing.'
            : 'Form prepared! Open the job link to copy the title and company, then fill them in below.';
    }

    private parseJobUrl(url: string): Partial<JobApplication> {
        try {
            const parsed = new URL(url);
            const hostParts = parsed.hostname.split('.');
            const domain = hostParts[hostParts.length - 2];

            // Job boards — don't use their name as the company
            const jobBoards = new Set(['linkedin', 'indeed', 'glassdoor', 'monster', 'ziprecruiter', 'seek', 'careers']);

            let companyName = '';
            if (!jobBoards.has(domain)) {
                companyName = domain.charAt(0).toUpperCase() + domain.slice(1);
            }

            let jobTitle = '';
            let jobLink = url;

            if (parsed.hostname.includes('linkedin.com')) {
                // LinkedIn search results: extract keywords + build canonical job URL
                if (parsed.pathname.includes('search')) {
                    const keywords = parsed.searchParams.get('keywords');
                    if (keywords) jobTitle = keywords;

                    const jobId = parsed.searchParams.get('currentJobId');
                    if (jobId) jobLink = `https://www.linkedin.com/jobs/view/${jobId}/`;
                }
                // LinkedIn direct job URL: /jobs/view/{id}
                else if (parsed.pathname.includes('/jobs/view/')) {
                    const parts = parsed.pathname.split('/').filter(p => p);
                    const viewIdx = parts.indexOf('view');
                    if (viewIdx >= 0 && parts[viewIdx + 1]) {
                        jobLink = `https://www.linkedin.com/jobs/view/${parts[viewIdx + 1]}/`;
                    }
                }
            } else {
                // Generic URL: extract title from last non-numeric path segment
                const pathParts = parsed.pathname.split('/').filter(p => p.length > 0);
                for (let i = pathParts.length - 1; i >= 0; i--) {
                    if (!/^\d+$/.test(pathParts[i]) && pathParts[i] !== 'job' && pathParts[i] !== 'jobs') {
                        jobTitle = pathParts[i]
                            .replace(/[-_]/g, ' ')
                            .replace(/\b\w/g, c => c.toUpperCase());
                        break;
                    }
                }
            }

            return { jobTitle, companyName, jobLink };
        } catch {
            return {};
        }
    }

    logout(): void {
        this.auth.logout();
    }

    filterJobs(): void {
        this.filteredJobs = this.jobs.filter(job => {
            const matchesText = !this.filterText ||
                job.jobTitle.toLowerCase().includes(this.filterText.toLowerCase()) ||
                job.companyName.toLowerCase().includes(this.filterText.toLowerCase());

            const matchesStatus = !this.filterStatus || job.applicationStatus === this.filterStatus;

            return matchesText && matchesStatus;
        });
    }

    onFilterChange(): void {
        this.filterJobs();
    }
}

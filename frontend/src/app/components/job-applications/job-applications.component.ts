import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JobApplication } from '../../models/job-application.model';
import { JobApplicationService } from '../../services/job-application.service';

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

    workTypes = ['On-site', 'Remote', 'Hybrid'];
    applicationStatuses = ['Applied', 'In Progress', 'Interview', 'Offer', 'Rejected'];

    constructor(private jobService: JobApplicationService) { }

    ngOnInit(): void {
        this.loadJobs();
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

        const extracted = this.parseJobUrl(this.jobUrl);
        this.newJob = {
            ...this.initializeNewJob(),
            ...extracted,
            jobLink: this.jobUrl,
            applicationStatus: 'Applied',
        };
        this.jobUrl = '';
        this.extractionError = extracted.jobTitle
            ? 'Form prepared! Review the extracted details and fill in anything missing.'
            : 'Form prepared! Fill in the job title and company name, then paste the description.';
    }

    private parseJobUrl(url: string): Partial<JobApplication> {
        try {
            const parsed = new URL(url);
            const hostParts = parsed.hostname.split('.');

            // Extract company from domain, e.g. "cisco" from "careers.cisco.com"
            let companyName = '';
            if (hostParts.length >= 2) {
                const raw = hostParts[hostParts.length - 2];
                companyName = raw.charAt(0).toUpperCase() + raw.slice(1);
            }

            // Extract job title from last non-numeric path segment
            const pathParts = parsed.pathname.split('/').filter(p => p.length > 0);
            let jobTitle = '';
            for (let i = pathParts.length - 1; i >= 0; i--) {
                if (!/^\d+$/.test(pathParts[i])) {
                    jobTitle = pathParts[i]
                        .replace(/-/g, ' ')
                        .replace(/\b\w/g, c => c.toUpperCase());
                    break;
                }
            }

            return { jobTitle, companyName };
        } catch {
            return {};
        }
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

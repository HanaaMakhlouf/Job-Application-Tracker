export interface JobApplication {
    id?: string;
    jobTitle: string;
    companyName: string;
    jobLink: string;
    description: string;
    location: string;
    workType: 'On-site' | 'Remote' | 'Hybrid';
    applicationStatus: 'Applied' | 'In Progress' | 'Interview' | 'Offer' | 'Rejected';
    notes: string;
    createdDate?: Date;
    updatedDate?: Date;
}

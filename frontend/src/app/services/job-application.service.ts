import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { JobApplication } from '../models/job-application.model';

@Injectable({
    providedIn: 'root'
})
export class JobApplicationService {
    private apiUrl = 'http://localhost:5002/api/jobs'; // Will connect to C# backend

    constructor(private http: HttpClient) { }

    getAll(): Observable<JobApplication[]> {
        return this.http.get<JobApplication[]>(this.apiUrl);
    }

    getById(id: string): Observable<JobApplication> {
        return this.http.get<JobApplication>(`${this.apiUrl}/${id}`);
    }

    create(job: JobApplication): Observable<JobApplication> {
        return this.http.post<JobApplication>(this.apiUrl, job);
    }

    update(id: string, job: JobApplication): Observable<JobApplication> {
        return this.http.put<JobApplication>(`${this.apiUrl}/${id}`, job);
    }

    delete(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }

    extractJobInfo(url: string): Observable<Partial<JobApplication>> {
        return this.http.post<Partial<JobApplication>>(`${this.apiUrl}/extract`, { url });
    }
}

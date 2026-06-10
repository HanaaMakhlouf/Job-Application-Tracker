import { Component } from '@angular/core';
import { JobApplicationsComponent } from './components/job-applications/job-applications.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [JobApplicationsComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App { }

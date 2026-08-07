import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () =>
            import('./components/job-applications/job-applications.component').then(
                m => m.JobApplicationsComponent
            ),
        canActivate: [authGuard]
    },
    {
        path: 'login',
        loadComponent: () =>
            import('./components/login/login.component').then(m => m.LoginComponent)
    },
    {
        path: 'register',
        loadComponent: () =>
            import('./components/register/register.component').then(m => m.RegisterComponent)
    },
    { path: '**', redirectTo: '' }
];

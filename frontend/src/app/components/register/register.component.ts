import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'app-register',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterLink],
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
    name = '';
    email = '';
    password = '';
    error = '';
    loading = false;

    constructor(private auth: AuthService, private router: Router) {}

    submit(): void {
        if (!this.name || !this.email || !this.password) {
            this.error = 'Please fill in all fields';
            return;
        }

        if (this.password.length < 6) {
            this.error = 'Password must be at least 6 characters';
            return;
        }

        this.loading = true;
        this.error = '';

        this.auth.register({ name: this.name, email: this.email, password: this.password }).subscribe({
            next: () => this.router.navigate(['/']),
            error: (err) => {
                this.error = err.status === 409
                    ? 'An account with this email already exists'
                    : 'Something went wrong. Please try again.';
                this.loading = false;
            }
        });
    }
}

import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterLink],
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss']
})
export class LoginComponent {
    email = '';
    password = '';
    error = '';
    loading = false;

    constructor(private auth: AuthService, private router: Router) {}

    submit(): void {
        if (!this.email || !this.password) {
            this.error = 'Please fill in all fields';
            return;
        }

        this.loading = true;
        this.error = '';

        this.auth.login(this.email, this.password).subscribe({
            next: () => this.router.navigate(['/']),
            error: (err) => {
                this.error = err.status === 401
                    ? 'Invalid email or password'
                    : 'Something went wrong. Please try again.';
                this.loading = false;
            }
        });
    }
}

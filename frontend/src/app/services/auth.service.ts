import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { Router } from '@angular/router';
import { from, Observable } from 'rxjs';
import type { AuthResponse, Session } from '@supabase/supabase-js';
import { createSupabaseClient } from './supabase-client';

@Injectable({ providedIn: 'root' })
export class AuthService {
    private readonly platformId = inject(PLATFORM_ID);
    private readonly router = inject(Router);
    private readonly supabase = createSupabaseClient(this.platformId);

    login(email: string, password: string): Observable<AuthResponse> {
        return from(this.supabase.auth.signInWithPassword({ email, password }));
    }

    register(email: string, password: string, name: string): Observable<AuthResponse> {
        return from(this.supabase.auth.signUp({ email, password, options: { data: { name } } }));
    }

    async logout(): Promise<void> {
        await this.supabase.auth.signOut();
        this.router.navigate(['/login']);
    }

    getSession(): Observable<{ data: { session: Session | null } }> {
        return from(this.supabase.auth.getSession());
    }

    async getToken(): Promise<string | null> {
        const { data } = await this.supabase.auth.getSession();
        return data.session?.access_token ?? null;
    }

    async getCurrentUser(): Promise<{ name: string; email: string } | null> {
        const { data } = await this.supabase.auth.getSession();
        const user = data.session?.user;
        return user ? { name: user.user_metadata?.['name'] ?? '', email: user.email ?? '' } : null;
    }
}

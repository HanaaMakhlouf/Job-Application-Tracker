import { createClient, SupabaseClient } from '@supabase/supabase-js';
import { isPlatformBrowser } from '@angular/common';
import { SUPABASE_URL, SUPABASE_ANON_KEY } from '../supabase.config';

export function createSupabaseClient(platformId: object): SupabaseClient {
    const isBrowser = isPlatformBrowser(platformId);

    return createClient(SUPABASE_URL, SUPABASE_ANON_KEY, {
        auth: {
            persistSession: isBrowser,
            autoRefreshToken: isBrowser,
            detectSessionInUrl: isBrowser,
        },
    });
}

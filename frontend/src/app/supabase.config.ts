// Supabase's anon/public key is safe to ship in client code by design
// (rate-limited, meant to be public) — unlike a DB password or JWT secret,
// it does not need to be hidden via user-secrets/environment variables.
export const SUPABASE_URL = 'https://fvaiqrabivwsllfgmexh.supabase.co';
export const SUPABASE_ANON_KEY = 'sb_publishable_1NZcrvgve-fvgochgFklWA_JEl4uv9M';

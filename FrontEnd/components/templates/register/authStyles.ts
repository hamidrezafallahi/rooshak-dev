/**
 * One source of truth for the sign-in and sign-up forms so they can never drift apart.
 * Every colour is a theme token (see tailwind.config.ts / ThemeSettings in the database).
 */
export const authSubtitle = 'font-medium text-store-text text-xl';

export const authLabel = 'block mb-2 font-medium text-store-text text-sm';

export const authInput =
  'block bg-store-muted p-2.5 border border-store-border focus:border-store-strong rounded-lg w-full text-store-text placeholder:text-store-subtle text-sm shadow-none focus-visible:ring-0 focus-visible:ring-offset-0';

export const authInputError = 'border-error focus:border-error';

export const authError = 'mt-1 text-error text-sm';

export const authPrimaryButton =
  'bg-primary hover:bg-primary/90 disabled:opacity-50 px-5 py-2.5 rounded-lg w-full font-medium text-primary-foreground text-sm text-center';

export const authFooter = 'font-medium text-store-subtle text-sm text-center';

export const authFooterLink = 'text-store-text hover:underline';

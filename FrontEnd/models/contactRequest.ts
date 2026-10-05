/** Body of the public contact-request form (cooperation / purchase consultation). */
export interface IContactRequestInput {
  firstName: string;
  lastName: string;
  phoneNumber: string;
  email?: string;
  address?: string;
  preferredContactTime?: string;
  message?: string;
  /** Honeypot — must stay empty for real visitors. */
  website?: string;
}

export interface IContactRequest extends IContactRequestInput {
  id: number;
  isActive: boolean;
  isReviewed: boolean;
  adminNote?: string | null;
  createdAt: string;
}

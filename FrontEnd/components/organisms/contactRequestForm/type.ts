import { IContactRequestInput } from '@models/contactRequest';

export type ContactRequestFormValues = Required<IContactRequestInput>;

export interface ContactRequestFormProps {
  /**
   * next-intl namespace holding the form texts.
   * The same form is "cooperation with us" in one shop and "purchase consultation" in another,
   * so each site only swaps the messages.
   */
  namespace?: string;
  className?: string;
}

import { ReactNode } from 'react';

export interface FormGeneratorProps {
  entityFormConfig: IFormConfig | null;
  defaultValues?: Record<string, any>;
  embedded?: boolean;
}
export interface IFormConfig{
    endPoint: string;
    englishDisplayName: string;
    entityIconBase64: string;
    entityName: string;
    formFieldsJson: string;
    persianDisplayName: string;
  }

export interface FormFieldRendererProps {
    field: FormField;
    register: any;
    error?: string;
    setValue:(name:string,value:unknown)=>void
    getValues:(name:string)=>any;
    defaultValues:Record<string,any>|undefined
    watch:(name:string)=>any;
    trigger:(name:string)=>any;
  }
  
export interface FormFieldRule {
    Rule: "required" | "min" | "max";
    Condition: boolean;
    Message: string;
  }
  
export interface FormField {
    Name: string;
    Caption: string;
    Type: string;
    PlaceHolder?: string;
    Help?: string;
    Order?: number;
    Options?: { label: string; value: string | number }[];
    FetchConfig?: { api: string; fetchFilters:string[] };
    Rules?: FormFieldRule[];
  }
  export interface IFormHeaderProps {
  DisplayName: string;
  icon: ReactNode;
  isEdit: boolean;
  resetField: () => void;
  backHref?: string;
}
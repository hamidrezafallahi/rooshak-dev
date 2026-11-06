export interface config {
  id: number;
  title: string;
  content: string,
  thumbnailUrl: string,
  slug: string,
  authorId: number
}

export interface configRequest {
  config:string
}
export interface configResponse {
  config: string;
}


interface  menuItem {
      entityName:string
      persianDisplayName:string
      englishDisplayName:string
      endPoint:string 
      entityIconBase64:string 
    }
export interface menuResponse {
  data: menuItem[];
  error?: string;
}



export interface IBaseQueryResponse<T> {
     isSuccess:boolean
     error: string
     data:T
}
 
 
export interface IBaseRequest<K> {
  
}

export interface IService<T, K = unknown> {
  url: string;
  body?: IBaseRequest<K>;
  method?: "GET" | "POST" | "DELETE" | "PUT"
  skip?: boolean;
  id?: string | number;
  /**
   * Reads answer isSuccess false both for "nothing found" and for real refusals, so only
   * a caller that cannot go on without this data asks to have the reason shown.
   */
  reportFailure?: boolean;
}


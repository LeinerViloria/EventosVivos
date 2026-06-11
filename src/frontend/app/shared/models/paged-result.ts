/** A server-paginated result: the page of items plus the total number of matches. */
export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}

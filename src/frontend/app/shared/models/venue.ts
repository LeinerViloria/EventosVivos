/** A venue option returned by GET /api/v1/venues/search for selectors. */
export interface VenueSearchItem {
  id: string;
  name: string;
  capacity: number;
  city: string;
}

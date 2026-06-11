import { EventType } from '../enums/event-type';
import { EventStatus } from '../enums/event-status';

/** A row of the events listing (GET /api/v1/events). Dates arrive as UTC ISO 8601 strings. */
export interface EventListItem {
  id: string;
  title: string;
  venueId: string;
  venueName: string;
  startUtc: string;
  endUtc: string;
  maxCapacity: number;
  reservedTickets: number;
  price: number;
  type: EventType;
  status: EventStatus;
}

/** Payload for POST /api/v1/events. Dates travel as ISO 8601 with the client's offset. */
export interface CreateEventRequest {
  title: string;
  description: string;
  venueId: string;
  maxCapacity: number;
  startsAt: string;
  endsAt: string;
  price: number;
  type: EventType;
}

export interface CreateEventResponse {
  id: string;
}

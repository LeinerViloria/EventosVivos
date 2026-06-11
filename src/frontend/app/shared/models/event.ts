import { EventType } from '../enums/event-type';

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

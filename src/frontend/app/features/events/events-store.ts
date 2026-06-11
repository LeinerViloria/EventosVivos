import { inject, Injectable } from '@angular/core';
import { HttpClient, httpResource } from '@angular/common/http';
import { API_BASE_URL } from '@core/api-base-url';
import { VenueSearchItem } from '@shared/models/venue';
import { CreateEventRequest, CreateEventResponse } from '@shared/models/event';
import { CreateReservationRequest, CreateReservationResponse } from '@shared/models/reservation';

@Injectable({ providedIn: 'root' })
export class EventsStore {
  private readonly http = inject(HttpClient);
  private readonly apiBase = inject(API_BASE_URL);

  /** Venues for the selector (reactive read). */
  readonly venues = httpResource<VenueSearchItem[]>(() => `${this.apiBase}/venues/search`, {
    defaultValue: [],
  });

  /** Command: create an event. */
  createEvent(request: CreateEventRequest) {
    return this.http.post<CreateEventResponse>(`${this.apiBase}/events`, request);
  }

  /** Command: reserve tickets for an event. */
  createReservation(request: CreateReservationRequest) {
    return this.http.post<CreateReservationResponse>(`${this.apiBase}/reservations`, request);
  }
}

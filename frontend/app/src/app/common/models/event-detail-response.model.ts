import { EventStatus } from '../enums/event-status';

export interface EventDetailResponse {
  id: number;
  name: string;
  description: string;
  eventTypeId: number;
  eventTypeName: string;
  eventStatusId: EventStatus;
  eventStatusName: string;
  venueId: number;
  venueName: string;
  maxCapacity: number;
  availableTickets: number;
  startDate: string;
  endDate: string;
  price: number;
}

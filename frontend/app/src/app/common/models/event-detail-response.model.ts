import { EventStatus } from '../enums/event-status';
import { EventType } from '../enums/event-type';

export interface EventDetailResponse {
  id: number;
  name: string;
  description: string;
  eventTypeId: EventType;
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

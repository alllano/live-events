import { EventStatus } from '../enums/event-status';

export interface EventFilterRequest {
  eventTypeId?: number;
  startDateFrom?: string;
  startDateTo?: string;
  venueId?: number;
  eventStatusId?: EventStatus;
  titleSearch?: string;
}

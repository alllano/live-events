import { EventStatus } from '../enums/event-status';
import { EventType } from '../enums/event-type';

export interface EventFilterRequest {
  eventTypeId?: EventType;
  startDateFrom?: string;
  startDateTo?: string;
  venueId?: number;
  eventStatusId?: EventStatus;
  titleSearch?: string;
}

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../environments/environment';
import { EventStatus } from '../common/enums/event-status';
import { EventType } from '../common/enums/event-type';
import { CreateEventRequest } from '../common/models/create-event-request.model';
import { EventDetailResponse } from '../common/models/event-detail-response.model';
import { EventFilterRequest } from '../common/models/event-filter-request.model';
import { EventListItemResponse } from '../common/models/event-list-item-response.model';
import { EventService } from './event.service';

describe('EventService', () => {
  let eventService: EventService;
  let httpTesting: HttpTestingController;

  const sampleEventDetail: EventDetailResponse = {
    id: 1,
    name: 'Sample Conference',
    description: 'A sample event used for testing.',
    eventTypeId: EventType.Conference,
    eventTypeName: 'Conference',
    eventStatusId: EventStatus.Active,
    eventStatusName: 'Active',
    venueId: 1,
    venueName: 'Auditorio Central',
    maxCapacity: 100,
    availableTickets: 100,
    startDate: '2026-07-01T10:00:00',
    endDate: '2026-07-01T12:00:00',
    price: 50,
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    eventService = TestBed.inject(EventService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('createEvent_WhenCalled_PostsToEventsUrlWithRequestBodyAndUnwrapsResponse', () => {
    const request: CreateEventRequest = {
      name: 'Sample Conference',
      description: 'A sample event used for testing.',
      venueId: 1,
      maxCapacity: 100,
      startDate: '2026-07-01T10:00:00',
      endDate: '2026-07-01T12:00:00',
      price: 50,
      eventTypeId: EventType.Conference,
    };

    let result: EventDetailResponse | undefined;
    eventService.createEvent(request).subscribe((response) => (result = response));

    const httpRequest = httpTesting.expectOne(`${environment.apiUrl}/events`);
    expect(httpRequest.request.method).toBe('POST');
    expect(httpRequest.request.body).toEqual(request);

    httpRequest.flush({ success: true, data: sampleEventDetail, statusCode: 201, message: null });

    expect(result).toEqual(sampleEventDetail);
  });

  it('getEventById_WhenCalled_GetsExactUrlWithId', () => {
    let result: EventDetailResponse | undefined;
    eventService.getEventById(1).subscribe((response) => (result = response));

    const httpRequest = httpTesting.expectOne(`${environment.apiUrl}/events/1`);
    expect(httpRequest.request.method).toBe('GET');

    httpRequest.flush({ success: true, data: sampleEventDetail, statusCode: 200, message: null });

    expect(result).toEqual(sampleEventDetail);
  });

  it('getEvents_WhenFilterHasUndefinedFields_OmitsThemFromQueryParamsAndUpdatesEventsSignal', () => {
    const filter: EventFilterRequest = {
      titleSearch: 'Conference',
      eventTypeId: undefined,
      venueId: undefined,
      eventStatusId: undefined,
      startDateFrom: undefined,
      startDateTo: undefined,
    };

    const sampleEventList: EventListItemResponse[] = [
      {
        id: 1,
        name: 'Sample Conference',
        eventTypeName: 'Conference',
        eventStatusName: 'Active',
        venueName: 'Auditorio Central',
        startDate: '2026-07-01T10:00:00',
        price: 50,
      },
    ];

    let result: EventListItemResponse[] | undefined;
    eventService.getEvents(filter).subscribe((response) => (result = response));

    const httpRequest = httpTesting.expectOne(
      (req) => req.url === `${environment.apiUrl}/events` && req.method === 'GET'
    );

    expect(httpRequest.request.params.keys()).toEqual(['titleSearch']);
    expect(httpRequest.request.params.get('titleSearch')).toBe('Conference');

    httpRequest.flush({ success: true, data: sampleEventList, statusCode: 200, message: null });

    expect(result).toEqual(sampleEventList);
    expect(eventService.events()).toEqual(sampleEventList);
  });
});

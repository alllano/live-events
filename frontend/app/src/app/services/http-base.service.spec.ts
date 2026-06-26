import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../environments/environment';
import { HttpBaseService } from './http-base.service';

describe('HttpBaseService', () => {
  let httpBaseService: HttpBaseService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    httpBaseService = TestBed.inject(HttpBaseService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('get_WhenResponseIsReceived_UnwrapsResponseDTODataField', () => {
    interface SampleData {
      name: string;
    }

    let result: SampleData | undefined;
    httpBaseService.get<SampleData>('sample').subscribe((data) => (result = data));

    const request = httpTesting.expectOne(`${environment.apiUrl}/sample`);
    request.flush({ success: true, data: { name: 'test' }, statusCode: 200, message: null });

    expect(result).toEqual({ name: 'test' });
  });

  it('get_WhenEndpointHasNoLeadingSlash_BuildsUrlWithExactlyOneSeparatorSlash', () => {
    httpBaseService.get('sample').subscribe();

    const request = httpTesting.expectOne(`${environment.apiUrl}/sample`);
    expect(request.request.url).toBe(`${environment.apiUrl}/sample`);
    request.flush({ success: true, data: null, statusCode: 200, message: null });
  });

  it('get_WhenEndpointHasLeadingSlash_BuildsUrlWithExactlyOneSeparatorSlash', () => {
    httpBaseService.get('/sample').subscribe();

    const request = httpTesting.expectOne(`${environment.apiUrl}/sample`);
    expect(request.request.url).toBe(`${environment.apiUrl}/sample`);
    request.flush({ success: true, data: null, statusCode: 200, message: null });
  });
});

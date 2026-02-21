import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EndpointDetail } from './endpoint-detail';

describe('EndpointDetail', () => {
  let component: EndpointDetail;
  let fixture: ComponentFixture<EndpointDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EndpointDetail]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EndpointDetail);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

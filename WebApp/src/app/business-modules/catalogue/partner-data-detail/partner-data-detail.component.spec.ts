import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PartnerDataDetailComponent } from './partner-data-detail.component';

describe('PartnerDataDetailComponent', () => {
  let component: PartnerDataDetailComponent;
  let fixture: ComponentFixture<PartnerDataDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PartnerDataDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PartnerDataDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

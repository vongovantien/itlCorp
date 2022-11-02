import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FormSearchTrackingComponent } from './form-search-tracking.component';

describe('FormSearchTrackingComponent', () => {
  let component: FormSearchTrackingComponent;
  let fixture: ComponentFixture<FormSearchTrackingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FormSearchTrackingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FormSearchTrackingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

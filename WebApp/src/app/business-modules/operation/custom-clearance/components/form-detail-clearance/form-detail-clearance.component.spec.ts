import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FormDetailClearanceComponent } from './form-detail-clearance.component';

describe('FormDetailClearanceComponent', () => {
  let component: FormDetailClearanceComponent;
  let fixture: ComponentFixture<FormDetailClearanceComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FormDetailClearanceComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FormDetailClearanceComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

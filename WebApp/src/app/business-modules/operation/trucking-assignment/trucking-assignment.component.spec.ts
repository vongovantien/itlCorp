import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TruckingAssignmentComponent } from './trucking-assignment.component';

describe('TruckingAssigmentComponent', () => {
  let component: TruckingAssignmentComponent;
  let fixture: ComponentFixture<TruckingAssignmentComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TruckingAssignmentComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TruckingAssignmentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

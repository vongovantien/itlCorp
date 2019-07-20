import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { NotSelectedAlertModalComponent } from './not-selected-alert-modal.component';

describe('NotSelectedAlertModalComponent', () => {
  let component: NotSelectedAlertModalComponent;
  let fixture: ComponentFixture<NotSelectedAlertModalComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ NotSelectedAlertModalComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(NotSelectedAlertModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

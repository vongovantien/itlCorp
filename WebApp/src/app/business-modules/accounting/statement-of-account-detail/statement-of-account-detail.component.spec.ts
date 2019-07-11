import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StatementOfAccountDetailComponent } from './statement-of-account-detail.component';

describe('StatementOfAccountDetailComponent', () => {
  let component: StatementOfAccountDetailComponent;
  let fixture: ComponentFixture<StatementOfAccountDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StatementOfAccountDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StatementOfAccountDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

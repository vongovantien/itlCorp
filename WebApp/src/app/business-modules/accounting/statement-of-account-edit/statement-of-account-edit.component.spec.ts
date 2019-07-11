import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StatementOfAccountEditComponent } from './statement-of-account-edit.component';

describe('StatementOfAccountEditComponent', () => {
  let component: StatementOfAccountEditComponent;
  let fixture: ComponentFixture<StatementOfAccountEditComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StatementOfAccountEditComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StatementOfAccountEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

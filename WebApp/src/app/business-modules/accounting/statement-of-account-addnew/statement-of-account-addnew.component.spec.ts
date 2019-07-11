import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StatementOfAccountAddnewComponent } from './statement-of-account-addnew.component';

describe('StatementOfAccountAddnewComponent', () => {
  let component: StatementOfAccountAddnewComponent;
  let fixture: ComponentFixture<StatementOfAccountAddnewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StatementOfAccountAddnewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StatementOfAccountAddnewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

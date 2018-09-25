import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SalesExecutiveComponent } from './sales-executive.component';

describe('SalesExecutiveComponent', () => {
  let component: SalesExecutiveComponent;
  let fixture: ComponentFixture<SalesExecutiveComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SalesExecutiveComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SalesExecutiveComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

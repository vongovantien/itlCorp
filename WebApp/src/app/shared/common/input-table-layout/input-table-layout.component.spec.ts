import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { InputTableLayoutComponent } from './input-table-layout.component';

describe('InputTableLayoutComponent', () => {
  let component: InputTableLayoutComponent;
  let fixture: ComponentFixture<InputTableLayoutComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ InputTableLayoutComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(InputTableLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

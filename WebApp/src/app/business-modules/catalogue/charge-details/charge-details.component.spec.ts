import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChargeDetailsComponent } from './charge-details.component';

describe('ChargeDetailsComponent', () => {
  let component: ChargeDetailsComponent;
  let fixture: ComponentFixture<ChargeDetailsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChargeDetailsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChargeDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

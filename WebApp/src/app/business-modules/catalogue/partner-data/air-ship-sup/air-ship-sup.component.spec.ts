import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AirShipSupComponent } from './air-ship-sup.component';

describe('AirShipSupComponent', () => {
  let component: AirShipSupComponent;
  let fixture: ComponentFixture<AirShipSupComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AirShipSupComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AirShipSupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ShippingInstructionComponent } from './shipping-instruction.component';

describe('ShippingInstructionComponent', () => {
  let component: ShippingInstructionComponent;
  let fixture: ComponentFixture<ShippingInstructionComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ShippingInstructionComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ShippingInstructionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

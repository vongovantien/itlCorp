import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaLclExportShippingInstructionComponent } from './sea-lcl-export-shipping-instruction.component';

describe('SeaLclExportShippingInstructionComponent', () => {
  let component: SeaLclExportShippingInstructionComponent;
  let fixture: ComponentFixture<SeaLclExportShippingInstructionComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaLclExportShippingInstructionComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaLclExportShippingInstructionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

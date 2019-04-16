import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaLclExportShipmentDetailComponent } from './sea-lcl-export-shipment-detail.component';

describe('SeaLclExportShipmentDetailComponent', () => {
  let component: SeaLclExportShipmentDetailComponent;
  let fixture: ComponentFixture<SeaLclExportShipmentDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaLclExportShipmentDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaLclExportShipmentDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

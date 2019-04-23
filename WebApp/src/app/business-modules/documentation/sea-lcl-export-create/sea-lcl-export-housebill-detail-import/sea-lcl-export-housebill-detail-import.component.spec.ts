import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaLclExportHousebillDetailImportComponent } from './sea-lcl-export-housebill-detail-import.component';

describe('SeaLclExportHousebillDetailImportComponent', () => {
  let component: SeaLclExportHousebillDetailImportComponent;
  let fixture: ComponentFixture<SeaLclExportHousebillDetailImportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaLclExportHousebillDetailImportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaLclExportHousebillDetailImportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

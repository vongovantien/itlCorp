import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaLclExportDetailImportComponent } from './sea-lcl-export-detail-import.component';

describe('SeaLclExportDetailImportComponent', () => {
  let component: SeaLclExportDetailImportComponent;
  let fixture: ComponentFixture<SeaLclExportDetailImportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaLclExportDetailImportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaLclExportDetailImportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

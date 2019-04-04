import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaFclExportDetailImportComponent } from './sea-fcl-export-detail-import.component';

describe('SeaFclExportDetailImportComponent', () => {
  let component: SeaFclExportDetailImportComponent;
  let fixture: ComponentFixture<SeaFclExportDetailImportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaFclExportDetailImportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaFclExportDetailImportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

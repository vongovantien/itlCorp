import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaLclExportHousebillAddnewComponent } from './sea-lcl-export-housebill-addnew.component';

describe('SeaLclExportHousebillAddnewComponent', () => {
  let component: SeaLclExportHousebillAddnewComponent;
  let fixture: ComponentFixture<SeaLclExportHousebillAddnewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaLclExportHousebillAddnewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaLclExportHousebillAddnewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

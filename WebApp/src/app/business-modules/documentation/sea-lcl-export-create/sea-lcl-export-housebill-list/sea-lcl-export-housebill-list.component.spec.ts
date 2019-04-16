import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaLclExportHousebillListComponent } from './sea-lcl-export-housebill-list.component';

describe('SeaLclExportHousebillListComponent', () => {
  let component: SeaLclExportHousebillListComponent;
  let fixture: ComponentFixture<SeaLclExportHousebillListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaLclExportHousebillListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaLclExportHousebillListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

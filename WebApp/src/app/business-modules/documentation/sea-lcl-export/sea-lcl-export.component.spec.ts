import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaLCLExportComponent } from './sea-lcl-export.component';

describe('SeaLCLExportComponent', () => {
  let component: SeaLCLExportComponent;
  let fixture: ComponentFixture<SeaLCLExportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaLCLExportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaLCLExportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaFCLExportComponent } from './sea-fcl-export.component';

describe('SeaFCLExportComponent', () => {
  let component: SeaFCLExportComponent;
  let fixture: ComponentFixture<SeaFCLExportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaFCLExportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaFCLExportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaLCLImportComponent } from './sea-lcl-import.component';

describe('SeaLCLImportComponent', () => {
  let component: SeaLCLImportComponent;
  let fixture: ComponentFixture<SeaLCLImportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaLCLImportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaLCLImportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

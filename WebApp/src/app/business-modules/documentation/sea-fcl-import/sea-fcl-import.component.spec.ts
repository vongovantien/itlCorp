import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaFCLImportComponent } from './sea-fcl-import.component';

describe('SeaFCLImportComponent', () => {
  let component: SeaFCLImportComponent;
  let fixture: ComponentFixture<SeaFCLImportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaFCLImportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaFCLImportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

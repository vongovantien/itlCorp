import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaConsolExportComponent } from './sea-consol-export.component';

describe('SeaConsolExportComponent', () => {
  let component: SeaConsolExportComponent;
  let fixture: ComponentFixture<SeaConsolExportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaConsolExportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaConsolExportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

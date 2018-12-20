import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaFclExportCreateComponent } from './sea-fcl-export-create.component';

describe('SeaFclExportCreateComponent', () => {
  let component: SeaFclExportCreateComponent;
  let fixture: ComponentFixture<SeaFclExportCreateComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaFclExportCreateComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaFclExportCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

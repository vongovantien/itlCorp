import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaLclExportCreateComponent } from './sea-lcl-export-create.component';

describe('SeaLclExportCreateComponent', () => {
  let component: SeaLclExportCreateComponent;
  let fixture: ComponentFixture<SeaLclExportCreateComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaLclExportCreateComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaLclExportCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

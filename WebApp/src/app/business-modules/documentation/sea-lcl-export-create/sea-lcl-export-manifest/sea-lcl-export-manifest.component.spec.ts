import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SeaLclExportManifestComponent } from './sea-lcl-export-manifest.component';

describe('SeaLclExportManifestComponent', () => {
  let component: SeaLclExportManifestComponent;
  let fixture: ComponentFixture<SeaLclExportManifestComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SeaLclExportManifestComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SeaLclExportManifestComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
